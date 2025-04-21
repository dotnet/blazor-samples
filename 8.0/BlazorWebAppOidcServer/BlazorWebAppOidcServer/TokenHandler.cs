using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace BlazorWebAppOidcServer;

public class TokenHandler(IHttpContextAccessor httpContextAccessor) : 
    DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = httpContextAccessor.HttpContext?
            .GetTokenAsync("access_token").Result ?? 
            throw new Exception("No access token");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
