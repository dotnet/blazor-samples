using BlazorWebAppEntra.Client.Weather;
using Microsoft.Identity.Abstractions;

namespace BlazorWebAppEntra;

internal sealed class ServerWeatherForecaster : IWeatherForecaster
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public ServerWeatherForecaster(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IConfiguration configuration)
    {
        this._httpClient = httpClient;
        this._tokenAcquisition = tokenAcquisition;
        this._configuration = configuration;
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
       
        var scopes = _configuration.GetSection("DownstreamApi:Scopes").Get<string[]>();
        if (scopes == null || scopes.Length == 0)
            throw new InvalidOperationException("No downstream API scopes configured!");

        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

         _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync("/weather-forecast");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
