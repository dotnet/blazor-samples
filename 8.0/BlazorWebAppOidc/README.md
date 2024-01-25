# Blazor Web App with OpenID Connect (OIDC)

This sample features:

- A Blazor Web App with global Auto interactivity.
  - This adds a `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services to the
    server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
- OIDC authentication with Microsoft Entra without using Entra-specific packages.
  - The goal is that this sample can be used as a starting point for any OIDC authentication flow.
- Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.

## Configure the sample

Configure the OIDC provider using the comments in the Program.cs file.

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppOidc` solution file in Visual Studio.
1. Select the `BlazorWebAppOidc` project in **Solution Explorer** and start the app with either Visual Studio's Run button or by selecting **Start Debugging** from the **Debug** menu.

### .NET CLI

In a command shell, navigate to the `BlazorWebAppOidc` project folder and use the `dotnet run` command to run the sample.
