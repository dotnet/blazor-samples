using Microsoft.Identity.Client;
#if WINDOWS
using Microsoft.Identity.Client.Desktop;
#endif

namespace MauiBlazorWebEntra.Services;

internal static class MsalServiceExtensions
{
    /// <summary>
    /// Registers the MSAL public client application and authentication services.
    /// </summary>
    public static IServiceCollection AddMsalClient(this IServiceCollection services)
    {
        var msalBuilder = PublicClientApplicationBuilder
            .Create(MsalConfig.ClientId)
            .WithAuthority(MsalConfig.Authority)
            .WithRedirectUri(MsalConfig.RedirectUri);

#if WINDOWS
        // Windows: use embedded WebView2 browser + WAM broker for authentication.
        // WAM doesn't support Entra External ID (CIAM), so the embedded WebView2
        // provides the sign-in UI instead of opening the system browser.
        msalBuilder.WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
#endif

        var msalClient = msalBuilder.Build();

#if WINDOWS || MACCATALYST
        // MSAL persists tokens natively on iOS (Keychain) and Android (SharedPreferences).
        // Windows and Mac Catalyst need manual persistence — Windows because it uses a
        // generic .NET assembly, Mac Catalyst because MSAL doesn't ship a maccatalyst TFM yet.
        // Remove MACCATALYST from this condition once MSAL ships Mac Catalyst support:
        // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527
        msalClient.EnableSecureStorageTokenCachePersistence();
#endif

        services.AddSingleton<IPublicClientApplication>(msalClient);

        return services;
    }

    /// <summary>
    /// Persists the MSAL token cache to SecureStorage on platforms that don't
    /// automatically persist (Windows). iOS uses Keychain and Android uses
    /// SharedPreferences, both handled natively by MSAL.
    /// </summary>
    private static void EnableSecureStorageTokenCachePersistence(this IPublicClientApplication msalClient)
    {
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
    }

    /// <summary>
    /// Applies platform-specific options to the MSAL interactive token request.
    /// On Android, sets the parent activity. On iOS/Mac Catalyst, configures
    /// system web view. On Windows, sets the WinUI3 parent window for the
    /// embedded WebView2 auth dialog.
    /// </summary>
    public static AcquireTokenInteractiveParameterBuilder WithPlatformOptions(this AcquireTokenInteractiveParameterBuilder builder)
    {
#if ANDROID

        if (Platform.CurrentActivity is not Android.App.Activity activity)
            throw new InvalidOperationException("No running acivity found.");

        return builder.WithParentActivityOrWindow(activity);

#elif IOS

        return builder.WithSystemWebViewOptions(new SystemWebViewOptions());

#elif MACCATALYST

        // MSAL doesn't ship a maccatalyst TFM yet, so WithSystemWebViewOptions
        // throws PlatformNotSupportedException. Use WithMacCatalystWebView() which
        // drives ASWebAuthenticationSession via ICustomWebUi.
        // Remove this #elif once MSAL ships Mac Catalyst support:
        // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527
        return builder.WithMacCatalystWebView();

#elif WINDOWS

        if (IPlatformApplication.Current?.Application is not IApplication app)
            throw new InvalidOperationException("No running application found.");
        if (app.Windows.FirstOrDefault() is not IWindow win || win.Handler?.PlatformView is not Microsoft.UI.Xaml.Window platformWindow)
            throw new InvalidOperationException("No running window found.");

        return builder.WithParentActivityOrWindow(platformWindow);

#else
        return builder;
#endif
    }
}
