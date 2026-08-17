using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using BlazorSample.Client.Starship;

namespace BlazorSample.Starship;

internal sealed class ServerFormValidation(
    IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory) 
    : IFormValidation
{
    public async Task<IDictionary<string, string[]>> ValidateStarshipFormAsync(
        StarshipModel starship)
    {
        Dictionary<string, string[]> genericError = new()
        {
            {
                "Validation Error",
                ["An unexpected server error occurred during validation."]
            }
        };

        try
        {
            if (httpContextAccessor.HttpContext is null)
            {
                throw new Exception("HttpContext not available");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, 
                "https://localhost:7277/api-starship-validation")
            {
                Content = new StringContent(JsonSerializer.Serialize(starship), 
                    System.Text.Encoding.UTF8, "application/json")
            };

            var accessToken = 
                await httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            using var httpClient = httpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request);

            if (response?.StatusCode == HttpStatusCode.NoContent)
            {
                return new Dictionary<string, string[]>();
            }

            if (response?.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();

                var deserialized =
                    JsonSerializer.Deserialize<ValidationProblemDetails>(
                        content,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                return deserialized?.Errors ?? genericError;
            }

            return genericError;
        }
        catch (Exception)
        {
            // Log the exception
        }

        return genericError;
    }
}
