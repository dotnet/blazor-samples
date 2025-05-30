using BlazorWebAppOidc.Client.Weather;

namespace BlazorWebAppOidc.Weather;

internal sealed class ServerWeatherForecaster(IHttpClientFactory clientFactory) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/weather-forecast");
        var client = clientFactory.CreateClient("ExternalApi");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
}
