using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Routing;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty);

        group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

        // Sign out with both the Cookie and OIDC authentication schemes. Users who have not signed out with the OIDC scheme will
        // automatically get signed back in as the same user the next time they visit a page that requires authentication
        // with no opportunity to choose another account.
        group.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        return group;
    }

    // Prevent open redirects. Non-empty returnUrls are absolute URIs provided by NavigationManager.Uri.
    private static AuthenticationProperties GetAuthProperties(string? returnUrl) =>
        new()
        {
            RedirectUri = returnUrl switch
            {
                string => new Uri(returnUrl, UriKind.Absolute).PathAndQuery,
                null => "/",
            }
        };
}


"AzureAd": {
  "Domain": "XXXXXXXXXX.onmicrosoft.com",
  "CallbackPath": "/signin-oidc",
  "ClientId": "00001111-aaaa-2222-bbbb-3333cccc4444",
  "Instance": "https://login.microsoftonline.com/",
  "ResponseType": "code",
  "TenantId": "aaaabbbb-0000-cccc-1111-dddd2222eeee",
  "VaultUri": "https://XXXXXXXXXX.vault.azure.net/",
  "SecretName": "XXXXXXXXXX"
},

I can't reproduce that error here, but it looks like you aren't configuring the app and the app's registration correctly.

In the app's registration in Entra ...

* The ID tokens checkbox should ***not*** be selected.

In the app's `AzureAd` config is ...

```json
"AzureAd": {
  "Domain": "XXXXXXXXXX.onmicrosoft.com",
  "CallbackPath": "/signin-oidc",
  "ClientId": "00001111-aaaa-2222-bbbb-3333cccc4444",
  "Instance": "https://login.microsoftonline.com/",
  "ResponseType": "code",
  "TenantId": "aaaabbbb-0000-cccc-1111-dddd2222eeee",
  "VaultUri": "https://XXXXXXXXXX.vault.azure.net/",
  "SecretName": "XXXXXXXXXX"
},
```

... and that assumes that the secret is being supplied via Azure Key Vault. If you're using the Secret Manager Tool locally, you wouldn't have the `VaultUri` and `SecretName` there.
