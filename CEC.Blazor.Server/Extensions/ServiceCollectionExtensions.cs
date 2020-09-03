using CEC.Weather.Data;
using CEC.Weather.Data.Validators;
using CEC.Weather.Services;
using CEC.Blazor.Server.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<WeatherForecastDummyDataService>();
            services.AddScoped<WeatherForecastServerDataService>();
            services.AddScoped<WeatherForecastControllerService>();
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            services.AddScoped<CosmicDirectoryService>();
            services.AddSingleton<SalaryDataService>();
            services.AddTransient<SalaryControllerService>();
            return services;
        }

    }
}
