using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// Service responsible for handling user authentication operations like sign in and token refresh.
/// </summary>
public partial class SignInService(HttpClient httpClient)
{
    /// <summary>
    /// Attempts to sign in a user with the provided username and password.
    /// </summary>
    /// <param name="userName">The user's email or username</param>
    /// <param name="password">The user's password</param>
    /// <returns>An access token response as a string if successful; otherwise, throws an exception</returns>
    /// <exception cref="Exception">Throws exception if sign in fails or server error occurs</exception>
    public async Task<AccessTokenResponse> SignInAsync(string userName, string password)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "login",
                new SignInRequest(userName, password),
                SignInServiceContext.Default.SignInRequest);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync(SignInServiceContext.Default.AccessTokenResponse);
                return token ?? throw new Exception("Failed to deserialize access token response.");
            }
            else
            {
                throw new Exception("Sign in failed. Please check your credentials.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error signing in: {ex}");
            throw new Exception("Server error during sign in.", ex);
        }
    }

    /// <summary>
    /// Attempts to refresh the access token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>An access token response as a string if successful; otherwise, throws an exception</returns>
    /// <exception cref="Exception">Throws exception if token refresh fails or server error occurs</exception>
    public async Task<AccessTokenResponse> RefreshSignInAsync(string refreshToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "refresh",
                new RefreshTokenRequest(refreshToken),
                SignInServiceContext.Default.RefreshTokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync(SignInServiceContext.Default.AccessTokenResponse);
                return token ?? throw new Exception("Failed to deserialize access token response.");
            }
            else
            {
                throw new Exception("Token refresh failed.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing access token: {ex}");
            throw new Exception("Server error during token refresh.", ex);
        }
    }

    private sealed record SignInRequest(string Email, string Password);

    private sealed record RefreshTokenRequest(string RefreshToken);

    [JsonSerializable(typeof(SignInRequest))]
    [JsonSerializable(typeof(RefreshTokenRequest))]
    [JsonSerializable(typeof(AccessTokenResponse))]
    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    private partial class SignInServiceContext : JsonSerializerContext
    {
    }
}
