# .NET MAUI Blazor Hybrid and Web App with Microsoft Entra

This .NET 10 sample supports Microsoft Entra External ID customer tenants or Microsoft Entra workforce tenants, using one configuration at startup. Home is public; Counter and Weather require sign-in.

| Project | Purpose |
| --- | --- |
| `MauiBlazorWebEntra` | Native MAUI Blazor Hybrid app, using MSAL.NET |
| `MauiBlazorWebEntra.Web` | Blazor Web App server, sign-in/sign-out endpoints, and protected `/api/weather` endpoint |
| `MauiBlazorWebEntra.Web.Client` | WebAssembly client, hosted by the server |
| `MauiBlazorWebEntra.Shared` | Shared Home, Counter, and Weather pages |

**Web:** Interactive Auto rendering with server-owned OpenID Connect sign-in and cookies through Microsoft.Identity.Web. WebAssembly receives serialized name/role authentication state and calls the API with the browser cookie; it doesn't acquire or store tokens.

**Native:** MSAL opens the system browser, acquires an API access token, and handles token caching and silent renewal. Bearer requests to `/api/weather` must have the `Weather.Read` delegated scope. ID tokens aren't API access tokens.

## Select a tenant type

Select the configuration in `MauiBlazorWebEntra.Web/Program.cs`:

```csharp
var entraConfiguration = builder.Configuration.GetRequiredSection("EntraExternalId"); // Or "EntraWorkforceId".
```

Select the matching configuration in `MauiBlazorWebEntra/MauiProgram.cs`:

```csharp
builder.Services.AddSingleton(MsalConfig.ExternalId); // Or MsalConfig.WorkforceId.
builder.Services.AddSingleton<MsalClient>();
```

External ID is the default. The selected native `MsalConfig` record is registered once; DI constructor-injects it into `MsalClient` and `MsalAuthenticationService`. Use the same tenant type in both apps, then rebuild/restart them. This is a startup choice, not simultaneous sign-in to two tenants. Browser cookies are isolated between profiles, and MSAL uses the selected application's token cache.

## Configure the selected tenant

| Profile | Web configuration section | Native configuration in `MsalConfig.cs` |
| --- | --- | --- |
| External ID | `EntraExternalId`, with instance `https://<tenant-subdomain>.ciamlogin.com/` | `ExternalIdClientId` and `ExternalId`: authority `https://<tenant-subdomain>.ciamlogin.com/<tenant-id>` |
| Workforce | `EntraWorkforceId`, with instance `https://login.microsoftonline.com/` | `WorkforceIdClientId` and `WorkforceId`: authority `https://login.microsoftonline.com/<tenant-id>` |

For External ID, use an **external tenant for customers**, not Azure AD B2C. Create a [sign-up and sign-in user flow](https://learn.microsoft.com/entra/external-id/customers/how-to-user-flow-sign-up-sign-in-customers) and [associate both applications with it](https://learn.microsoft.com/entra/external-id/customers/how-to-user-flow-add-application). Workforce tenants don't use that customer user flow.

Register two applications with **Accounts in this organizational directory only** in the selected tenant:

| Registration | Required configuration |
| --- | --- |
| Web app and API | **Web** redirect URIs `https://localhost:7257/signin-oidc` and `https://localhost:7257/signout-callback-oidc`. Create a server credential. Expose `api://<web-and-api-client-id>/Weather.Read` and set `api.requestedAccessTokenVersion` to `2` in the app manifest. |
| Native public client | **Mobile and desktop applications** redirect URIs `msal<native-client-id>://auth` for Android/iOS/Mac Catalyst and `http://localhost` for Windows. Add the API's delegated `Weather.Read` permission and grant administrator consent. No native client secret or broker is used. |

1. In `MauiBlazorWebEntra.Web/appsettings.json`, fill in the selected section's `Instance`, `TenantId`, and **web/API** `ClientId`.
1. Store the **web app** client secret locally, never in source control. Use the selected section name:

   ```bash
   dotnet user-secrets set "EntraExternalId:ClientSecret" "<client-secret>" --project MauiBlazorWebEntra.Web
   ```

   For workforce, use `EntraWorkforceId:ClientSecret` instead.
1. In `MauiBlazorWebEntra/MsalConfig.cs`, set the selected profile's **native** client ID, authority, and scope `api://<web-and-api-client-id>/Weather.Read`. The `22222222-...` External ID and `44444444-...` workforce client IDs are placeholders.
1. In **both** `Platforms/iOS/Info.plist` and `Platforms/MacCatalyst/Info.plist`, replace that profile's placeholder URL scheme with `msal<native-client-id>`. Both profiles' callbacks are registered so switching the selected configuration doesn't require changing the manifests again. Android uses the two constants from `MsalConfig.cs` automatically.

## Run the sample

Install the [.NET 10 SDK and MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0), plus the native tools for your target platform. Run commands from `10.0/MauiBlazorWebEntra`; `global.json` selects .NET 10.0.401 or a later patch in that SDK feature band, not a .NET 11 preview.

1. Trust the ASP.NET Core development certificate: `dotnet dev-certs https --trust`.
1. Start the server:

   ```bash
   dotnet run --project MauiBlazorWebEntra.Web --launch-profile https
   ```

1. Open `https://localhost:7257/` and log in. Counter and Weather become available. After WebAssembly downloads, reload and Home reports `WebAssembly`.
1. Open `MauiBlazorWebEntra.sln` in your IDE and run the native project. Select **Log In** to use the selected tenant's hosted sign-in.

## Notes

* **Backend address:** `MauiProgram.cs` uses `https://localhost:7257/`, or `https://10.0.2.2:7257/` for Android Debug builds. A physical device needs a reachable, trusted HTTPS address. The Debug-only local certificate exceptions apply to API calls, not to Microsoft Entra sign-in.
* **Sign-out:** Web sign-out clears the app cookie and ends the Entra browser session. Native sign-out removes the selected app's MSAL cached accounts/tokens locally; it doesn't revoke issued tokens or clear the system browser's sign-in cookies.
* **Apple platforms:** UIKit scenes are configured for iOS and Mac Catalyst. MSAL's iOS keychain group and the Mac Catalyst cache require the included keychain entitlements and suitable signing/provisioning. Mac Catalyst uses the reference tutorial's `ASWebAuthenticationSession` adapter because MSAL.NET has no Mac Catalyst target.
* **Workforce policies:** This sample uses browser sign-in without a broker. Organizations that require broker/device-compliance integration need additional platform setup.
* This is a separate sample from `MauiBlazorWebIdentity`, with different app/project identities, server port, authentication cookie, and user-secrets ID. It has no local Identity database or account-management pages.

## References

* [Original Entra External ID Blazor Hybrid tutorial](https://github.com/mattleibow/ms-identity-ciam-dotnet-tutorial/tree/mattleibow/add-blazor-hybrid-maui/1-Authentication/3-sign-in-blazor-maui)
* [Secure a .NET 10 Blazor Web App with Microsoft Entra](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?view=aspnetcore-10.0)
* [MSAL.NET authentication for .NET MAUI](https://learn.microsoft.com/dotnet/maui/data-cloud/authentication?view=net-maui-10.0)
