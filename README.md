---
description: "Samples to accompany the official Microsoft Blazor documentation."
page_type: sample
languages:
- razor
- csharp
- cshtml
name: "Blazor sample applications"
products:
- aspnet-core
- blazor
urlFragment: "/"
---
# Samples to accompany the official Microsoft Blazor documentation

Samples in this repository accompany the [official Microsoft Blazor documentation](https://docs.microsoft.com/aspnet/core/blazor/).

To obtain a local copy of the sample apps in this repository, use ***either*** of the following approaches:

* [Fork this repository](https://docs.github.com/get-started/quickstart/fork-a-repo) and [clone it](https://docs.github.com/repositories/creating-and-managing-repositories/cloning-a-repository) to your local system.
* Select the **Code** button. Select **Download ZIP** to save the repository locally. Extract the saved Zip archive (`.zip`) to access the sample apps.

To obtain a single sample app and only its last commit, use [`git sparse-checkout`](https://git-scm.com/docs/git-sparse-checkout).

In the following example, the `git sparse-checkout set` command specifies the path to the sample folder:

* Replace the `{VERSION FOLDER}` placeholder with the version folder.
* Replace the `{SAMPLE FOLDER}` placeholder with the sample folder.

In a command shell, navigate to the folder where you would like to clone the sample. Execute the following commands in the command shell passing the version/sample folder path to the `git sparse-checkout set` command:

```cli
git clone --depth 1 --filter=blob:none https://github.com/dotnet/blazor-samples.git --sparse
cd blazor-samples
git sparse-checkout init --cone
git sparse-checkout set {VERSION FOLDER}/{SAMPLE FOLDER}
```

The following [PowerShell](https://learn.microsoft.com/powershell/) example obtains the 9.0 Blazor Web App sample and places it in the user's documents folder using PowerShell's `~/documents` path for the change directory (`cd`) command:

```powershell
cd "~/documents"
git clone --depth 1 --filter=blob:none https://github.com/dotnet/blazor-samples.git --sparse
cd blazor-samples
git sparse-checkout init --cone
git sparse-checkout set 9.0/BlazorSample_BlazorWebApp
```

## Sample app article links (latest release)

* General Blazor samples referenced by the full Blazor documentation set (`BlazorSample_BlazorWebApp`/`BlazorSample_BlazorWebAssembly`)

* .NET MAUI Blazor Hybrid app with a Blazor Web App and a shared UI RCL (`MauiBlazorWeb`)<br>[Build a .NET MAUI Blazor Hybrid app with a Blazor Web App](https://learn.microsoft.com/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app)

* .NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity (`MauiBlazorWebIdentity`)<br>[.NET MAUI Blazor Hybrid and Web App with ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/hybrid/security/maui-blazor-web-identity)

* Blazor with SignalR (`BlazorSignalRApp`)<br>[Use ASP.NET Core SignalR with Blazor](https://learn.microsoft.com/aspnet/core/blazor/tutorials/signalr-blazor)

* Blazor Web App movie database app tutorial (`BlazorWebAppMovies`)<br>[Build a Blazor movie database app](https://learn.microsoft.com/aspnet/core/blazor/tutorials/movie-database-app)

* Blazor Web App with Microsoft Entra ID (`BlazorWebAppEntra`/`BlazorWebAppEntraBff`)<br>[Secure an ASP.NET Core Blazor Web App with Microsoft Entra ID](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra)

* Blazor Web App with OIDC and Aspire (`BlazorWebAppOidcBff`/`BlazorWebAppOidc`)<br>[Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc)

* Blazor Web App with Windows Authentication<br>[Secure an ASP.NET Core Blazor Web App with Windows Authentication](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-windows-authentication)

* Call a web API (`BlazorWebAppCallWebApi`/`BlazorWebAssemblyCallWebApi`/`BlazorWebAppCallWebApi_Weather`)<br>[Call a web API from ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/call-web-api)

* Blazor WebAssembly scopes-enabled logging (`BlazorWebAssemblyScopesLogger`)<br>[ASP.NET Core Blazor logging: Client-side log scopes](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/logging#client-side-log-scopes)

* Blazor WebAssembly with ASP.NET Core Identity (`BlazorWebAssemblyStandaloneWithIdentity`)<br>[Blazor WebAssembly with ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity)

* Blazor WebAssembly with app roles, built-in Azure Administrator roles, and Azure security groups (`BlazorWebAssemblyEntraGroupsAndRoles`)<br>[Microsoft Entra (ME-ID) groups, Administrator Roles, and App Roles](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/microsoft-entra-id-groups-and-roles)

* WebAssembly Browser App import-export interop (`WASMBroswerAppImportExportInterop`)<br>[JavaScript [JSImport]/[JSExport] interop with a WebAssembly Browser App project](https://learn.microsoft.com/aspnet/core/client-side/dotnet-interop/wasm-browser-app)

* Blazor WebAssembly Xref Generator (`BlazorWebAssemblyXrefGenerator`)<br>[Host and deploy ASP.NET Core Blazor WebAssembly with GitHub Pages](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly/github-pages)

* .NET on Web Workers (`DotNetOnWebWorkers`)<br>[.NET on Web Workers](https://learn.microsoft.com/aspnet/core/client-side/dotnet-on-webworkers)

* Blazor WebAssembly + React (`BlazorWebAssemblyReact`)<br>[Host and deploy: JavaScript bundler support](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/#javascript-bundler-support)

* .NET WebAssembly + React (`DotNetWebAssemblyReact`)<br>[Host and deploy: JavaScript bundler support](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/#javascript-bundler-support)

## Snippet sample apps for article code examples

**WARNING**: Always follow an article's security guidance when implementing sample code.

Prior to the release of .NET 8, snippet sample apps for Blazor Server and Blazor WebAssembly provide the code examples that appear in Blazor articles. Many of the components in the snippet sample apps compile and run if copied to a local test app. However, the entire snippet sample apps aren't meant to be runnable demonstration sample apps, and not all of the components are completely functional. The purpose of the snippet sample apps prior to .NET 8's release is merely to supply code examples to documentation.

For the release of .NET 8 (and later releases), the sample apps for Blazor Web App and Blazor WebAssembly both supply snippets to articles and are fully working demonstration sample apps.

Blazor snippet sample apps

* Blazor Web App (.NET 8 or later)
* Blazor Server (.NET 7.0 or earlier)
* Blazor WebAssembly

## Community help and support

For more information, see the *Support requests* section in the [Blazor *Fundamentals* overview article](https://docs.microsoft.com/aspnet/core/blazor/fundamentals/#support-requests).
