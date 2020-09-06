using CEC.Blazor.Services;
using CEC.Weather.Data;
using CEC.Weather.Data.Validators;
using CEC.Weather.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.WASM.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            // Scoped service for the WASM Client version of WeatherForecast Data Service 
            services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            // Transient service for the Fluent Validator for the WeatherForecast record
            return services;
        }
    }
}
