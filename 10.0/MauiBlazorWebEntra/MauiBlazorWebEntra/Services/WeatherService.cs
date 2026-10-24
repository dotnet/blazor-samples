using MauiBlazorWebEntra.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MauiBlazorWebEntra.Services;

public class WeatherService(MsalAuthenticationStateProvider authStateProvider) : IWeatherService
{

    public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
    {
        WeatherForecast[] forecasts = [];
        try
        {
            var httpClient = HttpClientHelper.GetHttpClient();
            var weatherUrl = HttpClientHelper.WeatherUrl;

            var accessToken = await authStateProvider.GetAccessTokenAsync();

            if (accessToken is null)
            {
                Debug.WriteLine("No access token available for weather API call.");
                return forecasts;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            forecasts = (await httpClient.GetFromJsonAsync<WeatherForecast[]>(weatherUrl)) ?? [];
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"HTTP Request error: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred: {ex.Message}");
        }

        return forecasts;
    }
}
