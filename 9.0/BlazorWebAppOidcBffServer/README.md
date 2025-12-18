# Blazor Web App with OpenID Connect (OIDC) (without YARP and Aspire)

This sample features:

* A Blazor Web App with global Server interactivity.
* OIDC authentication with Microsoft Entra without using Entra-specific packages. This sample can be used as a starting point for any OIDC authentication flow.
* Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.
* Secure web API call for weather data to a separate web API project. The access token is obtained from the server-side `HttpContext` and attached to outgoing requests with a `DelegatingHandler` service.

## Article for this sample app

The article for this sample app is [Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=without-yarp-aspire-server).

If you need to open an issue that pertains to the coding of the sample app, open [an issue on this samples repo](https://github.com/dotnet/blazor-samples/issues). Otherwise, open an issue using the **Open a documentation issue** link/feedback form at the bottom of [the article](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=without-yarp-aspire-server).

## Configure the sample

Configure the OIDC provider using the comments in the `Program.cs` file and the article.

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppOidcServer` solution file in Visual Studio.
1. If the **Start Projects** launch profile isn't present, use the following guidance to create one.
   * Open the **Start** button menu with the small triangle next to the **Start** button.
   * Select **Configure Startup Projects**.
   * Select **Multiple startup projects**.
   * Name the launch profile (for example, "Start Projects").
   * Select the start Action for the two projects. You can start either or both projects with or without debugging depending on if you plan to debug app code.
   * Save the launch profile with the **OK** button.
1. With the multiple projects launch profile showing next to the **Start** button, select the **Start** button to start the projects.

### .NET CLI

In a command shell:

1. Navigate to the `MinimalApiJwt` project folder and use the `dotnet run` command to run the project.
1. Navigate to the `BlazorWebAppOidcServer` project folder and use the `dotnet watch` command to run the project.
