# `BlazorWebAppEntra`

Sample app to accompany [Secure an ASP.NET Core Blazor Web App with Microsoft Entra ID](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=non-bff-pattern).

Update the `NSwag.AspNetCore` NuGet package in the `MinimalApiJwt` project to the latest version.

## Use a production distributed token cache provider

The sample app uses in-memory distributed token caches, but a production distributed token cache provider is recommended for production apps. For more information, see [Use a production distributed token cache provider](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?pivots=non-bff-pattern#use-a-production-distributed-token-cache-provider).
