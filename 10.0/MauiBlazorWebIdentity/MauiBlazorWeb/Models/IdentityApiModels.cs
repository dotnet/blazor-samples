using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace MauiBlazorWeb.Models
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(LoginRequest))]
    [JsonSerializable(typeof(AccessTokenResponse))]
    [JsonSerializable(typeof(RefreshRequest))]
    [JsonSerializable(typeof(RegisterRequest))]
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(ValidationProblemDetails))]
    internal partial class IdentityJsonContext : JsonSerializerContext
    {
    }

    public sealed class LoginRequest
    {
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("password")]
        public required string Password { get; init; }
    }

    public sealed class RegisterRequest
    {
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("password")]
        public required string Password { get; init; }
    }

    public sealed class RefreshRequest
    {
        [JsonPropertyName("refreshToken")]
        public required string RefreshToken { get; init; }
    }

    public sealed class AccessTokenResponse
    {
        [JsonPropertyName("tokenType")]
        public required string TokenType { get; init; }

        [JsonPropertyName("accessToken")]
        public required string AccessToken { get; init; }

        [JsonPropertyName("expiresIn")]
        public required int ExpiresIn { get; init; }

        [JsonPropertyName("refreshToken")]
        public required string RefreshToken { get; init; }
    }

    public class ProblemDetails
    {
        [JsonPropertyName("detail")]
        public string? Detail { get; init; }
    }

    public sealed class ValidationProblemDetails : ProblemDetails
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; init; }
    }

    // A client-side result, not the JSON returned by the Identity API.
    public sealed class SignInResponse
    {
        public required SignInResult Result { get; init; }
        public AccessTokenResponse? Tokens { get; init; }
    }
}
