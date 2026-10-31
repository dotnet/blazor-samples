using System.Net.Http.Json;
using System.Text.Json;
using MauiBlazorWebEntra.Models;
using MauiBlazorWebEntra.Shared.Services;

namespace MauiBlazorWebEntra.Services
{
    public sealed class WeatherService(HttpClient httpClient) : IWeatherService
    {
        public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
        {
            return await httpClient.GetFromJsonAsync("api/weather", MauiJsonContext.Default.WeatherForecastArray)
                ?? throw new JsonException("The weather API returned no forecasts.");
        }
    }
}
