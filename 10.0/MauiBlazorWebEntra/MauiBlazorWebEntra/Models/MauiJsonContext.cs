using System.Text.Json.Serialization;
using MauiBlazorWebEntra.Shared.Services;

namespace MauiBlazorWebEntra.Models
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(WeatherForecast[]))]
    internal partial class MauiJsonContext : JsonSerializerContext
    {
    }
}
