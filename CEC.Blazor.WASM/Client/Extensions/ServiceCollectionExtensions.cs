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
            services.AddScoped<BrowserService>();
            services.AddScoped<WeatherForecastControllerService>();
            services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            return services;
        }

    }
}
