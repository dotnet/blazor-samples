using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;
using BlazorWebAppOidc;
using BlazorWebAppOidc.Client.Weather;
using BlazorWebAppOidc.Components;

const string MS_OIDC_SCHEME = "MicrosoftOidc";

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAuthentication(MS_OIDC_SCHEME)
    .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
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
        // The following paths must match the redirect and post logout redirect 
        // paths configured when registering the application with the OIDC provider. 
        // The default values are "/signin-oidc" and "/signout-callback-oidc".

        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        // ........................................................................

        // ........................................................................
        // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
        // sign-out. The default value is "/signout-oidc".

        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
        // ........................................................................

        // ........................................................................
        // The "Weather.Get" scope is configured in the Azure or Entra portal under 
        // "Expose an API". This is necessary for backend web API (MinimalApiJwt)
        // to validate the access token with AddBearerJwt. The following code example
        // uses a scope format of the App ID URI for an AAD B2C tenant type. If your
        // tenant is an ME-ID tenant, the App ID URI format is different:
        // api://{CLIENT ID}, so the full scope with an API name of "Weather.Get" is:
        // api://{CLIENT ID}/Weather.Get
        // The {CLIENT ID} is the application (client) ID of the MinimalApiJwt app 
        // registration.

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
        // Setting ResponseType to "code" configures the OIDC handler to use 
        // authorization code flow. Implicit grants and hybrid flows are unnecessary
        // in this mode. In a Microsoft Entra ID app registration, you don't need to 
        // select either box for the authorization endpoint to return access tokens 
        // or ID tokens. The OIDC handler automatically requests the appropriate 
        // tokens using the code returned from the authorization endpoint.

        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        // ........................................................................

        // ........................................................................
        // // Set MapInboundClaims to "false" to obtain the original claim types from 
        // the token. Many OIDC servers use "name" and "role"/"roles" rather than 
        // the SOAP/WS-Fed defaults in ClaimTypes. Adjust these values if your 
        // identity provider uses different claim types.

        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = "name";
        oidcOptions.TokenValidationParameters.RoleClaimType = "roles";
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

        // ........................................................................
        // OIDC connect options set later via ConfigureCookieOidcRefresh
        //
        // (1) The "offline_access" scope is required for the refresh token.
        //
        // (2) SaveTokens is set to true, which saves the access and refresh tokens
        // in the cookie, so the app can authenticate requests for weather data and
        // cookie, so the app can authenticate requests for weather data and
        // use the refresh token to obtain a new access token on access token
        // expiration.
        // ........................................................................
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

// ConfigureCookieOidcRefresh attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. If the refresh fails, the user will be signed
// out. OIDC connect options are set for saving tokens and the offline access
// scope.
builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
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
    .AddAdditionalAssemblies(typeof(BlazorWebAppOidc.Client._Imports).Assembly);

app.MapForwarder("/weather-forecast", "https://weatherapi", transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
