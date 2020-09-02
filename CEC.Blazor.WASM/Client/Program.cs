using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CEC.Weather.Services;
using CEC.Routing;
using CEC.Blazor.WASM.Client.Extensions;

namespace CEC.Blazor.WASM.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddApplicationServices();
            builder.Services.AddCECBlazor();
            builder.Services.AddCECRouting();

            await builder.Build().RunAsync();
        }
    }
}
