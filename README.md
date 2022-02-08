---
page_type: sample
languages:
  - csharp
  - razor
products:
  - aspnet-core
  - blazor
name: Blazor sample applications
urlFragment: blazor-samples
description: "Samples to accompany the official Microsoft Blazor documentation."
---
# Samples to accompany the official Microsoft Blazor documentation

Samples in this repository accompany the [official Microsoft Blazor documentation](https://docs.microsoft.com/aspnet/core/blazor/).

To obtain a local copy of the sample apps in this repository, use ***either*** of the following approaches:

* [Fork this repository](https://docs.github.com/get-started/quickstart/fork-a-repo) and [clone it](https://docs.github.com/repositories/creating-and-managing-repositories/cloning-a-repository) to your local system.
* Select the **Code** button. Select **Download ZIP** to save the repository locally. Extract the saved Zip archive (`.zip`) to access the sample apps.

## Blazor Server with EF Core

Blazor Server EF Core sample app (ASP.NET Core 6.0): [Browse on GitHub](https://github.com/dotnet/blazor-samples/tree/main/6.0/BlazorServerEFCoreSample)

For more information, see [ASP.NET Core Blazor Server with Entity Framework Core (EFCore)](https://docs.microsoft.com/aspnet/core/blazor/blazor-server-ef-core).

## Blazor with SignalR

Blazor SignalR sample app (ASP.NET Core 6.0):

* [Blazor Server sample: Browse on GitHub](https://github.com/dotnet/blazor-samples/tree/main/6.0/BlazorServerSignalRApp)
* [Blazor WebAssembly sample: Browse on GitHub](https://github.com/dotnet/blazor-samples/tree/main/6.0/BlazorWebAssemblySignalRApp)

For more information, see [Use ASP.NET Core SignalR with Blazor](https://docs.microsoft.com/aspnet/core/tutorials/signalr-blazor).

## Snippet sample apps for article code examples

**WARNING**: Always follow an article's security guidance when implementing sample code.

Snippet sample apps for Blazor Server and Blazor WebAssembly provide the code examples that appear in Blazor articles. Snippet sample apps compile and run. However, several of the examples aren't fully working in their current form because either of the following are true for the article's examples:

* The example requires extra Razor, C#, or other code to run correctly that the article's example doesn't require in order to explain Blazor concepts.
* The example requires additional packages to use additional API, sometimes third-party packages, an account (token or key) for an external service, or another app (for example, a separate running web API app to interact with over a network). Usually, the article associated with the example provides additional guidance on how to make the example work in a live test app.

The primary purpose of the snippet sample apps is to supply code examples to documentation, not to illustrate Blazor best practices. The best use of the sample app code is to facilitate copying examples into local test apps for experimentation and further customization for use in production apps. Namespaces, names, and locations of app resources are contrived in order to maintain the code efficiently for articles and make sure that the code compiles:

* Folder names and folder locations throughout the snippet sample apps roughly match the type of example and article subject. They aren't meant to represent the folder names and layout of a real production app.
* C# files (`.cs`) often appear in the root of the app's folder, which isn't normal for typical production apps.
* Some components create mock C# objects instead of using formal, correct code to create the objects. For example, a component that requires a list of `TodoItem` items might include an `@code` block as its first line (`@code{ private List<TodoItem> todos = new(); }`) to create a variable for use in the example ***that the article doesn't show to readers***. To implement these unfinished examples in a production app for users, finish the code and supply an `@code` block with formal, correct code to create the required objects. The purpose of using these mocked C# objects in the snippet sample apps is to make sure that the code compiles correctly for the documentation.
* Some components only show a portion of their Razor markup in an article. This is accomplished by surrounding the code for display with snippet HTML comments (for example, `<!-- <snippet> -->...<!-- </snippet> -->`). These comments can be removed or ignored, as they have no purpose in an ordinary Blazor app outside of the documentation.

Blazor snippet sample apps (ASP.NET Core 6.0):

* [Blazor Server: Browse on GitHub](https://github.com/dotnet/blazor-samples/tree/main/6.0/BlazorSample_Server)
* [Blazor WebAssembly: Browse on GitHub](https://github.com/dotnet/blazor-samples/tree/main/6.0/BlazorSample_WebAssembly)

## Community help and support

For more information, see the *Support requests* section in the [Blazor *Fundamentals* overview article](https://docs.microsoft.com/aspnet/core/blazor/fundamentals/#support-requests).
