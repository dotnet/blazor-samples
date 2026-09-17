using MauiBlazorWeb.Services;
using MauiBlazorWeb.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
using Microsoft.Maui.DevFlow.Blazor;
#endif

namespace MauiBlazorWeb
{
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
            DevFlowWebViewInspector.Enable();
            builder.AddMauiDevFlowAgent(options =>
            {
                options.Port = ResolveDevFlowAgentPort();
            });
            builder.AddMauiBlazorDevFlowTools();
#endif

            //Register needed elements for authentication:
            // This is the core functionality
            builder.Services.AddAuthorizationCore();
            // This is our custom provider
            builder.Services.AddScoped<MauiAuthenticationStateProvider>();
            // Use our custom provider when the app needs an AuthenticationStateProvider
            builder.Services.AddScoped<AuthenticationStateProvider>(s
                => (MauiAuthenticationStateProvider)s.GetRequiredService<MauiAuthenticationStateProvider>());
            builder.Services.AddScoped<AccountClient>();

            // Add device-specific services used by the MauiBlazorWeb.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddScoped<IWeatherService, WeatherService>();

            return builder.Build();
        }

#if DEBUG
        private static int ResolveDevFlowAgentPort()
        {
            const int defaultPort = 10223;
            var configuredPort = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT")
                ?? Environment.GetEnvironmentVariable("DEVFLOW_TEST_PORT");
            return int.TryParse(configuredPort, out var port) && port is > 0 and <= 65535
                ? port
                : defaultPort;
        }
#endif
    }
}
