using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using MauiBlazorWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace MauiBlazorWeb.Services
{
    public sealed class IdentityApiClient(IHttpClientFactory httpClientFactory)
    {
        public async Task<IdentityResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.PostAsJsonAsync("identity/register", request, IdentityJsonContext.Default.RegisterRequest, cancellationToken);

            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                response.EnsureSuccessStatusCode();
                return IdentityResult.Success;
            }

            var problem = await response.Content.ReadFromJsonAsync(IdentityJsonContext.Default.ValidationProblemDetails, cancellationToken);
            if (problem?.Errors is null)
            {
                throw new JsonException("The Identity API returned invalid registration errors.");
            }

            var errors = new List<IdentityError>();
            foreach (var error in problem.Errors)
            {
                if (error.Value is null)
                {
                    throw new JsonException("The Identity API returned invalid registration errors.");
                }

                foreach (var description in error.Value)
                {
                    if (string.IsNullOrEmpty(description))
                    {
                        throw new JsonException("The Identity API returned an invalid registration error.");
                    }

                    errors.Add(new IdentityError { Code = error.Key, Description = description });
                }
            }

            if (errors.Count == 0)
            {
                throw new JsonException("The Identity API returned no registration errors for a failed request.");
            }

            return IdentityResult.Failed(errors.ToArray());
        }

        public Task<SignInResponse> PasswordSignInAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var credentials = new LoginRequest { Email = email, Password = password };
            return PostAsync("identity/login", credentials, IdentityJsonContext.Default.LoginRequest, cancellationToken);
        }

        public async Task<AccessTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var request = new RefreshRequest { RefreshToken = refreshToken };
            var response = await PostAsync("identity/refresh", request, IdentityJsonContext.Default.RefreshRequest, cancellationToken);
            return response.Tokens;
        }

        private async Task<SignInResponse> PostAsync<T>(string path, T request, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.PostAsJsonAsync(path, request, jsonTypeInfo, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new SignInResponse
                {
                    Result = await ReadSignInFailureAsync(response, cancellationToken)
                };
            }

            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync(IdentityJsonContext.Default.AccessTokenResponse, cancellationToken)
                ?? throw new JsonException("The Identity API returned no token response.");
            return new SignInResponse { Result = SignInResult.Success, Tokens = token };
        }

        private static async Task<SignInResult> ReadSignInFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
            {
                return SignInResult.Failed;
            }

            var problem = await response.Content.ReadFromJsonAsync(IdentityJsonContext.Default.ProblemDetails, cancellationToken)
                ?? throw new JsonException("The Identity API returned an invalid sign-in failure.");
            return problem.Detail switch
            {
                "LockedOut" => SignInResult.LockedOut,
                "NotAllowed" => SignInResult.NotAllowed,
                "RequiresTwoFactor" => SignInResult.TwoFactorRequired,
                _ => SignInResult.Failed
            };
        }
    }
}
