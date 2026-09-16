using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAppRemoteValidation.Client.Validation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddClientModelValidation();

await builder.Build().RunAsync();
