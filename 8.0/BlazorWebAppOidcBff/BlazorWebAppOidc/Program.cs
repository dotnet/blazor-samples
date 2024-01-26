using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Yarp.ReverseProxy.Transforms;
using BlazorWebAppOidc;
using BlazorWebAppOidc.Client.Weather;
using BlazorWebAppOidc.Components;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAuthentication("MicrosoftOidc")
    .AddOpenIdConnect("MicrosoftOidc", oidcOptions =>
    {
        // For the following OIDC settings, any line that's commented out
        // represents a DEFAULT setting. If you adopt the default, you can
        // remove the line if you wish.

        // ........................................................................
        // The OIDC handler must use a sign-in scheme capable of persisting 
        // user credentials across requests.

        oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // ........................................................................

        // ........................................................................
        // The "openid" and "profile" scopes are required for the OIDC handler 
        // and included by default. You should enable these scopes here if scopes 
        // are provided by "Authentication:Schemes:MicrosoftOidc:Scope" 
        // configuration because configuration may overwrite the scopes collection.

        //oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        // ........................................................................

        // ........................................................................
        // SaveTokens is set to true to save the access and refresh tokens in the 
        // cookie, so the app can authenticate requests for weather data.

        oidcOptions.SaveTokens = true;
        // ........................................................................

        // ........................................................................
        // The following paths must match the redirect and post logout redirect 
        // paths configured when registering the application with the OIDC provider. 
        // For Microsoft Entra ID, this is accomplished through the "Authentication" 
        // blade of the application's registration in the Azure portal. Both the
        // signin and signout paths must be registered as Redirect URIs. The default 
        // values are "/signin-oidc" and "/signout-callback-oidc".
        // Microsoft Identity currently only redirects back to the 
        // SignedOutCallbackPath if authority is 
        // https://login.microsoftonline.com/{TENANT ID}/v2.0/ as it is above. 
        // You can use the "common" authority instead, and logout redirects back to 
        // the Blazor app. For more information, see 
        // https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/5783

        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        // ........................................................................
        
        // ........................................................................
        // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
        // sign-out. The default value is "/signout-oidc".

        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
        // ........................................................................

        // ........................................................................
        // The "offline_access" scope is required for the refresh token.

        oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);
        // ........................................................................

        // ........................................................................
        // The "Weather.Get" scope is configured in the Azure or Entra portal under 
        // "Expose an API". This is necessary for backend web API (MinimalApiJwt)
        // to validate the access token with AddBearerJwt.

        oidcOptions.Scope.Add("https://{DIRECTORY NAME}.onmicrosoft.com/{CLIENT ID}/Weather.Get");
        // ........................................................................

        // ........................................................................
        // The following example Authority is configured for Microsoft Entra ID
        // and a single-tenant application registration. Set the {TENANT ID} 
        // placeholder to the Tenant ID. The "common" Authority 
        // https://login.microsoftonline.com/common/v2.0/ should be used 
        // for multi-tenant apps. You can also use the "common" Authority for 
        // single-tenant apps, but it requires a custom IssuerValidator as shown 
        // in the comments below. 

        oidcOptions.Authority = "https://login.microsoftonline.com/{TENANT ID}/v2.0/";
        // ........................................................................

        // ........................................................................
        // Set the Client ID for the app. Set the {CLIENT ID} placeholder to
        // the Client ID.

        oidcOptions.ClientId = "{CLIENT ID}";
        // ........................................................................
        
        // ........................................................................
        // ClientSecret shouldn't be compiled into the application assembly or 
        // checked into source control. Instead adopt User Secrets, Azure KeyVault, 
        // or an environment variable to supply the value. Authentication scheme 
        // configuration is automatically read from 
        // "Authentication:Schemes:{SchemeName}:{PropertyName}", so ClientSecret is 
        // for OIDC configuration is automatically read from 
        // "Authentication:Schemes:MicrosoftOidc:ClientSecret" configuration.

        //oidcOptions.ClientSecret = "{PREFER NOT SETTING THIS HERE}";
        // ........................................................................

        // ........................................................................
        // Setting ResponseType to "code" configures the OIDC handler to use 
        // authorization code flow. Implicit grants and hybrid flows are unnecessary
        // in this mode. In a Microsoft Entra ID app registration, you don't need to 
        // select either box for the authorization endpoint to return access tokens 
        // or ID tokens. The OIDC handler automatically requests the appropriate 
        // tokens using the code returned from the authorization endpoint.

        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        // ........................................................................

        // ........................................................................
        // Many OIDC servers use "name" and "role" rather than the SOAP/WS-Fed 
        // defaults in ClaimTypes. If you don't use ClaimTypes, mapping inbound 
        // claims to ASP.NET Core's ClaimTypes isn't necessary.

        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        oidcOptions.TokenValidationParameters.RoleClaimType = "role";
        // ........................................................................

        // ........................................................................
        // Many OIDC providers work with the default issuer validator, but the
        // configuration must account for the issuer parameterized with "{TENANT ID}" 
        // returned by the "common" endpoint's /.well-known/openid-configuration
        // For more information, see
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731

        //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
        //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
        // ........................................................................
    })
    .AddCookie("Cookies");

// This attaches a cookie OnValidatePrincipal callback to get a new access token when the current one expires, and
// reissue a cookie with the new access token saved inside. If the refresh fails, the user will be signed out.
builder.Services.ConfigureCookieOidcRefresh("Cookies", "MicrosoftOidc");

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapDefaultEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppOidc.Client._Imports).Assembly);

app.MapForwarder("/weatherforecast", "https://weatherapi", transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
