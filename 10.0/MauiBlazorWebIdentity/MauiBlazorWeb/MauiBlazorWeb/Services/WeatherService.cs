using MauiBlazorWeb.Shared.Services;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiBlazorWeb.Services;

public partial class WeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
    {
        try
        {
            var forecasts = await httpClient.GetFromJsonAsync(
                "weather", 
                WeatherServiceContext.Default.WeatherForecastArray);
            
            return forecasts ?? [];
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Request error: {httpEx.Message}");
            return [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
            return [];
        }
    }

    // Source generation for System.Text.Json
    [JsonSerializable(typeof(WeatherForecast[]))]
    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    public partial class WeatherServiceContext : JsonSerializerContext
    {
    }
}
