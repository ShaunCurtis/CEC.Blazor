using CEC.Blazor.Server.Data;
using CEC.Blazor.Server.Data.Validators;
using CEC.Blazor.Server.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<WeatherForecastDummyDataService>();
            services.AddSingleton<WeatherForecastDataService>();
            services.AddScoped<WeatherForecastControllerService>();
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            services.AddScoped<CosmicDirectoryService>();
            services.AddSingleton<SalaryDataService>();
            services.AddTransient<SalaryControllerService>();
            return services;
        }

    }
}
