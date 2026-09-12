# .NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity sample app (`MauiBlazorWeb`)

This sample demonstrates .NET MAUI Blazor Hybrid and Web App that shares common UI and *authentication*. The sample uses ASP.NET Core Identity local accounts, but you can use this pattern for any authentication provider from a MAUI Blazor Hybrid client.

The sample:	

* Sets up the UI to show or hide pages based on user authentication.
* Sets up ASP.NET Identity endpoints for remote clients.
* Logs users in, logs users out, and refreshes tokens from the MAUI client.
* Saves and retrieves tokens in secure device storage.
* Calls a secure endpoint (`/api/weather`) from the client.

## Identity REST API exploration

The sample retains the framework stock API at `/identity`. It adds
contribution-oriented first-party account endpoints under `/identity` only
where the framework has no route, while enhanced replacements are deliberately
isolated at `/identity-overrides`. This prevents route collisions and makes
stock versus proposed behavior directly comparable.

* [Identity REST API implementation tracker](IDENTITY_API_IMPLEMENTATION.md)
  records the phased status, endpoint ledger, and validation results.
* [Identity REST API capability matrix](IDENTITY_API_CAPABILITIES.md) maps every
  hosted `/Account/*` feature to stock, override, or proposed new REST support.

The access and refresh tokens are opaque Data Protection tickets, not JWTs.
Access tokens default to one hour and refresh tokens to 14 days. Refresh tokens
are reusable and there is no device registry, replay detection, or individual
token revocation. Updating the security stamp invalidates refresh tokens but
does not invalidate an issued access token before it expires. Production hosts
must share persistent Data Protection keys.

The completed portable surface includes stable login outcomes, extended account
profile/password/phone operations, 2FA status, safe personal-data projection,
password-confirmed account deletion, security-stamp logout-all, and linked
provider listing/unlinking. It also exposes official Identity passkey ceremony
begin/finish routes and passkey management. See the capability matrix for every
hosted feature's full, partial, or deferred classification and test mapping.

The net10 MAUI UI lists/removes passkeys and previews the registration-begin
JSON. It intentionally defers real native WebAuthn `CreateAsync`/`AssertAsync`
calls to the .NET 11 MAUI passkey API seam. Successful passkey ceremonies
require an authenticator, so integration tests cover the official temporary
cookie and the stable missing-cookie failure rather than fabricated protocol
internals. Likewise, external-provider browser challenge/callback handoff is
deferred until a real provider is configured; the REST surface safely manages
links that already exist.

`/development/notifications` is a bounded in-memory development aid, available
only in the Development environment. It is not a production email sender.
Configure a real `IEmailSender` for deployment, persist/share Data Protection
keys, use HTTPS, and apply normal platform signing/provisioning for MAUI
packages. Local unsigned Mac Catalyst development warnings are expected and are
not a deployment configuration.

For more information, see [.NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/hybrid/security/maui-blazor-web-identity).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).
1. Make sure you have [.NET 10 and the MAUI workload installed](https://learn.microsoft.com/dotnet/maui/get-started/installation).
1. Open the solution in Visual Studio 2022 or VS Code with the .NET MAUI extension installed.
1. Set the `MauiBlazorWeb` MAUI project as the startup project. In Visual Studio, right-click the project and select **Set as Startup Project**.
1. Start the `MauiBlazorWeb.Web` project without debugging. In Visual Studio, right-click on the project and select **Debug** > **Start without Debugging**.
1. Inspect the Identity endpoints by navigating to `https://localhost:7157/swagger` in a browser.
1. Navigate to `https://localhost:7157/account/register` to register a user in the Blazor Web App. In Development, open `https://localhost:7157/development/notifications` and use the explicit confirmation action. This bounded, in-memory page is not mapped outside Development and is not production email.
1. Start (`F5`) the `MauiBlazorWeb` MAUI project. You can set the debug target to either **Windows** or an Android emulator.
1. Notice you can only see the `Home` and `Login` pages.
1. Log in with the user that you registered.
1. Notice you can now see the shared `Counter` and `Weather` pages.
1. Log out and notice you can only see the `Home` and `Login` pages again.
1. Navigate to `https://localhost:7157/` and the web app behaves the same.
