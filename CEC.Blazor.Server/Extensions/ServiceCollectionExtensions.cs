using CEC.Blazor.Extensions;
using CEC.Weather.Data;
using CEC.Weather.Services;
using CEC.Weather.Data.Validators;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Singleton service for the Server Side version of WeatherForecast Data Service 
            services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            // Transient service for the Fluent Validator for the WeatherForecast record
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            // Factory for building the DBContext 
            var dbContext = configuration.GetValue<string>("Configuration:DBContext");
            services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
            return services;
        }

    }
}
