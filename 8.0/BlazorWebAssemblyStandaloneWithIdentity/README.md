# Blazor WebAssembly Standalone with ASP.NET Core Identity sample app

This sample app demonstrates how to use the built-in ASP.NET Core Identity capabilities from a standalone Blazor WebAssembly app.

## Steps to run the sample

1. Clone this repository.

1. The default URLs for the two apps are:

   * `Backend` app (`BackendUrl`): `https://localhost:7211`
   * `BlazorWasmAuth` app (`FrontendUrl`): `https://localhost:7171`
   
   If needed, update `appsettings.json` in both projects with a new `BackendUrl` for the `Backend` project and a new `FrontendUrl` for the `BlazorWasmAuth` project.

   If `Backend` should run at `https://localhost:5001` and `BlazorWasmAuth` should run at `https://localhost:5002`, make the following changes.

   Uupdate the `appsettings.json` file in `Backend` app to the following:

    ```json
    {
      "BackendUrl": "https://localhost:5001",
      "FrontEndUrl": "https://localhost:5002"
    }
    ```

    Update the `wwwroot/appsettings.json` file in `BlazorWasmAuth` app to the following:

    ```json
    {
      "BackendUrl": "https://localhost:5001",
      "FrontendUrl": "https://localhost:5002"
    }
    ```

1. Run the `Backend` and `BlazorWasmAuth` apps.

1. Navigate to the `BlazorWasmAuth` app.

1. Register a new user in the `BlazorWasmAuth` app using the **Register** link in the upper-right corner of the app's UI.

1. Log in with the new user.

1. Navigate to the private page that only authenticated users can reach. The link appears in the navigation sidebar after the user is authenticated.

1. Log out of the app.
