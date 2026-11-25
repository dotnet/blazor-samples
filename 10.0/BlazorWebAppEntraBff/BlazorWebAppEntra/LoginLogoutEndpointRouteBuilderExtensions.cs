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

        group.MapGet("/login", (HttpRequest request, string? returnUrl) => TypedResults.Challenge(GetAuthProperties(request, returnUrl)))
            .AllowAnonymous();

        // Sign out with both the Cookie and OIDC authentication schemes. Users who have not signed out with the OIDC scheme will
        // automatically get signed back in as the same user the next time they visit a page that requires authentication
        // with no opportunity to choose another account.
        group.MapPost("/logout", (HttpRequest request, [FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(request, returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(HttpRequest request, string? returnUrl)
    {
        // The return URL comes from the query string or a form field, which anyone can set.
        // To prevent open redirects, resolve it to an absolute URI and only use it if it's within the app.
        // Appending "/" to the path also accepts the app's root URI without a trailing slash, such as "/app?page=2".
        var baseUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/");
        if (!Uri.TryCreate(baseUri, returnUrl, out var redirectUri)
            || !(redirectUri.GetLeftPart(UriPartial.Path) + "/").StartsWith(baseUri.AbsoluteUri, StringComparison.Ordinal))
        {
            redirectUri = baseUri;
        }

        return new AuthenticationProperties { RedirectUri = redirectUri.AbsoluteUri };
    }
}
