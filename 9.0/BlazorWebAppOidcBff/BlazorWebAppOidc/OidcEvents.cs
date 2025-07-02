using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlazorWebAppOidc;

/// <summary>
/// Stores access token and refresh token in the server-side token store once a token is validated.
/// See https://docs.duendesoftware.com/accesstokenmanagement/blazor-server/ for more information.
/// </summary>
public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserTokenStore _store;

    public OidcEvents(IUserTokenStore store) => _store = store;

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await _store.StoreTokenAsync(context.Principal!, new UserToken
        {
            AccessToken = AccessToken.Parse(context.TokenEndpointResponse.AccessToken),
            AccessTokenType = AccessTokenType.Parse(context.TokenEndpointResponse.TokenType),
            RefreshToken = RefreshToken.Parse(context.TokenEndpointResponse.RefreshToken),
            Scope = !string.IsNullOrEmpty(context.TokenEndpointResponse.Scope)
                ? Scope.Parse(context.TokenEndpointResponse.Scope)
                : Scope.Parse(string.Join(" ", context.Options.Scope)),
            ClientId = !string.IsNullOrEmpty(context.ProtocolMessage.ClientId)
                ? ClientId.Parse(context.ProtocolMessage.ClientId)
                : ClientId.Parse(context.Options.ClientId!),
            IdentityToken = IdentityToken.Parse(context.TokenEndpointResponse.IdToken),
            Expiration = exp,
        });

        await base.TokenValidated(context);
    }
}
