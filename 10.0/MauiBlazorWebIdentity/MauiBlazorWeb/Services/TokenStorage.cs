using System.Diagnostics;
using System.Text.Json;
using MauiBlazorWeb.Models;

namespace MauiBlazorWeb.Services
{
    internal class TokenStorage
    {
        private const string StorageKeyName = "access_token";
        private static readonly SemaphoreSlim StorageLock = new(1, 1);

        public static async Task<bool> RemoveTokenAsync()
        {
            await StorageLock.WaitAsync();
            try
            {
                SecureStorage.Remove(StorageKeyName);
                return true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.WriteLine($"Unable to remove the Identity token from secure storage: {exception.Message}");
                return false;
            }
            finally
            {
                StorageLock.Release();
            }
        }

        public static async Task<AccessTokenInfo?> GetTokenFromSecureStorageAsync()
        {
            await StorageLock.WaitAsync();
            try
            {
                var token = await SecureStorage.GetAsync(StorageKeyName);
                return string.IsNullOrEmpty(token)
                    ? null
                    : JsonSerializer.Deserialize<AccessTokenInfo>(token);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.WriteLine($"Unable to retrieve the Identity token from secure storage: {exception.Message}");
                return null;
            }
            finally
            {
                StorageLock.Release();
            }
        }

        public static AccessTokenInfo? DeserializeToken(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return null;
            }

            var loginToken = JsonSerializer.Deserialize<LoginResponse>(token);
            if (loginToken == null)
            {
                return null;
            }

            return new AccessTokenInfo
            {
                LoginResponse = loginToken,
                Email = email,
                AccessTokenExpiration = DateTime.UtcNow.AddSeconds(loginToken.ExpiresIn)
            };
        }

        public static async Task<TokenStorageWriteResult?> SaveTokenToSecureStorageAsync(string token, string email)
        {
            var accessToken = DeserializeToken(token, email);
            if (accessToken is null)
            {
                return null;
            }

            await StorageLock.WaitAsync();
            try
            {
                // The complete access/refresh pair is one serialized value, so a
                // successful SecureStorage write cannot expose a mixed pair.
                await SecureStorage.SetAsync(StorageKeyName, JsonSerializer.Serialize(accessToken));
                return new TokenStorageWriteResult(accessToken, IsPersisted: true);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.WriteLine($"Unable to persist the Identity token in secure storage: {exception.Message}");
                // The server-issued pair remains valid for the current process.
                // Callers must keep it in memory but not assume it survives restart.
                return new TokenStorageWriteResult(accessToken, IsPersisted: false);
            }
            finally
            {
                StorageLock.Release();
            }
        }

        internal sealed record TokenStorageWriteResult(AccessTokenInfo Token, bool IsPersisted);
    }
}
