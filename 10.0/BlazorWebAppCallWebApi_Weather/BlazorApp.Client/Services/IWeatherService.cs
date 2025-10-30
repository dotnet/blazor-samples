using BlazorApp.Client.Models;

namespace BlazorApp.Client.Services;

public interface IWeatherService
{
    Task<WeatherForecast[]> GetWeather();
}
