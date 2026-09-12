using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using MauiBlazorWeb.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace MauiBlazorWeb.Services;

/// <summary>
/// Owns the native client's opaque Identity token pair and authentication state.
/// </summary>
public sealed class MauiAuthenticationStateProvider : AuthenticationStateProvider
{
    private const int TokenExpirationBufferMinutes = 30;
    private const string AuthenticationType = "IdentityBearer";
    private static readonly ClaimsPrincipal DefaultUser = new(new ClaimsIdentity());
    private static readonly Task<AuthenticationState> DefaultAuthState =
        Task.FromResult(new AuthenticationState(DefaultUser));
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Task<AuthenticationState> _currentAuthState = DefaultAuthState;
    private AccessTokenInfo? _accessToken;
    private bool _persistToken;
    private long _authEpoch;

    public LoginStatus LoginStatus { get; private set; }

    public string LoginFailureMessage { get; private set; } = string.Empty;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentAuthState != DefaultAuthState)
        {
            return _currentAuthState;
        }

        _currentAuthState = RestoreAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(_currentAuthState);
        return _currentAuthState;
    }

    public async Task<AccessTokenInfo?> GetAccessTokenInfoAsync()
    {
        if (await UpdateAndValidateAccessTokenAsync())
        {
            return _accessToken;
        }

        await LogoutAsync();
        return null;
    }

    public async Task LogoutAsync()
    {
        Interlocked.Increment(ref _authEpoch);
        LoginStatus = LoginStatus.None;
        LoginFailureMessage = string.Empty;
        _accessToken = null;
        _persistToken = false;
        _currentAuthState = DefaultAuthState;
        await TokenStorage.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(DefaultAuthState);
    }

    public Task LogInAsync(LoginRequest login)
    {
        var epoch = Interlocked.Increment(ref _authEpoch);
        _currentAuthState = LogInAsyncCore(login, epoch);
        NotifyAuthenticationStateChanged(_currentAuthState);
        return _currentAuthState;
    }

    private async Task<AuthenticationState> LogInAsyncCore(LoginRequest login, long epoch)
    {
        LoginStatus = LoginStatus.None;
        LoginFailureMessage = string.Empty;

        try
        {
            using var client = HttpClientHelper.GetHttpClient();
            using var response = await client.PostAsJsonAsync(
                HttpClientHelper.OverrideLoginUrl,
                new
                {
                    login.Email,
                    login.Password,
                    login.TwoFactorCode,
                    login.TwoFactorRecoveryCode,
                });

            if (!response.IsSuccessStatusCode)
            {
                var failure = await response.Content.ReadFromJsonAsync<LoginFailureResponse>();
                if (epoch == Volatile.Read(ref _authEpoch))
                {
                    LoginStatus = LoginStatus.Failed;
                    LoginFailureMessage = GetLoginFailureMessage(failure?.Code);
                }

                return new AuthenticationState(DefaultUser);
            }

            var responseToken = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (responseToken is null)
            {
                throw new InvalidOperationException("Identity login returned no token response.");
            }

            var email = await GetAuthoritativeEmailAsync(client, responseToken);
            if (string.IsNullOrWhiteSpace(email) || epoch != Volatile.Read(ref _authEpoch))
            {
                return new AuthenticationState(DefaultUser);
            }

            var token = TokenStorage.DeserializeToken(
                JsonSerializer.Serialize(responseToken),
                email);
            if (token is null)
            {
                throw new InvalidOperationException("Identity login returned an invalid token response.");
            }

            _persistToken = login.RememberMe;
            _accessToken = login.RememberMe
                ? await TokenStorage.SaveTokenToSecureStorageAsync(JsonSerializer.Serialize(responseToken), email)
                : token;
            if (_accessToken is null || epoch != Volatile.Read(ref _authEpoch))
            {
                return new AuthenticationState(DefaultUser);
            }

            if (!login.RememberMe)
            {
                await TokenStorage.RemoveTokenAsync();
            }

            LoginStatus = LoginStatus.Success;
            return new AuthenticationState(CreateAuthenticatedUser(email));
        }
        catch (HttpRequestException)
        {
            LoginStatus = LoginStatus.Failed;
            LoginFailureMessage = "The Identity server could not be reached.";
            return new AuthenticationState(DefaultUser);
        }
        catch (JsonException)
        {
            LoginStatus = LoginStatus.Failed;
            LoginFailureMessage = "The Identity server returned an invalid response.";
            return new AuthenticationState(DefaultUser);
        }
    }

    private async Task<AuthenticationState> RestoreAuthenticationStateAsync()
    {
        _persistToken = true;
        if (!await UpdateAndValidateAccessTokenAsync() || _accessToken is null)
        {
            return new AuthenticationState(DefaultUser);
        }

        LoginStatus = LoginStatus.Success;
        return new AuthenticationState(CreateAuthenticatedUser(_accessToken.Email));
    }

    private async Task<bool> UpdateAndValidateAccessTokenAsync()
    {
        if (_accessToken is null)
        {
            _accessToken = await TokenStorage.GetTokenFromSecureStorageAsync();
            _persistToken = _accessToken is not null;
        }

        if (_accessToken is null)
        {
            return false;
        }

        if (DateTime.UtcNow.AddMinutes(TokenExpirationBufferMinutes) < _accessToken.AccessTokenExpiration)
        {
            return true;
        }

        await _refreshLock.WaitAsync();
        try
        {
            if (_accessToken is null)
            {
                return false;
            }

            if (DateTime.UtcNow.AddMinutes(TokenExpirationBufferMinutes) < _accessToken.AccessTokenExpiration)
            {
                return true;
            }

            var epoch = Volatile.Read(ref _authEpoch);
            using var client = HttpClientHelper.GetHttpClient();
            using var response = await client.PostAsJsonAsync(
                HttpClientHelper.RefreshUrl,
                new { _accessToken.LoginResponse.RefreshToken });
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var refreshed = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (refreshed is null || epoch != Volatile.Read(ref _authEpoch))
            {
                return false;
            }

            var replacement = TokenStorage.DeserializeToken(
                JsonSerializer.Serialize(refreshed),
                _accessToken.Email);
            if (replacement is null)
            {
                return false;
            }

            _accessToken = _persistToken
                ? await TokenStorage.SaveTokenToSecureStorageAsync(JsonSerializer.Serialize(refreshed), replacement.Email)
                : replacement;
            return _accessToken is not null && epoch == Volatile.Read(ref _authEpoch);
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<string?> GetAuthoritativeEmailAsync(HttpClient client, LoginResponse token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, HttpClientHelper.ManageInfoUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
        using var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return (await response.Content.ReadFromJsonAsync<ManageInfoResponse>())?.Email;
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string email) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Name, email)], AuthenticationType));

    private static string GetLoginFailureMessage(string? code) => code switch
    {
        "requires_two_factor" => "Two-factor authentication is required.",
        "locked_out" => "This account is locked out.",
        "not_allowed" => "This account is not allowed to sign in. Confirm its email first.",
        _ => "The email address or password is incorrect.",
    };

    private sealed record LoginFailureResponse(string Code);

    private sealed record ManageInfoResponse(string? Email, bool IsEmailConfirmed);
}
