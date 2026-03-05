using MauiBlazorWeb.Components.Account;
using MauiBlazorWeb.Services;
using MauiBlazorWeb.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MauiBlazorWeb;

public static class MauiProgram
{
    // TODO: Place this in AppSettings or Client config file
    // See: https://learn.microsoft.com/dotnet/maui/data-cloud/local-web-services
    // Android Emulator uses 10.0.2.2 to refer to localhost
    private static string _baseUrl =
        DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7157/"
            : "https://localhost:7157/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Add device-specific services used by the MauiBlazorWeb.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddSingleton<ISupportedFeatures, SupportedFeatures>();

        // Register the message handler that trusts localhost certificates
        // This is only for development purposes and should not be used in production
        builder.Services.AddTransient<LocalhostTrustingMessageHandler>();

        // Register authentication services
        builder.Services.AddAccountServices($"{_baseUrl}identity/", clientBuilder => clientBuilder
            .ConfigurePrimaryHttpMessageHandler<LocalhostTrustingMessageHandler>());

        // Configure Typed HttpClient for WeatherService WITH authentication
        builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
            {
                client.BaseAddress = new Uri($"{_baseUrl}api/");
            })
            .ConfigurePrimaryHttpMessageHandler<LocalhostTrustingMessageHandler>()
            .AddIdentityAuthorizationHandler();

        // Register needed elements for authentication:
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthorizationCore();

        return builder.Build();
    }
}
