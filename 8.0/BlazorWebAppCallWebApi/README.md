# Blazor Web App Call web API sample app

This sample app demonstrates how to call a web API running on a backend server app and running within the Blazor Web App's server project.

For more information, see [Call a web API from ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/call-web-api).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).

1. The default and fallback URLs for the two apps are:

   * `Backend` app (`BackendUrl`): `https://localhost:7212` (fallback: `https://localhost:5001`)
   * `BlazorApp` app (`FrontendUrl`): `https://localhost:7172` (fallback: `https://localhost:5002`)
   
   You can use the existing URLs or update them in the `appsettings.json` file of each project with new `BackendUrl` and `FrontendUrl` endpoints:

   * `appsettings.json` file in the root of the `Backend` app.
   * `BlazorApp/appsettings.json` file in the `BlazorApp` app.
  
1. If you plan to run the apps using the .NET CLI with `dotnet run`, note that first launch profile in the launch settings file is used to run an app, which is the insecure `http` profile (HTTP protocol). To run the apps securely (HTTPS protocol), take ***either*** of the following approaches:

   * Pass the launch profile option to the command when running the apps: `dotnet run -lp https`.
   * In the launch settings files (`Properties/launchSettings.json`) ***of both projects***, rotate the `https` profiles to the top, placing them above the `http` profiles.
  
   If you use Visual Studio to run the apps, Visual Studio automatically uses the `https` launch profile. No action is required to run the apps securely when using Visual Studio.

1. Run the `Backend` and `BlazorApp` apps.

1. Navigate to the `BlazorApp` app at the `FrontendUrl`.

1. Navigate to the pages that call the web APIs.
