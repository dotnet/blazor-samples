# .NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity sample app (`MauiBlazorWeb`)

This .NET 10 sample combines a .NET MAUI Blazor Hybrid app, a Blazor Web App, and its WebAssembly client. They share Razor pages and service contracts while using ASP.NET Core Identity local accounts. The WebAssembly client is served by the web host, not run as a standalone app.

The four projects are:

* `MauiBlazorWeb.Shared`: runtime-neutral Home, Counter, and Weather pages and service contracts.
* `MauiBlazorWeb.Web`: ASP.NET Core Identity, static SSR account pages, interactive server rendering, and the authorized `/api/weather` endpoint.
* `MauiBlazorWeb.Web.Client`: browser-side routing, layout, authentication-state deserialization, and same-origin weather requests.
* `MauiBlazorWeb`: native MAUI Blazor Hybrid app, with bearer tokens kept in secure device storage.

The web app uses **InteractiveAuto**: initially it renders interactively on the server, then uses WebAssembly on later full page loads once the client has downloaded. Identity Account pages remain static SSR. The server serializes authentication state (name and role claims) for the WebAssembly client; the browser relies on the server's Identity cookie for same-origin API requests, not a browser-stored bearer token. The MAUI app continues to authenticate with the `/identity` API and bearer tokens. Sign-in and sign-out use full reloads so the browser's authentication state is refreshed; authorization of weather data is also enforced by the server endpoint.

In the MAUI app, **Remember me** controls token persistence for both login and refresh. When selected, tokens are saved in secure device storage and restored on later app launches. Otherwise, tokens remain in memory, including after refresh, and any previously saved login is removed. Logout clears both in-memory and stored tokens.

The weather API uses typed JSON results so unauthenticated requests receive HTTP 401 instead of an HTML login redirect. Unauthenticated page requests still redirect to the Identity login page.

For background, see [MAUI Blazor Hybrid with ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/security/maui-blazor-web-identity?view=aspnetcore-10.0) and [Blazor authentication state and Interactive Auto security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0#secure-data-in-blazor-web-apps-with-interactive-auto-rendering). This structure follows the .NET 10 Individual Accounts template generated with SDK 10, for example: `dotnet new blazor --name IdentityReference --framework net10.0 --auth Individual --interactivity Auto --all-interactive --no-restore --no-update-check`. The sample retains `AddIdentityApiEndpoints` for MAUI instead of replacing it with the template's cookie-only Identity registration.

The iOS and Mac Catalyst apps use the [UIKit scene lifecycle](https://learn.microsoft.com/dotnet/maui/ios/uikit-scene-lifecycle?view=net-maui-10.0) with .NET MAUI 10.0.110. For Xcode 27.0 builds, install a compatible .NET 10 Apple workload, such as [.NET SDK 10.0.401 with workload set 10.0.401.1](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode27.0-10722). The minimum deployment targets are iOS 15.0 and Mac Catalyst 17.0 (macOS 14).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).
1. Make sure you have the [.NET 10 SDK and the .NET MAUI workload installed](https://learn.microsoft.com/dotnet/maui/get-started/installation).
1. Open the solution in Visual Studio 2026 or later, or VS Code with the .NET MAUI extension installed.
1. Set the `MauiBlazorWeb` MAUI project as the startup project. In Visual Studio, right-click the project and select **Set as Startup Project**.
1. Start the `MauiBlazorWeb.Web` project without debugging. In Visual Studio, right-click on the project and select **Debug** > **Start without Debugging**.
1. Inspect the generated OpenAPI document at `https://localhost:7157/openapi/v1.json` (the sample doesn't include Swagger UI).
1. Navigate to `https://localhost:7157/account/register` to register a user in the Blazor Web App. Immediately after the user is registered, use the **Click here to confirm your account** link in the UI to confirm the user's email address because a real email sender isn't registered for account confirmation.
1. Start (`F5`) the `MauiBlazorWeb` MAUI project. You can set the debug target to either **Windows** or an Android emulator.
1. Notice you can only see the `Home` and `Login` pages.
1. Log in with the user that you registered.
1. Notice you can now see the shared `Counter` and `Weather` pages.
1. Log out and notice you can only see the `Home` and `Login` pages again.
1. Navigate to `https://localhost:7157/` and the web app behaves the same. After the WebAssembly download, reload the page to switch from server interactivity to WebAssembly. The shared Home page reports `WebAssembly` as the platform in the browser client.
