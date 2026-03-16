using MauiBlazorWebEntra.Services;
using MauiBlazorWebEntra.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
#if WINDOWS
using Microsoft.Identity.Client.Desktop;
#endif

namespace MauiBlazorWebEntra;

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
        var msalBuilder = PublicClientApplicationBuilder
            .Create(MsalConfig.ClientId)
            .WithAuthority(MsalConfig.Authority)
            .WithRedirectUri(MsalConfig.RedirectUri)
            .WithIosKeychainSecurityGroup("com.companyname.MauiBlazorWebEntra");

#if WINDOWS
        // Windows: use embedded WebView2 browser + WAM broker for authentication.
        // WAM doesn't support Entra External ID (CIAM), so the embedded WebView2
        // provides the sign-in UI instead of opening the system browser.
        msalBuilder.WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
#endif

        var msalClient = msalBuilder.Build();

#if WINDOWS
        // Windows: persist MSAL token cache to SecureStorage (DPAPI-backed).
        // iOS and Android persist automatically via Keychain / SharedPreferences.
        const string cacheKey = "msal_token_cache";
        msalClient.UserTokenCache.SetBeforeAccessAsync(async args =>
        {
            var cached = await SecureStorage.GetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                args.TokenCache.DeserializeMsalV3(Convert.FromBase64String(cached));
            }
        });
        msalClient.UserTokenCache.SetAfterAccessAsync(async args =>
        {
            if (args.HasStateChanged)
            {
                var data = args.TokenCache.SerializeMsalV3();
                await SecureStorage.SetAsync(cacheKey, Convert.ToBase64String(data));
            }
        });
#endif

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
