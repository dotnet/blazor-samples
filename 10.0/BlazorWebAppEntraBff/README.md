# Blazor Web App with Entra (without YARP and Aspire)

This sample features:

* A Blazor Web App with global Auto interactivity. `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services are added to the server and client Blazor apps to capture authentication state and flow it between the server and client.
* Authentication with Microsoft Entra using Microsoft Identity Web packages.
* A minimal API backend using the `JwtBearerHandler` to validate JWT tokens saved by the Blazor app using the in-memory token store for the access token.
* The BFF pattern using `MapGet` and `HttpClient` that forwards the requests to `/weatherforecast` to the backend using an access token generated for the web API on behalf of the user account for the user's claims.

## Article for this sample app

Sample app to accompany [Secure an ASP.NET Core Blazor Web App with Microsoft Entra ID (without YARP/Aspire)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=without-yarp-and-aspire).

Update the `NSwag.AspNetCore` NuGet package in the `MinimalApiJwt` project to the latest version.

## Use a production distributed token cache provider

The sample app uses in-memory distributed token caches, but a production distributed token cache provider is recommended for production apps. For more information, see [Use a production distributed token cache provider](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=without-yarp-and-aspire#use-a-production-distributed-token-cache-provider).
