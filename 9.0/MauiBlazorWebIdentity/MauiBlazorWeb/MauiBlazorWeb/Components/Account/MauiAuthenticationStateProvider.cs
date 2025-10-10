using Microsoft.AspNetCore.Components.Authorization;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// This class manages the authentication state of the user.
/// The class handles user sign in, sign out, and token validation, including refreshing tokens when they are close to expiration.
/// It uses secure storage to save and retrieve tokens, ensuring that users do not need to log in every time.
/// </summary>
public partial class MauiAuthenticationStateProvider(SignInService signInService) : AuthenticationStateProvider
{
    //TODO: Place this in AppSettings or Client config file
    private const string AuthenticationType = "Custom authentication";
    private static readonly TimeSpan TokenExpirationBuffer = TimeSpan.FromMinutes(30);

    private static ClaimsPrincipal _defaultUser = new ClaimsPrincipal(new ClaimsIdentity());
    private static Task<AuthenticationState> _defaultAuthState = Task.FromResult(new AuthenticationState(_defaultUser));

    private Task<AuthenticationState> _currentAuthState = _defaultAuthState;

    private AccessTokenInfo? _accessToken;

    public AuthenticationStatus AuthenticationStatus { get; private set; } = AuthenticationStatus.None;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentAuthState != _defaultAuthState)
        {
            return _currentAuthState;
        }

        _currentAuthState = CreateAuthenticationStateFromStorageAsync();
        NotifyAuthenticationStateChanged(_currentAuthState);

        return _currentAuthState;
    }

    public async Task<AccessTokenResponse?> GetAccessTokenInfoAsync()
    {
        if (await UpdateAndValidateAccessTokenAsync())
        {
            return _accessToken?.Response;
        }

        SignOut();
        return null;
    }

    public void SignOut()
    {
        AuthenticationStatus = AuthenticationStatus.None;

        _currentAuthState = _defaultAuthState;
        _accessToken = null;

        SecureTokenStorage.Clear();

        NotifyAuthenticationStateChanged(_defaultAuthState);
    }

    public async Task<AuthenticationStatus> PasswordSignInAsync(string userName, string password, bool isPersistent)
    {
        _currentAuthState = SignInAsyncCore(userName, password, isPersistent);

        NotifyAuthenticationStateChanged(_currentAuthState);

        await _currentAuthState;
        
        return AuthenticationStatus;

        async Task<AuthenticationState> SignInAsyncCore(string userName, string password, bool isPersistent)
        {
            var user = await SignInWithProviderAsync(userName, password, isPersistent);
            return new AuthenticationState(user);
        }
    }

    private async Task<ClaimsPrincipal> SignInWithProviderAsync(string userName, string password, bool isPersistent)
    {
        var authenticatedUser = _defaultUser;
        AuthenticationStatus = AuthenticationStatus.None;

        try
        {
            var response = await signInService.SignInAsync(userName, password);

            _accessToken = new AccessTokenInfo(
                userName,
                response,
                DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));

            if (isPersistent)
            {
                // Save token to secure storage so the user doesn't have to sign in every time
                var tokenString = JsonSerializer.Serialize(_accessToken, MauiAuthenticationStateProviderContext.Default.AccessTokenInfo);
                await SecureTokenStorage.SetAsync(tokenString);
            }

            authenticatedUser = CreateAuthenticatedUser(userName);
            AuthenticationStatus = AuthenticationStatus.Success;
        }
        catch (Exception ex)
        {
            AuthenticationStatus = AuthenticationStatus.Failed(ex.Message ?? "Sign in failed.");
        }

        return authenticatedUser;
    }

    private async Task<AuthenticationState> CreateAuthenticationStateFromStorageAsync()
    {
        var authenticatedUser = _defaultUser;
        AuthenticationStatus = AuthenticationStatus.None;

        if (await UpdateAndValidateAccessTokenAsync())
        {
            authenticatedUser = CreateAuthenticatedUser(_accessToken!.UserName);
            AuthenticationStatus = AuthenticationStatus.Success;
        }

        return new AuthenticationState(authenticatedUser);
    }

    private async Task<bool> UpdateAndValidateAccessTokenAsync()
    {
        try
        {
            // Get a point in time in the future to check if the token is expiring soon.
            var futureTime = DateTime.UtcNow.Add(TokenExpirationBuffer);

            // If there is no token loaded or the token is expiring soon, try to load it from secure storage
            // just in case the app was restarted and the token was lost in memory or updated elsewhere.
            if (_accessToken is null || futureTime >= _accessToken.ExpiresAt)
            {
                var tokenString = await SecureTokenStorage.GetAsync();

                // If there is no token in secure storage, return false
                if (string.IsNullOrEmpty(tokenString))
                {
                    return false;
                }

                _accessToken = JsonSerializer.Deserialize(tokenString, MauiAuthenticationStateProviderContext.Default.AccessTokenInfo);
            }

            // If there is still no token, return false
            if (_accessToken is null)
            {
                return false;
            }

            // Attempt to refresh the access token some minutes before it expires. We always try refreshing
            // since refresh token expiration is unknown (typically 14 days) and we want to avoid timing issues.
            if (futureTime >= _accessToken.ExpiresAt)
            {
                return await RefreshAccessTokenAsync(_accessToken.UserName, _accessToken.Response.RefreshToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking token for validity: {ex}");
            return false;
        }
    }

    private async Task<bool> RefreshAccessTokenAsync(string userName, string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            var response = await signInService.RefreshSignInAsync(refreshToken);

            _accessToken = new AccessTokenInfo(
                userName,
                response,
                DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));

            // Save token to secure storage so the user doesn't have to sign in every time
            var tokenString = JsonSerializer.Serialize(_accessToken, MauiAuthenticationStateProviderContext.Default.AccessTokenInfo);
            await SecureTokenStorage.SetAsync(tokenString);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing access token: {ex}");
            return false;
        }
    }

    private ClaimsPrincipal CreateAuthenticatedUser(string email)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, email) };  // TODO: Add more claims as needed
        var identity = new ClaimsIdentity(claims, AuthenticationType);
        return new ClaimsPrincipal(identity);
    }

    private record AccessTokenInfo(
        string UserName,
        AccessTokenResponse Response,
        DateTimeOffset ExpiresAt);

    [JsonSerializable(typeof(AccessTokenResponse))]
    [JsonSerializable(typeof(AccessTokenInfo))]
    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    private partial class MauiAuthenticationStateProviderContext : JsonSerializerContext
    {
    }

    // private static class SecureTokenStorage
    // {
    //     private const string SecureStorageKey = "access_token";

    //     public static void Clear() =>
    //         SecureStorage.Default.Remove(SecureStorageKey);

    //     public static async Task SetAsync(string value) =>
    //         await SecureStorage.Default.SetAsync(SecureStorageKey, value);

    //     public static async Task<string?> GetAsync() =>
    //         await SecureStorage.Default.GetAsync(SecureStorageKey);
    // }

    private static class SecureTokenStorage
    {
        private const string SecureStorageKey = "access_token";

        public static void Clear() =>
            Preferences.Default.Remove(SecureStorageKey);

        public static async Task SetAsync(string value) =>
            Preferences.Default.Set(SecureStorageKey, value);

        public static async Task<string?> GetAsync() =>
            Preferences.Default.Get(SecureStorageKey, "");
    }
}
