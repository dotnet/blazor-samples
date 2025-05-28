using Azure.Identity;
using BlazorWebAppEntra;
using BlazorWebAppEntra.Client.Weather;
using BlazorWebAppEntra.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
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
        // Disable L1 Cache default: false
        //options.DisableL1Cache = false;
        
        // L1 Cache Size Limit default: 500 MB
        //options.L1CacheOptions.SizeLimit = 500 * 1024 * 1024;
        
        // Encrypt tokens at rest default: false
        options.Encrypt = true;
        
        // Sliding Expiration default: 1 hour
        //options.SlidingExpiration = TimeSpan.FromHours(1);
    });

/* When you move to web farm testing with tokens encrypted at rest while
 * hosting multiple app instances and stop using AddDistributedMemoryCache 
 * in favor of a production distributed token cache provider, enable the 
 * following code, which configures Data Protection to protect keys with 
 * Azure Key Vault and maintain keys in Azure Blob Storage.
 * Other options, both within Azure and outside of Azure, are available for
 * managing Data Protection keys across multiple app instances. See the 
 * ASP.NET Core Data Protection documentation for details.

// Requires the Microsoft.Extensions.Azure NuGet package
builder.Services.TryAddSingleton<AzureEventSourceLogForwarder>();

builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(new Uri("{BLOB URI WITH SAS TOKEN}"))
    .ProtectKeysWithAzureKeyVault(new Uri("{KEY IDENTIFIER}"), new DefaultAzureCredential());
*/

builder.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme).Configure(oidcOptions =>
{
    oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);
});

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

// Remove or set 'SerializeAllClaims' to 'false' if you only want to 
// serialize name and role claims for CSR.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

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
