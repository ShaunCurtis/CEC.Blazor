using CEC.Blazor.Components;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WeatherForecastControllerService : BaseControllerService<DbWeatherForecast>, IControllerService<DbWeatherForecast>
    {

        /// <summary>
        /// Property exposing Null Trapped Record ID
        /// </summary>
        public override int RecordID => this.Record is null ? -1 : this.Record.WeatherForecastID;

        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        /// <summary>
        /// WeatherForecast data service
        /// </summary>
        public WeatherForecastDataService WeatherForecastDataService { get; set; }

        public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, WeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
        {
            this.Service = weatherForecastDataService;
            this.WeatherForecastDataService = weatherForecastDataService;
        }

        /// <summary>
        /// Overrides the base method to load the sorted version of the list getter
        /// and set the default sort column
        /// </summary>
        /// <param name="withDelegate"></param>
        public async override Task LoadPagingAsync(bool withDelegate = true)
        {
            await base.LoadPagingAsync(false);
            this.PageLoaderAsync = new IControllerPagingService<DbWeatherForecast>.PageLoaderDelegateAsync(this.GetDataPageWithSortingAsync);
            this.DefaultSortColumn = "WeatherForecastID";
            await this.LoadAsync();
        }

        /// <summary>
        /// Method to get a sorted Data Page for the List Page
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async override Task<List<DbWeatherForecast>> GetDataPageWithSortingAsync()
        {
            await this.GetFilteredListAsync();
            if (this.PageStartRecord > this.Records.Count) this.CurrentPage = 1;
            if (string.IsNullOrEmpty(this.SortColumn)) return this.Records.Skip(this.PageStartRecord).Take(this._PageSize).ToList();
            else
            {
                if (this.SortingDirection == SortDirection.Ascending)
                {
                    return this.Records.OrderBy(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
                }
                else
                {
                    return this.Records.OrderByDescending(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
                }
            }
        }
    }
}
