# .NET on Web Workers

This sample accompanies [.NET on Web Workers](https://learn.microsoft.com/aspnet/core/client-side/dotnet-on-webworkers) in the ASP.NET Core Blazor documentation.

## Setup

Confirm that the `wasm-tools` workload is installed:

```bash
cd dotnet
dotnet workload restore
```

### Linux

Long version, from root:

``` bash
# builds the dotnet project, creating build artifacts in dist/dotnet
dotnet publish -c Debug ./dotnet/QRGenerator.csproj
cd react
# installs node packages:
npm install
# cleans the previous dotnet artifacts and copies build output into './public/qr'
npm run integrate
# builds react app
npm run build
# launches
npm run start
```

Short version, from the `react` directory:

``` bash
npm install
npm run build:all:debug
npm run start
```

### Windows

Same as Linux but with `Win` suffix. Examples: `npm run integrateWin`, `buildWin:all:debug`

## Communication

The React app runs in the main thread that has access to DOM. It imports functions for launching a .NET runtime on a Web Worker from a WebAssembly (Wasm) app (refer to [`client.js`](react/src/client.js)) and executes them. These functions establish a Web Worker using the [`worker.js`](dotnet/wwwroot/worker.js) file. Web Worker can perform heavy tasks without blocking the UI. However, it doesn't have direct control over the DOM and relies on communication with main thread for changes to the UI. Communication between the Web Worker and the main thread occurs through message passing. The sample app includes a few simple examples of passing information from .NET (`dotnet`) to the React frontend (`react`) and in the opposite direction.

From `react` to `dotnet`: QR code generation request.

From `dotnet` to `react`: Populating a frontend element with data.

+-----------------------------------------------+              +---------------------+
| React App                                     |              | WASM App            |
| (Main Thread)                                 |              | (WebWorker Thread)  |
|+-----------------+          +----------------+|              | +------------------+|
|| QrImage         |          | client.js      ||              | |  C#'s exports    ||
|| Component       |          |                || Message      | |  (QR Generation) ||
||                 | client   |                || Passing      | |                  ||
||+--------------+ | API      | +-------------+|| -----------> | +------------------+|
||| Button       | | Call     | | generate()  |||              |                     |
||| onClick      | | -------> | | function    ||| Message      |   ^   Built-in   |  |
||+--------------+ |          | |             ||| Passing      |   |   interop    V  |
||+--------------+ |          | |             ||| Transferable |                     |
|||              | | update   | |             ||| <----------- | +------------------+|
||| <img src=..> | | <------- | |             |||  IMAGE       | |  JS's imports    ||
|||              | |          | |             |||              | |                  ||
||+--------------+ |          | +-------------+||              | |                  ||
|+-----------------+          +----------------+|              | +------------------+|
+-----------------------------------------------+              +---------------------+
