using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.RegisterCustomElement<Counter>("blazor-counter");
builder.RootComponents.RegisterCustomElement<DateTimeNow>("blazor-date-time-now");

await builder.Build().RunAsync();
