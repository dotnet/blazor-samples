# Blazor Web App with OpenID Connect (OIDC) (without YARP and Aspire)

This sample features:

* A Blazor Web App with global Auto interactivity.
* `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services are added to the server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
* OIDC authentication with Microsoft Entra without using Entra-specific packages. This sample can be used as a starting point for any OIDC authentication flow.
* Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.
* Secure web API call for weather data to a separate web API project. The access token is obtained from the server-side `HttpContext` and attached to outgoing requests with a custom `DelegatingHandler` service.

## Article for this sample app

The article for this sample app is [Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=without-yarp-aspire).

If you need to open an issue that pertains to the coding of the sample app, open [an issue on this samples repo](https://github.com/dotnet/blazor-samples/issues). Otherwise, open an issue using the **Open a documentation issue** link/feedback form at the bottom of [the article](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=non-bff-pattern).

## Configure the sample

Configure the OIDC provider using the comments in the `Program.cs` file.

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppOidc` solution file in Visual Studio.
1. Use the **Start Projects** launch profile to start the web API app and Blazor apps.

### .NET CLI

In a command shell:

* Navigate to the `MinimalApiJwt` project folder and use the `dotnet run` command to run the project.
* Navigate to the `BlazorWebAppOidc` project folder and use the `dotnet watch` command to run the project.
