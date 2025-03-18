using BlazorWebAppEntra.Client.Weather;
using Microsoft.Identity.Web;

namespace BlazorWebAppEntra;

internal sealed class ServerWeatherForecaster(HttpClient httpClient, ITokenAcquisition tokenAcquisition, IConfiguration configuration) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync([$"{configuration["AzureAd:AppIdUri"]}/Weather.Get"]);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/weather-forecast");
        requestMessage.Headers.Authorization = new("Bearer", accessToken);
        using var response = await httpClient.SendAsync(requestMessage);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
}
