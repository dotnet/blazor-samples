using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAssemblyXrefGenerator;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("APIClient", client => client.BaseAddress = new Uri("https://learn.microsoft.com"));

builder.Services.AddHttpClient("APIBrowserPageClient", client => client.BaseAddress = new Uri("https://learn.microsoft.com/en-us"));

await builder.Build().RunAsync();
