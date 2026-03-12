using MauiBlazorWebEntra.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MauiBlazorWebEntra.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly MsalAuthenticationStateProvider _authStateProvider;

        public WeatherService(MsalAuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
        {
            var forecasts = Array.Empty<WeatherForecast>();
            try
            {
                var httpClient = HttpClientHelper.GetHttpClient();
                var weatherUrl = HttpClientHelper.WeatherUrl;

                var accessToken = await _authStateProvider.GetAccessTokenAsync();

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
}
