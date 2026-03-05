using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// Client for making HTTP requests to the Identity API endpoints for authentication operations.
/// </summary>
public partial class IdentityApiClient(HttpClient httpClient)
{
    /// <summary>
    /// Posts a login request to the /login endpoint with the provided username and password.
    /// </summary>
    /// <param name="userName">The user's email or username</param>
    /// <param name="password">The user's password</param>
    /// <returns>An access token response if successful; otherwise, throws an exception</returns>
    /// <exception cref="Exception">Throws exception if login request fails or server error occurs</exception>
    public async Task<IdentityAccessTokenResponse> PostLoginAsync(string userName, string password)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "login",
                new LoginRequest(userName, password),
                IdentityApiClientContext.Default.LoginRequest);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync(IdentityApiClientContext.Default.IdentityAccessTokenResponse);
                return token ?? throw new Exception("Failed to deserialize access token response.");
            }
            else
            {
                throw new Exception("Login failed. Please check your credentials.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during login request: {ex}");
            throw new Exception("Server error during login.", ex);
        }
    }

    /// <summary>
    /// Posts a refresh token request to the /refresh endpoint with the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>An access token response if successful; otherwise, throws an exception</returns>
    /// <exception cref="Exception">Throws exception if token refresh request fails or server error occurs</exception>
    public async Task<IdentityAccessTokenResponse> PostRefreshAsync(string refreshToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "refresh",
                new RefreshTokenRequest(refreshToken),
                IdentityApiClientContext.Default.RefreshTokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync(IdentityApiClientContext.Default.IdentityAccessTokenResponse);
                return token ?? throw new Exception("Failed to deserialize access token response.");
            }
            else
            {
                throw new Exception("Token refresh failed.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during refresh request: {ex}");
            throw new Exception("Server error during token refresh.", ex);
        }
    }

    /// <summary>
    /// Posts a forgot password request to the /ForgotPassword endpoint with the provided email.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <exception cref="Exception">Throws exception if the forgot password request fails or server error occurs</exception>
    public async Task PostForgotPasswordAsync(string email)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "forgotPassword",
                new ForgotPasswordRequest(email),
                IdentityApiClientContext.Default.ForgotPasswordRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Forgot password request failed.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during forgot password request: {ex}");
            throw new Exception("Server error during forgot password request.", ex);
        }
    }

    /// <summary>
    /// Posts a resend confirmation email request to the /ResendConfirmationEmail endpoint with the provided email.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <exception cref="Exception">Throws exception if the resend confirmation email request fails or server error occurs</exception>
    public async Task PostResendConfirmationEmailAsync(string email)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "resendConfirmationEmail",
                new ResendConfirmationEmailRequest(email),
                IdentityApiClientContext.Default.ResendConfirmationEmailRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Resend confirmation email request failed.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during resend confirmation email request: {ex}");
            throw new Exception("Server error during resend confirmation email request.", ex);
        }
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record RefreshTokenRequest(string RefreshToken);

    private sealed record ForgotPasswordRequest(string Email);

    private sealed record ResendConfirmationEmailRequest(string Email);

    [JsonSerializable(typeof(ForgotPasswordRequest))]
    [JsonSerializable(typeof(IdentityAccessTokenResponse))]
    [JsonSerializable(typeof(LoginRequest))]
    [JsonSerializable(typeof(RefreshTokenRequest))]
    [JsonSerializable(typeof(ResendConfirmationEmailRequest))]
    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    private partial class IdentityApiClientContext : JsonSerializerContext
    {
    }
}
