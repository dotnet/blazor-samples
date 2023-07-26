using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<NotifierService>();

            #region snippet1
            var vehicleData = new Dictionary<string, string>()
            {
                { "color", "blue" },
                { "type", "car" },
                { "wheels:count", "3" },
                { "wheels:brand", "Blazin" },
                { "wheels:brand:type", "rally" },
                { "wheels:year", "2008" },
            };

            var memoryConfig = new MemoryConfigurationSource { InitialData = vehicleData };

            builder.Configuration.Add(memoryConfig);
            #endregion

            await builder.Build().RunAsync();
        }
    }
}
