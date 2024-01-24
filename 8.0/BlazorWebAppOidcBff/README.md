# Blazor Web App with OpenID Connect (OIDC) (BFF Pattern)

This sample features:

- A Blazor Web App with global Auto interactivity.
  - This adds a `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services to the
    server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
- OIDC authentication with Microsoft Entra without using Entra-specific packages.
  - The goal is that this sample can be used as a starting point for any OIDC authentication flow.
- A minimal API backend using the JwtBearerHandler to validate JWT tokens saved by the Blazor app in the sign-in cookie.
- The BFF pattern using Aspire service discovery and YARP for proxying the requests to `/weatherforecast` on the backend with the `access_token` stored in the cookie.
- Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.

## Configure the sample

Configure the OIDC provider. If using Microsoft Entra, you follow along with the comments in Program.cs. here's an excerpt:

```csharp
// Save the access and refresh tokens in the cookie, so we can authenticate requests to the "weatherapi" service.
// The offline_access scope is required for the refresh token.
oidcOptions.SaveTokens = true;
oidcOptions.Scope.Add("offline_access");
// The "Weather.Get" scope is configured in the Azure or Entra portal under "Expose an API".
// This is necessary for MinimalApiJwt to be able to validate the access token with AddBearerJwt.
oidcOptions.Scope.Add("https://{directory-name}.onmicrosoft.com/{client-id}/Weather.Get");

// The "common" authority should be used for multi-tenant applications. You can also use the common
// authority for single-tenant applications, but that requires a custom IssuerValidator as shown in the comments below.
//oidcOptions.Authority = "https://login.microsoftonline.com/common/v2.0/";
oidcOptions.Authority = "https://login.microsoftonline.com/{tenant-id}/v2.0/";
oidcOptions.ClientId = "{client-id}";

// ClientSecret should not be compiled into the application assembly or checked into source control.
// Instead consider user-secrets, Azure KeyVault and/or environment variables. Authentication scheme configuration
// is automatically read from builder.Configuration["Authentication:Schemes:{SchemeName}:{PropertyName}"],
// so ClientSecret will be read from "Authentication:Schemes:MicrosoftOidc:ClientSecret" configuration.
//oidcOptions.ClientSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

// This configures the OIDC handeler to do authorization code flow only. Implicit grants and hybrid flows are unnecessary
// in this mode. You do not need to check either box for the authorization endpoint to return access tokens or ID tokens.
// The OIDC handler will automatically request the appropriate tokens using the code returned from the authorization endpoint.
oidcOptions.ResponseType = "code";
```

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppOidc` solution file in Visual Studio.
1. Select the `Aspire/Aspire.AppHost` project in **Solution Explorer** and start the app with either Visual Studio's Run button or by selecting **Start Debugging** from the **Debug** menu.

### .NET CLI

In a command shell, navigate to the `Aspire/Aspire.AppHost` project folder and use the `dotnet run` command to run the sample. The `BlazorWebAppOidc` and `MinimalApiJwt` projects are also started by the command.
