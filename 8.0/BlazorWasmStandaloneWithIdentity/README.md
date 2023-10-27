# Blazor WebAssembly Standalone

This sample app demonstrates how to use the built-in ASP.NET Core Identity capabilities from a standalone Blazor WebAssembly application.

## Steps to run the sample

1. Clone this repository
1. Run both projects (`Backend` and `BlazorWasmAuth`) and note the domain and port being used. For example, assume `Backend` runs on `https://localhost:5001` and `BlazorWasmAuth` runs on `https://localhost:5003`.
1. Update `appsettings.json` in both projects. Use `BackendUrl` for the `Backend` project and `FrontendUrl` for the `BlazorWasmAuth` project. For example, if `Backend` runs on `https://localhost:5001` and `BlazorWasmAuth` runs on `https://localhost:5003`, then `appsettings.json` in `Backend` should look like this:

    ```json
    {
      "BackendUrl": "https://localhost:5001",
      "FrontEndUrl": "https://localhost:5003"
    }
    ```

    and `appsettings.json` in `BlazorWasmAuth` should look like this:

    ```json
    {
      "BackendUrl": "https://localhost:5001",
      "FrontendUrl": "https://localhost:5003",
      
    }
    ```
3. Right click the solution explorer tree on the solution and then choose `Configure Startup Projects...`. Select `Multiple startup projects` and set the action for both `Backend` and `BlazorWasmAuth` to `Start`. Click `OK`.
1. Run the solution. Both `Backend` and `BlazorWasmAuth` should start.
1. Register a new user in the `BlazorWasmAuth` app.
1. Log in with the new user.                 
1. Navigate between pages
1. Log out
