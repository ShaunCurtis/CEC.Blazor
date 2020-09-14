﻿using CEC.Blazor.Components.Base;
using CEC.Blazor.Extensions;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherViewer : RecordComponentBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherForecastControllerService ControllerService { get; set; }

        public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

        protected async override Task OnInitializedAsync()
        {
            this.Service = this.ControllerService;
            await base.OnInitializedAsync();
        }

        protected void NextRecord(int increment) 
        {
            var rec = (this._ID + increment) == 0 ? 1 : this._ID + increment;
            rec = rec > this.Service.BaseRecordCount ? this.Service.BaseRecordCount : rec;
            this.NavManager.NavigateTo($"/WeatherForecast/View?id={rec}");
        }
    }
}
