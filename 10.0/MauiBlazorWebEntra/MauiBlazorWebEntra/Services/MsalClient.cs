using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Services;

internal static class MsalClient
{
    public const string ExternalIdClientId = "22222222-2222-2222-2222-222222222222";
    public const string WorkforceIdClientId = "44444444-4444-4444-4444-444444444444";

    public static IReadOnlyDictionary<string, string?> ExternalId { get; } = new Dictionary<string, string?>
    {
        ["Entra:ClientId"] = ExternalIdClientId,
        ["Entra:Authority"] = "https://YOUR_TENANT_SUBDOMAIN.ciamlogin.com/YOUR_EXTERNAL_TENANT_ID",
        ["Entra:Scopes:0"] = "api://YOUR_EXTERNAL_WEB_AND_API_CLIENT_ID/Weather.Read"
    };

    public static IReadOnlyDictionary<string, string?> WorkforceId { get; } = new Dictionary<string, string?>
    {
        ["Entra:ClientId"] = WorkforceIdClientId,
        ["Entra:Authority"] = "https://login.microsoftonline.com/YOUR_WORKFORCE_TENANT_ID",
        ["Entra:Scopes:0"] = "api://YOUR_WORKFORCE_WEB_AND_API_CLIENT_ID/Weather.Read"
    };

    public static IPublicClientApplication Create(IConfiguration configuration)
    {
        var clientId = configuration["Entra:ClientId"]
            ?? throw new InvalidOperationException("Configuration 'Entra:ClientId' is missing.");
        var authority = configuration["Entra:Authority"]
            ?? throw new InvalidOperationException("Configuration 'Entra:Authority' is missing.");
#if WINDOWS
        var redirectUri = "http://localhost";
#else
        var redirectUri = $"msal{clientId}://auth";
#endif

        var builder = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(authority)
            .WithRedirectUri(redirectUri);

#if IOS
        builder.WithIosKeychainSecurityGroup(AppInfo.Current.PackageName);
#endif

        var client = builder.Build();

#if WINDOWS || MACCATALYST
        // These targets use MSAL's desktop assembly, which needs cache persistence.
        var cacheKey = $"msal_token_cache_{clientId}";
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
