using System.Net.Http.Json;
using System.Text.Json;
using MauiBlazorWeb.Models;
using MauiBlazorWeb.Shared.Services;

namespace MauiBlazorWeb.Services
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
