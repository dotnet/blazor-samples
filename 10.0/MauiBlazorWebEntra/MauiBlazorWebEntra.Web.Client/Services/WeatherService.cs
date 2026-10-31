using System.Net.Http.Json;
using MauiBlazorWebEntra.Shared.Services;

namespace MauiBlazorWebEntra.Web.Client.Services;

public sealed class WeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
    {
        return await httpClient.GetFromJsonAsync<WeatherForecast[]>("api/weather")
            ?? throw new InvalidOperationException("The weather API returned no forecasts.");
    }
}
