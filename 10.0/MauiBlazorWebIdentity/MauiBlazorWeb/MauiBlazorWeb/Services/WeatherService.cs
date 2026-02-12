using MauiBlazorWeb.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MauiBlazorWeb.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly MauiAuthenticationStateProvider _authenticationStateProvider;

        public WeatherService(MauiAuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
        {
            var forecasts = Array.Empty<WeatherForecast>();
            try
            {
                var httpClient = HttpClientHelper.GetHttpClient();
                var weatherUrl = HttpClientHelper.WeatherUrl;

                var accessTokenInfo = await _authenticationStateProvider.GetAccessTokenInfoAsync();

                if (accessTokenInfo is null)
                {
                    throw new Exception("Could not retrieve access token to get weather forecast.");
                }

                var token = accessTokenInfo.LoginResponse.AccessToken;
                var scheme = accessTokenInfo.LoginResponse.TokenType; //"Bearer"

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(scheme))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
                    forecasts = (await httpClient.GetFromJsonAsync<WeatherForecast[]>(weatherUrl)) ?? [];
                }
                else
                {
                    Debug.WriteLine("Token or scheme is null or empty.");
                }
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
