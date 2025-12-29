namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// The JSON data transfer object for the bearer token response typically found in "/login" and "/refresh" responses.
/// </summary>
public sealed class IdentityAccessTokenResponse
{
    /// <summary>
    /// The value is always "Bearer" which indicates this response provides a "Bearer" token
    /// in the form of an opaque <see cref="AccessToken"/>.
    /// </summary>
    public string TokenType { get; } = "Bearer";

    /// <summary>
    /// The opaque bearer token to send as part of the Authorization request header.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// The number of seconds before the <see cref="AccessToken"/> expires.
    /// </summary>
    public required long ExpiresIn { get; init; }

    /// <summary>
    /// If set, this provides the ability to get a new access_token after it expires using a refresh endpoint.
    /// </summary>
    public required string RefreshToken { get; init; }
}
