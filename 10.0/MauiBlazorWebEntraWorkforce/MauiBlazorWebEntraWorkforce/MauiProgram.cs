using MauiBlazorWebEntraWorkforce.Services;
using MauiBlazorWebEntraWorkforce.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MauiBlazorWebEntraWorkforce;

public static class MauiProgram
{
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

        // Add MSAL services
        builder.Services.AddMsalClient();

        // Add Blazor authentication and authorization services
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<MsalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<MsalAuthenticationStateProvider>());

        // Add device-specific services used by the MauiBlazorWebEntraWorkforce.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IWeatherService, WeatherService>();

        return builder.Build();
    }
}
