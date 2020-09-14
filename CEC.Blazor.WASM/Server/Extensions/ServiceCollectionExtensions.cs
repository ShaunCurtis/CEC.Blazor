using CEC.Blazor.Extensions;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.WASM.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
            // Factory for building the DBContext 
            var dbContext = configuration.GetValue<string>("Configuration:DBContext");
            services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
            return services;
        }
    }
}
