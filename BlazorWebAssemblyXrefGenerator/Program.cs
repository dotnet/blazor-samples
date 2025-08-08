using BlazorWebAssemblyXrefGenerator;
using BlazorWebAssemblyXrefGenerator.Services;
using BlazorWebAssemblyXrefGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddHttpClient("APIClient", client => client.BaseAddress = new Uri("https://learn.microsoft.com"));

await builder.Build().RunAsync();
