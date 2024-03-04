using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Todo service for components adopting CSR
builder.Services.AddScoped<IMovieService, ClientMovieService>();

// Set base address for default host
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["FrontendUrl"] ?? "https://localhost:5002") });

await builder.Build().RunAsync();
