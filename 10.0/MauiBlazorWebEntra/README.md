# .NET MAUI Blazor Hybrid and Web App with Microsoft Entra External ID

This .NET 10 sample signs customers in with [Microsoft Entra External ID](https://learn.microsoft.com/entra/external-id/customers/overview-customers-ciam), using the browser-hosted sign-up and sign-in flow. Home is public; Counter and Weather require sign-in.

| Project | Purpose |
| --- | --- |
| `MauiBlazorWebEntra` | Native MAUI Blazor Hybrid app, using MSAL.NET |
| `MauiBlazorWebEntra.Web` | Blazor Web App server, sign-in/sign-out endpoints, and protected `/api/weather` endpoint |
| `MauiBlazorWebEntra.Web.Client` | WebAssembly client, hosted by the server |
| `MauiBlazorWebEntra.Shared` | Shared Home, Counter, and Weather pages |

**Web:** Interactive Auto rendering with server-owned OpenID Connect sign-in and cookies through Microsoft.Identity.Web. WebAssembly receives serialized name/role authentication state and calls the API with the browser cookie; it doesn't acquire or store tokens.

**Native:** MSAL opens the system browser, acquires an API access token, and handles token caching and silent renewal. Bearer requests to `/api/weather` must have the `Weather.Read` delegated scope. ID tokens aren't API access tokens.

## Configure Microsoft Entra External ID

Use an **external tenant for customers**, not a workforce tenant or Azure AD B2C. Create a [sign-up and sign-in user flow](https://learn.microsoft.com/entra/external-id/customers/how-to-user-flow-sign-up-sign-in-customers), then register two applications with **Accounts in this organizational directory only** in that tenant and [associate both with the flow](https://learn.microsoft.com/entra/external-id/customers/how-to-user-flow-add-application).

| Registration | Required configuration |
| --- | --- |
| Web app and API | **Web** redirect URIs `https://localhost:7257/signin-oidc` and `https://localhost:7257/signout-callback-oidc`. Create a server credential. Expose `api://<web-and-api-client-id>/Weather.Read` and set `api.requestedAccessTokenVersion` to `2` in the app manifest. |
| Native public client | **Mobile and desktop applications** redirect URIs `msal<native-client-id>://auth` for Android/iOS/Mac Catalyst and `http://localhost` for Windows. Add the API's delegated `Weather.Read` permission and grant administrator consent. No native client secret or broker is used. |

Set the following sample configuration:

1. In `MauiBlazorWebEntra.Web/appsettings.json`, replace `AzureAd:Instance`, `TenantId`, and `ClientId` with the external tenant's `https://<tenant-subdomain>.ciamlogin.com/`, tenant ID, and **web/API** application ID.
1. Store the **web app** client secret locally, never in source control:

   ```bash
   dotnet user-secrets set "AzureAd:ClientSecret" "<client-secret>" --project MauiBlazorWebEntra.Web
   ```

1. In `MauiBlazorWebEntra/MsalConfig.cs`, set `ClientId` to the **native** application ID, `Authority` to `https://<tenant-subdomain>.ciamlogin.com/<tenant-id>`, and `Scopes` to `["api://<web-and-api-client-id>/Weather.Read"]`. The `22222222-...` client ID is only a placeholder.
1. In **both** `Platforms/iOS/Info.plist` and `Platforms/MacCatalyst/Info.plist`, replace the placeholder URL scheme with `msal<native-client-id>`. Android uses the constant from `MsalConfig.cs` automatically.

## Run the sample

Install the [.NET 10 SDK and MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0), plus the native tools for your target platform. Run commands from `10.0/MauiBlazorWebEntra`; `global.json` selects .NET 10.0.401 or a later patch in that SDK feature band, not a .NET 11 preview.

1. Trust the ASP.NET Core development certificate: `dotnet dev-certs https --trust`.
1. Start the server:

   ```bash
   dotnet run --project MauiBlazorWebEntra.Web --launch-profile https
   ```

1. Open `https://localhost:7257/` and log in. Counter and Weather become available. After WebAssembly downloads, reload and Home reports `WebAssembly`.
1. Open `MauiBlazorWebEntra.sln` in your IDE and run the native project. Select **Log In** to use the same tenant's hosted account flow.

## Notes

* **Backend address:** `MauiProgram.cs` uses `https://localhost:7257/`, or `https://10.0.2.2:7257/` for Android Debug builds. A physical device needs a reachable, trusted HTTPS address. The Debug-only local certificate exceptions apply to API calls, not to Microsoft Entra sign-in.
* **Sign-out:** Web sign-out clears the app cookie and ends the Entra browser session. Native sign-out removes MSAL's cached accounts/tokens locally; it doesn't revoke issued tokens or clear the system browser's sign-in cookies.
* **Apple platforms:** UIKit scenes are configured for iOS and Mac Catalyst. MSAL's iOS keychain group and the Mac Catalyst cache require the included keychain entitlements and suitable signing/provisioning. Mac Catalyst uses the reference tutorial's `ASWebAuthenticationSession` adapter because MSAL.NET has no Mac Catalyst target.
* This is a separate sample from `MauiBlazorWebIdentity`, with different app/project identities, server port, authentication cookie, and user-secrets ID. It has no local Identity database or account-management pages.

## References

* [Original Entra External ID Blazor Hybrid tutorial](https://github.com/mattleibow/ms-identity-ciam-dotnet-tutorial/tree/mattleibow/add-blazor-hybrid-maui/1-Authentication/3-sign-in-blazor-maui)
* [Secure a .NET 10 Blazor Web App with Microsoft Entra](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?view=aspnetcore-10.0)
* [MSAL.NET authentication for .NET MAUI](https://learn.microsoft.com/dotnet/maui/data-cloud/authentication?view=net-maui-10.0)
