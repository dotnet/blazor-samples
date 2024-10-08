# Blazor Web App Call web API (Weather) sample app

This sample updates the `Weather` page in a project created from the Blazor Web App project template to make an API call from the client to the server when the page is prerendered and uses the Auto render mode.

For more information, see [Call a web API from ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/call-web-api).

* The `Weather` component depends on an `IWeatherService` for obtaining the weather data. This insulates the component from determining where it's running.
* The server project has a `WeatherService` that generates the weather data and is exposed as an API endpoint.
* The client project has a `WeatherService` that calls the server API endpoint to get the weather data.
* The `Weather` page is prerendered using static streaming rendering, so the page loads immediately with a "Loading..." placeholder and then is updated with the weather data.
* Next, the `Weather` page renders using the Auto interactive render mode, so it renders initially from the server over a WebSocket while the .NET runtime is downloaded, and then it switches to WebAssembly client-side rendering (CSR) for future visits.
* The `Weather` page persists its prerendered state so that the state can be reused when the page renders interactively.
* Links to the `Weather` page disable enhanced navigation (`data-enhance-nav="false"`), which is currently incompatible with the Persisted Component State service. For more information, see [Prerendered data (Blazor documentation)](https://learn.microsoft.com/aspnet/core/blazor/call-web-api?view=aspnetcore-8.0#prerendered-data).
