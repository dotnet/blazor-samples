using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using MauiBlazorWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace MauiBlazorWeb.Services
{
    // Native client counterpart to Identity's server-side SignInManager.
    public sealed partial class MauiSignInManager(IdentityApiClient identityApiClient, ISecureStorage secureStorage, ILogger<MauiSignInManager> logger) : IDisposable
    {
        private static readonly ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());
        private static readonly TimeSpan tokenExpirationBuffer = TimeSpan.FromMinutes(30);
        private readonly SemaphoreSlim sessionLock = new(1, 1);
        private readonly TokenManager tokenManager = new(secureStorage, logger);

        public ClaimsPrincipal User { get; private set; } = anonymousUser;
        public event Action<ClaimsPrincipal>? UserChanged;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await sessionLock.WaitAsync(cancellationToken);
            var previousUser = User;

            try
            {
                await GetAccessTokenCoreAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Unable to refresh the saved sign-in.");
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Restoring the saved sign-in timed out.");
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Unable to read the saved sign-in or its refreshed token.");
            }
            finally
            {
                sessionLock.Release();
                NotifyUserChanged(previousUser);
            }
        }

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent, CancellationToken cancellationToken = default)
        {
            await sessionLock.WaitAsync(cancellationToken);
            var previousUser = User;

            try
            {
                var response = await identityApiClient.PasswordSignInAsync(email, password, cancellationToken);
                var result = response.Result;

                if (!result.Succeeded)
                {
                    return result;
                }

                await tokenManager.SaveAsync(email, response.Tokens, isPersistent);

                User = CreateAuthenticatedUser(email);
                return SignInResult.Success;
            }
            finally
            {
                sessionLock.Release();
                NotifyUserChanged(previousUser);
            }
        }

        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            await sessionLock.WaitAsync(cancellationToken);
            var previousUser = User;

            try
            {
                return await GetAccessTokenCoreAsync(cancellationToken);
            }
            finally
            {
                sessionLock.Release();
                NotifyUserChanged(previousUser);
            }
        }

        private async Task<string?> GetAccessTokenCoreAsync(CancellationToken cancellationToken)
        {
            var current = await tokenManager.GetAsync();
            if (current is null)
            {
                User = anonymousUser;
                return null;
            }

            var accessToken = current.Tokens.AccessToken;
            if (DateTime.UtcNow.Add(tokenExpirationBuffer) >= current.AccessTokenExpiration)
            {
                var response = await identityApiClient.RefreshAsync(current.Tokens.RefreshToken, cancellationToken);

                if (response is null)
                {
                    logger.LogInformation("The saved refresh token is no longer valid. Signing out.");
                    ClearSession();
                    return null;
                }

                await tokenManager.SaveAsync(current.Email, response, current.IsPersistent);
                accessToken = response.AccessToken;
            }

            if (User.Identity?.IsAuthenticated != true || User.Identity.Name != current.Email)
            {
                User = CreateAuthenticatedUser(current.Email);
            }

            return accessToken;
        }

        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            await sessionLock.WaitAsync(cancellationToken);
            var previousUser = User;

            try
            {
                ClearSession();
            }
            finally
            {
                sessionLock.Release();
                NotifyUserChanged(previousUser);
            }
        }

        private void ClearSession()
        {
            tokenManager.Clear();
            User = anonymousUser;
        }

        private void NotifyUserChanged(ClaimsPrincipal previousUser)
        {
            if (!ReferenceEquals(previousUser, User))
            {
                UserChanged?.Invoke(User);
            }
        }

        public void Dispose() =>
            sessionLock.Dispose();

        private static ClaimsPrincipal CreateAuthenticatedUser(string email) =>
            new(new ClaimsIdentity([new Claim(ClaimTypes.Name, email)], "Identity"));

        private sealed class TokenManager(ISecureStorage secureStorage, ILogger<MauiSignInManager> logger)
        {
            private const string StorageKeyName = "access_token";
            private AccessTokenInfo? token;
            private bool loaded;

            public async Task<AccessTokenInfo?> GetAsync()
            {
                if (!loaded)
                {
                    await LoadAsync();
                }

                return token;
            }

            private async Task LoadAsync()
            {
                try
                {
                    var json = await secureStorage.GetAsync(StorageKeyName);
                    if (json is null)
                    {
                        return;
                    }

                    var savedToken = JsonSerializer.Deserialize(json, StorageJsonContext.Default.AccessTokenInfo);
                    if (savedToken is null ||
                        string.IsNullOrWhiteSpace(savedToken.Email) ||
                        !IsValidTokenResponse(savedToken.Tokens) ||
                        savedToken.AccessTokenExpiration == default)
                    {
                        logger.LogWarning("Stored sign-in credentials are invalid. Treating the session as signed out.");
                        return;
                    }

                    token = savedToken;
                    token.IsPersistent = true;
                }
                catch (Exception ex) when (ex is JsonException or CryptographicException or IOException)
                {
                    logger.LogWarning(ex, "Unable to read stored sign-in credentials. Treating the session as signed out.");
                }
                finally
                {
                    loaded = true;
                }
            }

            public async Task SaveAsync(string email, AccessTokenResponse? tokens, bool isPersistent)
            {
                if (!IsValidTokenResponse(tokens))
                {
                    throw new JsonException("The Identity API returned invalid token information.");
                }

                var value = new AccessTokenInfo
                {
                    Email = email,
                    Tokens = tokens,
                    AccessTokenExpiration = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn),
                    IsPersistent = isPersistent
                };

                if (isPersistent)
                {
                    await secureStorage.SetAsync(StorageKeyName, JsonSerializer.Serialize(value, StorageJsonContext.Default.AccessTokenInfo));
                }
                else
                {
                    secureStorage.Remove(StorageKeyName);
                }

                token = value;
                loaded = true;
            }

            public void Clear()
            {
                secureStorage.Remove(StorageKeyName);
                token = null;
                loaded = true;
            }

            private static bool IsValidTokenResponse([NotNullWhen(true)] AccessTokenResponse? response) =>
                response is not null && !string.IsNullOrEmpty(response.AccessToken)
                    && !string.IsNullOrEmpty(response.RefreshToken)
                    && string.Equals(response.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase)
                    && response.ExpiresIn > 0;
        }

        private sealed class AccessTokenInfo
        {
            public required string Email { get; init; }

            [JsonPropertyName("LoginResponse")]
            public required AccessTokenResponse Tokens { get; init; }

            public required DateTime AccessTokenExpiration { get; init; }

            [JsonIgnore]
            public bool IsPersistent { get; set; }
        }

        [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
        [JsonSerializable(typeof(AccessTokenInfo))]
        private partial class StorageJsonContext : JsonSerializerContext
        {
        }
    }
}
