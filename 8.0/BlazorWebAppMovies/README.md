# Blazor Web App Movies tutorial sample app

This sample app is the completed app for the Blazor Web App Movies tutorial:

[Build a Blazor movie database app (Overview)](https://learn.microsoft.com/aspnet/core/blazor/tutorials/movie-database-app/)

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).

1. The default URLs for the app are `https://localhost:7073` (secure HTTPs) and `http://localhost:5261` (insecure HTTP).
   
   You can use the existing URLs or update them in the `Properties/launchSettings.json` file.
  
1. If you plan to run the apps using the .NET CLI with `dotnet run`, note that first launch profile in the launch settings file is used to run an app, which is the insecure `http` profile (HTTP protocol). To run the apps securely (HTTPS protocol), take ***either*** of the following approaches:

   * Pass the launch profile option to the command when running the apps: `dotnet run -lp https`.
   * In the launch settings file (`Properties/launchSettings.json`), rotate the `https` profile to the top, placing it above the `http` profile.
  
   If you use Visual Studio to run the apps, Visual Studio automatically uses the `https` launch profile. No action is required to run the apps securely when using Visual Studio.

1. Run the app.
