# .NET on WebAssembly in a React component

This sample shows how to use .NET on WebAssembly integrated into a React app. It also extracts the React component into a reusable package.

* [Blazor Community Standup - Integrate .NET in JavaScript apps](https://www.youtube.com/watch?v=tAh899Gri4E)
* [Blazor + React demo (`BlazorWebAssemblyReact`, .NET 10 or later, `dotnet/blazor-samples` GitHub repository](https://github.com/dotnet/blazor-samples)
* [Host and deploy: JavaScript bundler support](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/#javascript-bundler-support)

## Project structure

* `app`: Target React app using rollup to do the JavaScript build.
* `qrlibrary`: npm library implementing QR generation.
  * `dotnet`: .NET implementation of QR generator.
  * `react`: [Rollup](https://rollupjs.org/)-bundled React component for showing a QR code image.

## Building the source code

### .NET

* Install the [.NET SDK](https://dotnet.microsoft.com/download) (.NET 10 or later).
* Run `dotnet publish` in the `qrlibrary/dotnet` folder.

### React library

In the `qrlibrary/react` folder:

* Run `npm install`.
* Run `npm run build`.

### App

In the `app` folder:

* Run `npm install`.
* Run `npm run build`.

## Producing bundler-friendly build output

In JS, file dependencies, such as other JS files or images, are resolved using `import` statements. In browsers, only JS files can be imported using `import` statements. In .NET 10 or later, the .NET SDK can produce build output for JavaScript (JS) bundlers with the MSBuild property `WasmBundlerFriendlyBootConfig` set to `true`.

Only publishing the app (`dotnet publish` or **Publish** in Visual Studio/Visual Studio Code) copies all files to the output directory. Bundler-friendly output isn't generated when building an app to make incremental builds faster. Making it work for build output should be possible by properly mapping imports to individual locations using a bundler's custom plugin. Microsoft doesn't provide any such plugin at the moment.

For more information, see [Host and deploy: JavaScript bundler support](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/#javascript-bundler-support) in the ASP.NET Core Blazor documentation.
