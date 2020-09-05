using CEC.Weather.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.WASM.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
            return services;
        }
    }
}
