using System.Diagnostics;
using System.Net.Http.Headers;

namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// DelegatingHandler that automatically adds authentication headers to HTTP requests.
/// </summary>
public class AuthorizationMessageHandler(MauiAuthenticationStateProvider authProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessTokenInfo = await authProvider.GetAccessTokenInfoAsync();

        if (accessTokenInfo is null)
        {
            throw new InvalidOperationException("Could not retrieve access token.");
        }

        var token = accessTokenInfo.AccessToken;
        var scheme = accessTokenInfo.TokenType;

        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(scheme))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, token);
        }
        else
        {
            Debug.WriteLine("Token or scheme is null or empty.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
