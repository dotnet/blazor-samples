namespace MauiBlazorWebEntraWorkforce.Services;

/// <summary>
/// Configuration for Microsoft Entra workforce tenant authentication.
/// Run Setup-Azure.ps1 to populate these values automatically,
/// or replace them manually from the Azure portal.
/// </summary>
public static class MsalConfig
{
    // The Entra workforce tenant domain (for example, "contoso.onmicrosoft.com").
    public const string TenantDomain = "YOUR_TENANT_DOMAIN";

    // The Entra workforce tenant ID (a GUID).
    public const string TenantId = "YOUR_TENANT_ID";

    // The MAUI app (public client) registration client ID.
    public const string ClientId = "YOUR_MAUI_CLIENT_ID";

    // Authority for the workforce tenant.
    public static string Authority => $"https://login.microsoftonline.com/{TenantId}";

    // The API scope exposed by the web server app registration.
    // Format: api://{web-client-id}/access_as_user
    public const string ApiScope = "api://YOUR_WEB_CLIENT_ID/access_as_user";

    // Scopes to request when acquiring tokens.
    public static string[] Scopes => [ApiScope];

    // MSAL redirect URI for native apps.
#if WINDOWS
    // Windows uses http://localhost with the desktop auth experience handled in-process.
    public static string RedirectUri => "http://localhost";
#else
    // iOS/Android/Mac Catalyst use a custom scheme redirect.
    public static string RedirectUri => $"msal{ClientId}://auth";
#endif
}
