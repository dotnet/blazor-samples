using System.Net.Http.Json;

namespace BlazorSample.Client.Starship;

internal sealed class ClientFormValidation(HttpClient httpClient) : IFormValidation
{
    public async Task<IDictionary<string, string[]>> ValidateStarshipFormAsync(
        StarshipModel starship)
    {
        Dictionary<string, string[]> genericError = new()
        {
            {
                "Validation Error", 
                ["An unexpected client error occurred during validation."]
            }
        };

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/starship-validation", starship);

            if (response.IsSuccessStatusCode)
            {
                var deserializedResponseContent = 
                    await response.Content.ReadFromJsonAsync
                        <IDictionary<string, string[]>>();

                return deserializedResponseContent ?? genericError;
            }
        }
        catch (Exception)
        {
            // Log the exception
        }

        return genericError;
    }
}
