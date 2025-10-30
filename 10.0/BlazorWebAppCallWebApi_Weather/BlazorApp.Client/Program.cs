using BlazorApp.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Preconfigure an HttpClient for web API calls
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the client-side weather service
builder.Services.AddSingleton<IWeatherService, ClientWeatherService>();

await builder.Build().RunAsync();
