using System.Text.Json;
using MauiBlazorWeb.Models;

namespace MauiBlazorWeb.Services
{
    internal class TokenStorage
    {
        private const string StorageKeyName = "access_token";
        private static readonly SemaphoreSlim StorageLock = new(1, 1);

        public static async Task RemoveTokenAsync()
        {
            await StorageLock.WaitAsync();
            try
            {
                SecureStorage.Remove(StorageKeyName);
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

        public static async Task<AccessTokenInfo?> SaveTokenToSecureStorageAsync(string token, string email)
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
                return accessToken;
            }
            finally
            {
                StorageLock.Release();
            }
        }
    }
}
