using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastControllerService : BaseControlService<WeatherForecast>, IControlService<WeatherForecast>
    {

        /// <summary>
        /// Property exposing Error Trapped Record ID
        /// </summary>
        public override int RecordID => this.Record is null ? 0 : this.Record.WeatherForecastID;

        public WeatherForecastDataService WeatherForecastDataService { get; set; }

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, WeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
        {
            this.Service = weatherForecastDataService;
            this.WeatherForecastDataService = weatherForecastDataService;
            this.RecordConfiguration = weatherForecastDataService.Configuration;
        }

    }
}
