using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Services;

internal static class MsalClient
{
    public static IPublicClientApplication Create()
    {
        var builder = PublicClientApplicationBuilder.Create(MsalConfig.ClientId)
            .WithAuthority(MsalConfig.Authority)
            .WithRedirectUri(MsalConfig.RedirectUri);

#if IOS
        builder.WithIosKeychainSecurityGroup(AppInfo.Current.PackageName);
#endif

        var client = builder.Build();

#if WINDOWS || MACCATALYST
        // These targets use MSAL's desktop assembly, which needs cache persistence.
        const string cacheKey = "msal_token_cache";
        client.UserTokenCache.SetBeforeAccessAsync(async args =>
        {
            var cache = await SecureStorage.Default.GetAsync(cacheKey);
            if (cache is not null)
            {
                args.TokenCache.DeserializeMsalV3(Convert.FromBase64String(cache));
            }
        });
        client.UserTokenCache.SetAfterAccessAsync(async args =>
        {
            if (args.HasStateChanged)
            {
                await SecureStorage.Default.SetAsync(cacheKey,
                    Convert.ToBase64String(args.TokenCache.SerializeMsalV3()));
            }
        });
#endif

        return client;
    }

    public static AcquireTokenInteractiveParameterBuilder WithPlatformBrowser(
        this AcquireTokenInteractiveParameterBuilder request)
    {
#if MACCATALYST
        return request.WithMacCatalystWebView();
#else
        request = request.WithUseEmbeddedWebView(false);
#if ANDROID
        request = request.WithParentActivityOrWindow(Platform.CurrentActivity
            ?? throw new InvalidOperationException("No Android activity is available for sign-in."));
#elif IOS
        request = request.WithParentActivityOrWindow(Platform.GetCurrentUIViewController()
            ?? throw new InvalidOperationException("No iOS view controller is available for sign-in."));
#endif
        return request;
#endif
    }
}
