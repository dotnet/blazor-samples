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
