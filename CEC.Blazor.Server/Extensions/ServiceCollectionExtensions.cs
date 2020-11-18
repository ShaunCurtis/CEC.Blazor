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
            // Dummy service produces a new recordset each time the application runs 
            //services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
            services.AddSingleton<IWeatherForecastDataService, WeatherForecastDummyDataService>();
            services.AddSingleton<IWeatherStationDataService, WeatherStationDummyDataService>();
            services.AddSingleton<IWeatherReportDataService, WeatherReportDummyDataService>();
            // Scoped service for the WeatherForecast Controller Service
            services.AddScoped<WeatherForecastControllerService>();
            services.AddScoped<WeatherStationControllerService>();
            services.AddScoped<WeatherReportControllerService>();
            // Transient service for the Fluent Validator for the WeatherForecast record
            services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
            services.AddTransient<IValidator<DbWeatherStation>, WeatherStationValidator>();
            services.AddTransient<IValidator<DbWeatherReport>, WeatherReportValidator>();
            // Factory for building the DBContext 
            var dbContext = configuration.GetValue<string>("Configuration:DBContext");
            services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
            return services;
        }

    }
}
