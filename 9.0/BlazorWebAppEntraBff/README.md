# Blazor Web App with Entra (BFF Pattern)

This sample features:

- A Blazor Web App with global Auto interactivity.
  - This adds a `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services to the
    server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
- Authentication with Microsoft Entra using Microsoft Identity Web packages.
- A minimal API backend using the `JwtBearerHandler` to validate JWT tokens saved by the Blazor app using the in-memory token store for the access token.
- The BFF pattern using Aspire service discovery and YARP for proxying the requests to `/weatherforecast` on the backend with the `access_token` stored in the in-memory token store.

## Article for this sample app

The article for this sample app is [Secure an ASP.NET Core Blazor Web App with Microsoft Entra ID (BFF pattern)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=with-bff-pattern).

If you need to open an issue that pertains to the coding of the sample app, open [an issue on this samples repo](https://github.com/dotnet/blazor-samples/issues). Otherwise, open an issue using the **Open a documentation issue** link/feedback form at the bottom of [the article](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=with-bff-pattern).

## Configure the sample

Configure Entra following the guidance in [Secure an ASP.NET Core Blazor Web App with Microsoft Entra ID (BFF pattern)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=with-bff-pattern).

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppEntra` solution file in Visual Studio.
1. Select the `Aspire/Aspire.AppHost` project in **Solution Explorer** and start the app with either Visual Studio's Run button or by selecting **Start Debugging** from the **Debug** menu.

### .NET CLI

In a command shell, navigate to the `Aspire/Aspire.AppHost` project folder and use the `dotnet run` command to run the sample. The `BlazorWebAppEntra` and `MinimalApiJwt` projects are also started by the command.
