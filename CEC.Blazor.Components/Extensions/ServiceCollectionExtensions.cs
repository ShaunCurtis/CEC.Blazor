﻿using CEC.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCECBlazor(this IServiceCollection services)
        {
            services.AddScoped<BrowserService>();
            return services;
        }
    }
}
