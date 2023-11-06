using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWasmAuth.Components;
using BlazorWasmAuth.Identity;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register settings
builder.Configuration.AddJsonFile("appsettings.json");

// register the cookie handler
builder.Services.AddScoped<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

// set base address for default host
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.Configuration["FrontendUrl"]!) });

// configure client for auth interactions
builder.Services.AddHttpClient(
    "Auth",
    opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"]!))
    .AddHttpMessageHandler<CookieHandler>();

await builder.Build().RunAsync();
