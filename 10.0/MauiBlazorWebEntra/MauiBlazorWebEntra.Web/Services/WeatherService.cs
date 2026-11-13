using MauiBlazorWebEntra.Shared.Services;

namespace MauiBlazorWebEntra.Web.Services;

public class WeatherService : IWeatherService
{
    public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
    {
        // Simulate asynchronous loading to demonstrate coming from a slow data source
        await Task.Delay(500);

        WeatherForecast[]? forecasts;

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

        forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();

        return forecasts;
    }
}
