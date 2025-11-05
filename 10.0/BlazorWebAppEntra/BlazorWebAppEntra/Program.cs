using System.Security.Claims;
using Azure.Core;
using Azure.Identity;
using BlazorWebAppEntra.Client.Weather;
using BlazorWebAppEntra.Components;
using BlazorWebAppEntra.Weather;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

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
 * Our recommended approach for using Azure Blob Storage and Azure Key 
 * Vault is to use an Azure Managed Identity in production.
 * Give the Managed Identity 'Key Vault Crypto User' and 
 * 'Storage Blob Data Contributor' roles. Assign the Managed Identity
 * to the App Service in Settings > Identity > User assigned > Add.
 * Other options, both within Azure and outside of Azure, are available for
 * managing Data Protection keys across multiple app instances. See the 
 * ASP.NET Core Data Protection documentation for details.

// Requires the Microsoft.Extensions.Azure NuGet package
builder.Services.TryAddSingleton<AzureEventSourceLogForwarder>();

TokenCredential? credential;

if (builder.Environment.IsProduction())
{
    credential = new ManagedIdentityCredential("{MANAGED IDENTITY CLIENT ID}");
}
else
{
    // Local development and testing only
    DefaultAzureCredentialOptions options = new()
    {
        // Specify the tenant ID to use the dev credentials when running the app locally
        // in Visual Studio.
        VisualStudioTenantId = "{TENANT ID}",
        SharedTokenCacheTenantId = "{TENANT ID}"
    };

    credential = new DefaultAzureCredential(options);
}

builder.Services.AddDataProtection()
    .SetApplicationName("BlazorWebAppEntra")
    .PersistKeysToAzureBlobStorage(new Uri("{BLOB URI}"), credential)
    .ProtectKeysWithAzureKeyVault( new Uri("{KEY IDENTIFIER}"), credential);
*/

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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
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
