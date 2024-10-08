using System.Net.Http.Json;
using BlazorApp.Client.Models;

namespace BlazorApp.Client.Services;

public class ClientWeatherService(HttpClient httpClient) : IWeatherService
{
    public Task<WeatherForecast[]> GetWeather() =>
        httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weather")!;
}
