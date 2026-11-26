using System.Text.Json.Serialization;
using MauiBlazorWeb.Shared.Services;

namespace MauiBlazorWeb.Models
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(WeatherForecast[]))]
    internal partial class MauiJsonContext : JsonSerializerContext
    {
    }
}
