using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAppRemoteValidation.Client.Validation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// <snippet_HttpClient>
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// </snippet_HttpClient>
builder.Services.AddClientModelValidation();

await builder.Build().RunAsync();
