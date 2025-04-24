using Yarp.ReverseProxy.Transforms;
using BlazorWebAppEntra;
using BlazorWebAppEntra.Client.Weather;
using BlazorWebAppEntra.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme).Configure(oidcOptions =>
{
    oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);
});

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new("https://weatherapi");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapDefaultEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppEntra.Client._Imports).Assembly);

app.MapForwarder("/weather-forecast", "https://weatherapi", transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var tokenAcquisition = transformContext.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
        var configuration = transformContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var scopes = configuration.GetSection("DownstreamApi:Scopes").Get<IEnumerable<string>>();
        var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes ??
            throw new IOException("No downstream API scopes!"));
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
