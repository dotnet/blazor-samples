using System.Collections.Concurrent;
using System.Security.Claims;
using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;

namespace BlazorWebAppOidc;

/// <summary>
/// Simple implementation of a server-side token store.
/// See https://docs.duendesoftware.com/accesstokenmanagement/blazor-server/ for more information.
/// </summary>
public class ServerSideTokenStore : IUserTokenStore
{
    private static readonly ConcurrentDictionary<string, TokenForParameters> _tokens = new();

    public Task<TokenResult<TokenForParameters>> GetTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        if (_tokens.TryGetValue(sub, out var value))
        {
            return Task.FromResult(TokenResult.Success(value));
        }

        return Task.FromResult((TokenResult<TokenForParameters>)TokenResult.Failure("not found"));
    }

    public Task StoreTokenAsync(ClaimsPrincipal user, UserToken token, UserTokenRequestParameters? parameters = null, CancellationToken ct = default)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        _tokens[sub] = new TokenForParameters(token,
            token.RefreshToken == null
                ? null
                : new UserRefreshToken(token.RefreshToken.Value, token.DPoPJsonWebKey));

        return Task.CompletedTask;
    }

    public Task ClearTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null, CancellationToken ct = default)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");

        _tokens.TryRemove(sub, out _);
        return Task.CompletedTask;
    }
}
