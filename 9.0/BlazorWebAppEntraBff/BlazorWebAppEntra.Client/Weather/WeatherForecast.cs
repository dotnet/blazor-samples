namespace BlazorWebAppEntra.Client.Weather;

public sealed class WeatherForecast(DateOnly date, int temperatureC, string summary)
{
    public DateOnly Date { get; set; } = date;
    public int TemperatureC { get; set; } = temperatureC;
    public string? Summary { get; set; } = summary;
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
