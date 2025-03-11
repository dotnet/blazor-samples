# .NET MAUI Blazor Hybrid with a Blazor Web App and ASP.NET Core Identity sample app (`MauiBlazorWeb`)

This sample shows .NET MAUI Blazor Hybrid with a Blazor Web App that shares common UI and *authentication*. The sample uses ASP.NET Core Identity local accounts, but you can use this pattern for any authentication provider from a MAUI Blazor Hybrid client.

The sample demonstrates the following:	

* Setting up the UI to show/hide pages if a user is/is not authenticated.
* Setting up the ASP.NET Identity endpoints so they can be called by remote clients.
* Logging in, Logging out, and refreshing tokens from the MAUI client.
* Saving and retrieving tokens in secure device storage.
* Calling a secure endpoint (`/api/weather`) from the client.

For more information, see [.NET MAUI Blazor Hybrid with a Blazor Web App and ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app-identity).

## Steps to run the sample

1. Clone this repository or download a ZIP archive of the repository. For more information, see [How to download a sample](https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core#how-to-download-a-sample).
1. Make sure you have [.NET 9 and the MAUI workload installed](https://learn.microsoft.com/dotnet/maui/get-started/installation).
1. Open the solution in Visual Studio 2022 or VS Code with the .NET MAUI extension installed.
1. Set the `MauiBlazorWeb` MAUI project as the startup project. In Visual Studio, right-click the project and select **Set as Startup Project**.
1. Start the `MauiBlazorWeb.Web` project without debugging. In Visual Studio, right-click on the project and select **Debug** > **Start without Debugging**.
1. Inspect the Identity endpoints by navigating to `https://localhost:7157/swagger` in a browser.
1. Navigate to `https://localhost:7157/identity/register` to register a user in the Blazor Web App. Immediately after the user is registered, use the **Click here to confirm your account** link in the UI to confirm the user's email address because a real email sender isn't registered for account confirmation.
1. Start (`F5`) the `MauiBlazorWeb` MAUI project. You can set the debug target to either **Windows** or an Android emulator.
1. Notice you can only see the `Home` and `Login` pages.
1. Log in with the user that you registered.
1. Notice you can now see the shared `Counter` and `Weather` pages.
1. Log out and notice you can only see the `Home` and `Login` pages again.
1. Navigate to `https://localhost:7157/` and the web app behaves the same.
