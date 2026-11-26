using System.Net.Http.Headers;

namespace MauiBlazorWeb.Services
{
    public sealed class IdentityBearerTokenHandler(MauiSignInManager signInManager) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await signInManager.GetAccessTokenAsync(cancellationToken);

            if (token is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
