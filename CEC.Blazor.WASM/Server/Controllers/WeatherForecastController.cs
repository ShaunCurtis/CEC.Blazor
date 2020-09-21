using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CEC.Weather.Services;
using CEC.Weather.Data;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.WASM.Server.Controllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        protected IWeatherForecastDataService DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastDataService weatherForecastDataService)
        {
            this.DataService = weatherForecastDataService;
            this.logger = logger;
        }

        [MVC.Route("weatherforecast/list")]
        [HttpGet]
        public async Task<List<DbWeatherForecast>> GetList() => await DataService.GetRecordListAsync();

        [MVC.Route("weatherforecast/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherForecast>> GetFilteredRecordListAsync([FromBody]FilterList filterList) => await DataService.GetFilteredRecordListAsync(filterList);

        [MVC.Route("weatherforecast/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync();

        [MVC.Route("weatherforecast/get")]
        [HttpGet]
        public async Task<DbWeatherForecast> GetRec(int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherforecast/read")]
        [HttpPost]
        public async Task<DbWeatherForecast> Read([FromBody]int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherforecast/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherForecast record) => await DataService.UpdateRecordAsync(record);

        [MVC.Route("weatherforecast/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherForecast record) => await DataService.CreateRecordAsync(record);

        [MVC.Route("weatherforecast/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherForecast record) => await DataService.DeleteRecordAsync(record);
    }
}
