namespace MauiBlazorWebEntra;

internal static class MsalConfig
{
    public const string ClientId = "22222222-2222-2222-2222-222222222222";
    public const string Authority = "https://YOUR_TENANT_SUBDOMAIN.ciamlogin.com/YOUR_TENANT_ID";
    public static readonly string[] Scopes = ["api://YOUR_WEB_AND_API_CLIENT_ID/Weather.Read"];

    public static string RedirectUri =>
#if WINDOWS
        "http://localhost";
#else
        $"msal{ClientId}://auth";
#endif
}
