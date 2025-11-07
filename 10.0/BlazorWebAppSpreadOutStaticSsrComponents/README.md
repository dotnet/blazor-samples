# `BlazorWebAppSpreadOutStaticSsrComponents`

This sample demonstrates how to adopt static server-side rendering (static SSR) in components spread out around an app that otherwise adopt interactive render modes (Server, WebAssembly, or Auto) on a per-component basis.

**Update the sample's packages to the latest versions.**

The app targets .NET 9 and uses Blazor features available in ASP.NET Core 9.0 or later, but the technique demonstrated is most useful in 8.0. In ASP.NET Core 9.0 or later, Blazor includes a feature to simplify the implementation of this scenario with the `@attribute [ExcludeFromInteractiveRouting]` Razor component directive. For more information on how to use the `[ExcludeFromInteractiveRouting]` attribute, see [Static SSR pages in a interactive app (ASP.NET Core 9.0 or later)](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes#static-ssr-pages-in-an-interactive-app).

For more information, see [Static SSR pages in a interactive app: Static SSR components spread out across the app (ASP.NET Core 8.0)](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes?view=aspnetcore-8.0#static-ssr-components-spread-out-across-the-app).
