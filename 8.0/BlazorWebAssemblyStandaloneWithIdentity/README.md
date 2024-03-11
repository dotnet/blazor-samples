# Standalone Blazor WebAssembly with ASP.NET Core Identity

This sample app demonstrates how to use the built-in ASP.NET Core Identity capabilities from a standalone Blazor WebAssembly app.

For more information, see [Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).

1. The default and fallback URLs for the two apps are:

   * `Backend` app (`BackendUrl`): `https://localhost:7211` (fallback: `https://localhost:5001`)
   * `BlazorWasmAuth` app (`FrontendUrl`): `https://localhost:7171` (fallback: `https://localhost:5002`)
   
   You can use the existing URLs or update them in the `appsettings.json` file of each project with new `BackendUrl` and `FrontendUrl` endpoints:

   * `appsettings.json` file in the root of the `Backend` app.
   * `wwwroot/appsettings.json` file in the `BlazorWasmAuth` app.
  
1. If you plan to run the apps using the .NET CLI with `dotnet run`, note that first launch profile in the launch settings file is used to run an app, which is the insecure `http` profile (HTTP protocol). To run the apps securely (HTTPS protocol), take ***either*** of the following approaches:

   * Pass the launch profile option to the command when running the apps: `dotnet run -lp https`.
   * In the launch settings files (`Properties/launchSettings.json`) ***of both projects***, rotate the `https` profiles to the top, placing them above the `http` profiles.
  
   If you use Visual Studio to run the apps, Visual Studio automatically uses the `https` launch profile. No action is required to run the apps securely when using Visual Studio.

1. Run the `Backend` and `BlazorWasmAuth` apps.

1. Navigate to the `BlazorWasmAuth` app at the `FrontendUrl`.

1. Register a new user using the **Register** link in the upper-right corner of the app's UI or use one of the preregistered test users:

   * `leela@contoso.com` (Password: `Passw0rd!`). Leela has `Administrator`, `Manager`, and `User` roles and can access the private manager page but not the private editor page of the app. She can process data with both forms on the data processing page.
   * `harry@contoso.com` (Password: `Passw0rd!`). Harry only has the `User` role and can't access the manager and editor pages. He can only process data with the first form on the data processing page.

1. Log in with the user.

1. Navigate to the private page (`Components/Pages/PrivatePage.razor` at `/private-page`) that only authenticated users can reach. A link to the page appears in the navigation sidebar after the user is authenticated. Navigate to the private manager and editor pages to explore how the user's roles influence the pages that they can visit. Navigate to the data processing page (`Components/Pages/DataProcessing.razor` at `/data-processing`) to experience authenticated and authorized data processing web API calls.

1. Log out of the app.
