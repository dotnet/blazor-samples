using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorTodo.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add Todo service for components adopting CSR
builder.Services.AddScoped<ITodoService, ClientTodoService>();

// set base address for default host
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.Configuration["FrontendUrl"] ?? "https://localhost:5002") });

await builder.Build().RunAsync();
