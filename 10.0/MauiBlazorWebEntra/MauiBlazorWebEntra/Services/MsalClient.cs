using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Services;

public sealed class MsalClient
{
    public IPublicClientApplication Application { get; }

    public MsalClient(MsalConfig configuration)
    {
        if (configuration.Scopes is not { Length: > 0 } || configuration.Scopes.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty API scope is required.", nameof(configuration));
        }

        var builder = PublicClientApplicationBuilder.Create(configuration.ClientId)
            .WithAuthority(configuration.Authority)
            .WithRedirectUri(configuration.RedirectUri);

#if IOS
        builder.WithIosKeychainSecurityGroup(AppInfo.Current.PackageName);
#endif

        var client = builder.Build();

#if WINDOWS || MACCATALYST
        // These targets use MSAL's desktop assembly, which needs cache persistence.
        var cacheKey = $"msal_token_cache_{configuration.ClientId}";
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

        Application = client;
    }

    public AcquireTokenInteractiveParameterBuilder AcquireTokenInteractive(IEnumerable<string> scopes)
    {
        var request = Application.AcquireTokenInteractive(scopes);
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
