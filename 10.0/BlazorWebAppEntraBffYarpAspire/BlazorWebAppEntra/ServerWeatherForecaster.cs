using BlazorWebAppEntra.Client.Weather;
using Microsoft.Identity.Abstractions;

namespace BlazorWebAppEntra;

internal sealed class ServerWeatherForecaster(IDownstreamApi downstreamApi) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        using var response = await downstreamApi.CallApiForUserAsync("DownstreamApi",
            options =>
            {
                options.RelativePath = "/weather-forecast";
            });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
}
