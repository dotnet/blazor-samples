using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using BlazorWebAppEntra.Client.Weather;
using BlazorWebAppEntra.Components;
using BlazorWebAppEntra.Weather;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Remove or set 'SerializeAllClaims' to 'false' if you only want to 
// serialize name and role claims for CSR.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(msIdentityOptions =>
    {
        msIdentityOptions.CallbackPath = "/signin-oidc";
        msIdentityOptions.ClientId = "{CLIENT ID (BLAZOR APP)}";
        msIdentityOptions.Domain = "{DIRECTORY NAME}.onmicrosoft.com";
        msIdentityOptions.Instance = "https://login.microsoftonline.com/";
        msIdentityOptions.ResponseType = "code";
        msIdentityOptions.TenantId = "{TENANT ID}";
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("DownstreamApi", configOptions =>
    {
        configOptions.BaseUrl = "{BASE URL}";
        configOptions.Scopes = [ "{APP ID URI}/Weather.Get" ];
    })
    .AddDistributedTokenCaches();

builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<MsalDistributedTokenCacheAdapterOptions>(
    options =>
    {
        options.DisableL1Cache = false;
        options.L1CacheOptions.SizeLimit = 1024 * 1024 * 1024;
        options.Encrypt = true;
        options.SlidingExpiration = TimeSpan.FromHours(1);
    });

builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(new Uri("{BLOB URI WITH SAS TOKEN}"))
    .ProtectKeysWithAzureKeyVault(new Uri("{KEY IDENTIFIER}"), new DefaultAzureCredential());

builder.Services.AddAuthorization();

builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();

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

app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/weather-forecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppEntra.Client._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
