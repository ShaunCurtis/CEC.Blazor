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

            // Added here as we don't have access to buildler in AddApplicationServices
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            // the Services for the CEC.Blazor Library
            builder.Services.AddCECBlazor();
            // the Services for the CEC.Routing Library
            builder.Services.AddCECRouting();
            // the local application Services defined in ServiceCollectionExtensions.cs
            builder.Services.AddApplicationServices();

            await builder.Build().RunAsync();
        }
    }
}
