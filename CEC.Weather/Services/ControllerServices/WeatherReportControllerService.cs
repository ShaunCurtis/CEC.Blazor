using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Weather.Services
{
    public class WeatherReportControllerService : BaseControllerService<DbWeatherReport, WeatherForecastDbContext>, IControllerService<DbWeatherReport, WeatherForecastDbContext>
    {

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> StationLookupList { get; set; }

        public WeatherReportControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IWeatherReportDataService DataService) : base(appconfiguration, navmanager)
        {
            this.Service = DataService;
            this.DefaultSortColumn = "ID";
        }

        public async Task LoadLookups()
        {
            this.StationLookupList = await this.GetLookUpListAsync<DbWeatherStation>();
        }
    }
}
