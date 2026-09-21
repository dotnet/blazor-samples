using BlazorSample.Client.Starship;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// The .Client project registers an HttpClient for HTTP POST requests to the Minimal API.
builder.Services.AddHttpClient<IFormValidation, ClientFormValidation>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

await builder.Build().RunAsync();
