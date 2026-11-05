# .NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity sample app (`MauiBlazorWeb`)

This .NET 10 sample logs users in with ASP.NET Core Identity local accounts from both a .NET MAUI Blazor Hybrid app and a Blazor Web App that share one Razor class library. Home is public; Counter and Weather require login.

| Project | Purpose |
| --- | --- |
| `MauiBlazorWeb` | Native MAUI Blazor Hybrid app |
| `MauiBlazorWeb.Web` | Server: Identity, Identity API (`/identity/*`), and the protected `/api/weather` endpoint |
| `MauiBlazorWeb.Web.Client` | WebAssembly client, hosted by the server |
| `MauiBlazorWeb.Shared` | Shared Home, Counter, and Weather pages |

The two apps log in differently:

* **Web:** [Interactive Auto rendering](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0#automatic-auto-rendering) with static server-rendered Identity pages. The browser sends the Identity cookie, and the WebAssembly client gets the user from serialized authentication state.
* **Native:** Uses the Identity API's bearer and refresh tokens (opaque, not JWTs), refreshed automatically. **Remember me** saves them in `SecureStorage` so the login survives restarts; otherwise they stay in memory. Storage is best-effort: errors are logged, not shown. **Log Out** clears the tokens locally without revoking them on the server. Two-factor authentication isn't supported.

## Prerequisites

* [.NET 10 SDK and .NET MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0) (`dotnet workload install maui`), and Visual Studio 2026 or Visual Studio Code with the .NET MAUI extension.
* A platform to run on: Android 7.0+ (API 24), iOS 15+, Mac Catalyst 17+ (macOS 14+), or Windows 10 1809+. Apple targets need an Xcode version that your .NET Apple workload supports.
* The ASP.NET Core development certificate, trusted once: `dotnet dev-certs https --trust`.

## Steps to run the sample

1. Clone this repository or [download a ZIP archive](https://learn.microsoft.com/aspnet/core/fundamentals/#how-to-download-a-sample).
1. From `10.0/MauiBlazorWebIdentity`, start the web app on `https://localhost:7157`, the address the native app calls (or run it from your IDE with the `https` profile):

   ```bash
   dotnet run --project MauiBlazorWeb.Web --launch-profile https
   ```

   Development startup creates a SQLite database at `MauiBlazorWeb.Web/Data/app.db`. Delete it to reset the accounts.
1. Open `MauiBlazorWeb.sln` and run `MauiBlazorWeb` on an Android emulator, iOS simulator, Mac Catalyst, or Windows.
1. Register and confirm an account (next section), then log in. Counter and Weather appear. Log out and they disappear.
1. Browse to `https://localhost:7157/` and log in with the same account. After the WebAssembly client downloads, reload the page and Home says it's running on `WebAssembly`.

## Registration and confirmation

The server requires a confirmed account. Register at `https://localhost:7157/Account/Register`, or from the native app's **Register** page, which posts to `/identity/register` without logging in. **Continue registration** then opens the server's `/Account/RegisterConfirmation` page in the system browser.

The server owns that page. With the sample's no-op email sender, it shows a **Click here to confirm your account** link. Before real use, [configure an email sender](https://aka.ms/aspaccountconf) and remove that demo-link block from `RegisterConfirmation.razor`, because it lets anyone confirm any account. The page then asks users to check their email, and the native app needs no changes.

The system browser doesn't use the native app's Debug-only certificate exception. If it shows a certificate warning (common on simulators and emulators), open `https://localhost:7157/Account/RegisterConfirmation?email=<your-email>` in your desktop browser instead.

## Notes

* **Backend address:** `MauiProgram.cs` sets `BackendUrl` to `https://localhost:7157/`, or `https://10.0.2.2:7157/` in Android Debug builds (the emulator's alias for your machine). For a physical device, use an HTTPS address that it can reach and trust. See [Connect to local web services](https://learn.microsoft.com/dotnet/maui/data-cloud/local-web-services?view=net-maui-10.0).
* **Apple platforms:** The projects already include the [UIKit scene lifecycle](https://learn.microsoft.com/dotnet/maui/ios/uikit-scene-lifecycle?view=net-maui-10.0) that Xcode 27 builds require. Mac Catalyst also needs a provisioning profile with the keychain entitlement from `Platforms/MacCatalyst/Entitlements.plist` ([capabilities](https://learn.microsoft.com/dotnet/maui/mac-catalyst/capabilities?view=net-maui-10.0)), because ad hoc signed builds don't launch. The iOS simulator is the quickest Apple target.

## More information

* [.NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/hybrid/security/maui-blazor-web-identity?view=aspnetcore-10.0)
* [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
