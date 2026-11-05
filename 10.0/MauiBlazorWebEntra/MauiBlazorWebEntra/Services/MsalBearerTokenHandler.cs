using System.Net.Http.Headers;

namespace MauiBlazorWebEntra.Services;

public sealed class MsalBearerTokenHandler(MsalAuthenticationService authentication) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await authentication.GetAccessTokenAsync(cancellationToken)
            ?? throw new InvalidOperationException("Sign in again before requesting weather.");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
