using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

            // Added here as we don't have access to builder in AddApplicationServices
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
