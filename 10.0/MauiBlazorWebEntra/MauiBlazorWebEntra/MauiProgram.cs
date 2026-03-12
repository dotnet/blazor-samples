using MauiBlazorWebEntra.Services;
using MauiBlazorWebEntra.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra
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
#endif

            // Build MSAL public client for Entra External ID authentication
            var msalClient = PublicClientApplicationBuilder
                .Create(MsalConfig.ClientId)
                .WithAuthority(MsalConfig.Authority)
                .WithRedirectUri(MsalConfig.RedirectUri)
                .Build();

            builder.Services.AddSingleton(msalClient);

            // Register authentication services
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<MsalAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp
                => sp.GetRequiredService<MsalAuthenticationStateProvider>());

            // Add device-specific services used by the MauiBlazorWebEntra.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddScoped<IWeatherService, WeatherService>();

            return builder.Build();
        }
    }
}
