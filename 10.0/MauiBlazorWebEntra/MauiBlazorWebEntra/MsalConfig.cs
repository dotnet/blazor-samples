namespace MauiBlazorWebEntra.Services;

/// <summary>
/// Configuration for Microsoft Entra External ID (CIAM) authentication.
/// Run Setup-Azure.ps1 to populate these values automatically,
/// or replace them manually from the Azure portal.
/// </summary>
public static class MsalConfig
{
    // The Entra External ID (CIAM) tenant name (e.g., "contoso" from contoso.ciamlogin.com)
    public const string TenantName = "YOUR_TENANT_NAME";

    // The CIAM tenant ID (a GUID)
    public const string TenantId = "YOUR_TENANT_ID";

    // The MAUI app (public client) registration client ID
    public const string ClientId = "YOUR_MAUI_CLIENT_ID";

    // Authority for CIAM tenant
    public static string Authority => $"https://{TenantName}.ciamlogin.com/{TenantId}";

    // The API scope exposed by the web server app registration
    // Format: api://{web-client-id}/access_as_user
    public const string ApiScope = "api://YOUR_WEB_CLIENT_ID/access_as_user";

    // Scopes to request when acquiring tokens
    public static string[] Scopes => [ApiScope];

    // MSAL redirect URI for native apps
#if WINDOWS
    // Windows uses http://localhost with embedded WebView2 (intercepted in-process, no listener)
    public static string RedirectUri => "http://localhost";
#else
    // iOS/Android/Mac Catalyst use custom scheme redirect
    public static string RedirectUri => $"msal{ClientId}://auth";
#endif
}


