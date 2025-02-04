# `BlazorWebAppAreaOfStaticSsrComponents`

This sample app demonstrates how to maintain an area (folder) of components that adopt static server-side rendering (static SSR) in an app that otherwise adopts a global interactive render mode (Server, WebAssembly, or Auto).

The app targets .NET 9 and uses Blazor features available in ASP.NET Core 9.0 or later, but the technique demonstrated is most useful in 8.0. In ASP.NET Core 9.0 or later, Blazor includes a feature to simplify the implementation of this scenario with the `@attribute [ExcludeFromInteractiveRouting]` Razor component directive. For more information on how to use the `[ExcludeFromInteractiveRouting]` attribute, see [Static SSR pages in a interactive app (ASP.NET Core 9.0 or later)](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes#static-ssr-pages-in-an-interactive-app).

For more information on the technique used by this sample app, see [Static SSR pages in an interactive app: Area (folder) of static SSR components (ASP.NET Core 8.0)](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes?view=aspnetcore-8.0#area-folder-of-static-ssr-components).
