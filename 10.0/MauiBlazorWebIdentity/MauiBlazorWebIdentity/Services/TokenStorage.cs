using System.Diagnostics;
using System.Text.Json;
using MauiBlazorWebIdentity.Models;

namespace MauiBlazorWebIdentity.Services
{
    /// <summary>
    /// This class is used to store and retrieve the access token from the SecureStorage.
    /// </summary>
    internal class TokenStorage
    {
        private const string StorageKeyName = "access_token";

        public static void RemoveToken()
        {
            SecureStorage.Remove(StorageKeyName);
        }

        public static async Task<AccessTokenInfo?> GetTokenFromSecureStorageAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(StorageKeyName);

                if (!string.IsNullOrEmpty(token))
                {
                    return JsonSerializer.Deserialize<AccessTokenInfo>(token);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to retrieve AccessTokenInfo from SecureStorage." + ex);
            }
            return null;
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
            AccessTokenInfo? accessToken = null;
            try
            {
                accessToken = DeserializeToken(token, email);
                if (accessToken != null)
                {
                    await SecureStorage.SetAsync(StorageKeyName, JsonSerializer.Serialize<AccessTokenInfo>(accessToken));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to save AccessTokenInfo to SecureStorage." + ex);
                accessToken = null;
            }
            return accessToken;
        }
    }
}
