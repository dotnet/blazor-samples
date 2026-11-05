namespace MauiBlazorWebEntra;

public sealed record MsalConfig(string ClientId, string Authority, string[] Scopes)
{
    public const string ExternalIdClientId = "22222222-2222-2222-2222-222222222222";
    public const string WorkforceIdClientId = "44444444-4444-4444-4444-444444444444";

    public static MsalConfig ExternalId { get; } = new(
        ExternalIdClientId,
        "https://YOUR_TENANT_SUBDOMAIN.ciamlogin.com/YOUR_EXTERNAL_TENANT_ID",
        ["api://YOUR_EXTERNAL_WEB_AND_API_CLIENT_ID/Weather.Read"]);

    public static MsalConfig WorkforceId { get; } = new(
        WorkforceIdClientId,
        "https://login.microsoftonline.com/YOUR_WORKFORCE_TENANT_ID",
        ["api://YOUR_WORKFORCE_WEB_AND_API_CLIENT_ID/Weather.Read"]);

    public string RedirectUri =>
#if WINDOWS
        "http://localhost";
#else
        $"msal{ClientId}://auth";
#endif
}
