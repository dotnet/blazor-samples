using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Json;
using BlazorWasmAuth.Identity.Models;

namespace BlazorWasmAuth.Identity
{
    /// <summary>
    /// Handles state for cookie-based auth.
    /// </summary>
    public class CookieAuthenticationStateProvider : AuthenticationStateProvider, IAccountManagement
    {
        /// <summary>
        /// Map the JavaScript-formatted properties to C#-formatted classes.
        /// </summary>
        private readonly JsonSerializerOptions jsonSerializerOptions =
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

        /// <summary>
        /// Special auth client.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Authentication state.
        /// </summary>
        private bool _authenticated = false;

        /// <summary>
        /// Default principal for anonymous (not authenticated) users.
        /// </summary>
        private readonly ClaimsPrincipal Unauthenticated = 
            new(new ClaimsIdentity());

        /// <summary>
        /// Create a new instance of the auth provider.
        /// </summary>
        /// <param name="httpClientFactory">Factory to retrieve auth client.</param>
        public CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory)
            => _httpClient = httpClientFactory.CreateClient("Auth");

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The result serialized to a <see cref="FormResult"/>.
        /// </returns>
        public async Task<FormResult> RegisterAsync(string email, string password)
        {
            string[] defaultDetail = ["An unknown error prevented registration from succeeding."];

            try
            {
                // make the request
                var result = await _httpClient.PostAsJsonAsync(
                    "register", new
                    {
                        email,
                        password
                    });

                // successful?
                if (result.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }

                // body should contain details about why it failed
                var details = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(details);
                var errors = new List<string>();
                var errorList = problemDetails.RootElement.GetProperty("errors");
                foreach (var errorEntry in errorList.EnumerateObject())
                {
                    if (errorEntry.Value.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(errorEntry.Value.GetString()!);
                    }
                    else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(
                            errorEntry.Value.EnumerateArray().Select(
                                e => e.GetString() ?? string.Empty)
                            .Where(e => !string.IsNullOrEmpty(e)));
                    }
                }

                // return the error list
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = problemDetails == null ? defaultDetail : [.. errors]
                };
            }
            catch { }
             
            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = defaultDetail
            };
        }

        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The result of the login request serialized to a <see cref="FormResult"/>.</returns>
        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                // login with cookies
                var result = await _httpClient.PostAsJsonAsync(
                    "login?useCookies=true", new
                    {
                        email,
                        password
                    });

                // success?
                if (result.IsSuccessStatusCode)
                {
                    // need to refresh auth state
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    // success!
                    return new FormResult { Succeeded = true };
                }                
            }
            catch { }

            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = ["Invalid email and/or password."]
            };
        }

        /// <summary>
        /// Get authentication state.
        /// </summary>
        /// <remarks>
        /// Called by Blazor anytime and authentication-based decision needs to be made, then cached
        /// until the changed state notification is raised.
        /// </remarks>
        /// <returns>The authentication state asynchronous request.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            // default to not authenticated
            var user = Unauthenticated;

            try
            {
                // the user info endpoint is secured, so if the user isn't logged in this will fail
                var userResponse = await _httpClient.GetAsync("manage/info");

                // throw if user info wasn't retrieved
                userResponse.EnsureSuccessStatusCode();

                // user is authenticated,so let's build their authenticated identity
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, jsonSerializerOptions);
                if (userInfo != null)
                {
                    // in our system name and email are the same
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.Email),
                        new(ClaimTypes.Email, userInfo.Email)
                    };

                    // add any additional claims
                    claims.AddRange(
                        userInfo.Claims.Where(c => c.Key != ClaimTypes.Name && c.Key != ClaimTypes.Email)
                            .Select(c => new Claim(c.Key, c.Value)));

                    // set the principal
                    var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    _authenticated = true;
                }
            }
            catch { }

            // return the state
            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("Logout", null);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return _authenticated;
        }
    }
}
