# .NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity sample app (`MauiBlazorWeb`)

This sample demonstrates .NET MAUI Blazor Hybrid and Web App that shares common UI and *authentication*. The sample uses ASP.NET Core Identity local accounts, but you can use this pattern for any authentication provider from a MAUI Blazor Hybrid client.

The sample:	

* Sets up the UI to show or hide pages based on user authentication.
* Sets up ASP.NET Identity endpoints for remote clients.
* Logs users in, logs users out, and refreshes tokens from the MAUI client.
* Saves and retrieves tokens in secure device storage.
* Calls a secure endpoint (`/api/weather`) from the client.

## Identity REST API exploration

The sample retains the framework stock API at `/identity` and adds proposed
portable account endpoints under `/identity` only where the framework does not
already provide a route. Enhanced replacements are deliberately isolated under
`/identity-overrides` so their behavior can be compared directly.

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
