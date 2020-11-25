using CEC.Blazor.Components;
using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class MonthYearIDListFilter : ControlBase
    {
        [Inject]
        private WeatherReportControllerService Service { get; set; }

        [Parameter]
        public bool ShowID { get; set; } = true;

        private SortedDictionary<int, string> MonthLookupList { get; set; }

        private SortedDictionary<int, string> YearLookupList { get; set; }

        private SortedDictionary<int, string> IdLookupList { get; set; }

        private EditContext EditContext => new EditContext(this.Service.Record);

        private int OldMonth = 0;
        private int OldYear = 0;
        private long OldID = 0;

        private int Month
        {
            get => this.Service.FilterList.TryGetFilter("Month", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Month", value);
                else this.Service.FilterList.ClearFilter("Month");
                if (this.Month != this.OldMonth)
                {
                    this.OldMonth = this.Month;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        private int Year
        {
            get => this.Service.FilterList.TryGetFilter("Year", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Year", value);
                else this.Service.FilterList.ClearFilter("Year");
                if (this.Year != this.OldYear)
                {
                    this.OldYear = this.Year;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        private int ID
        {
            get => this.Service.FilterList.TryGetFilter("WeatherStationID", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("WeatherStationID", value);
                else this.Service.FilterList.ClearFilter("WeatherStationID");
                if (this.ID != this.OldID)
                {
                    this.OldID = this.ID;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        protected async override Task OnRenderAsync(bool firstRender)
        {
            this.OldYear = this.Year;
            this.OldMonth = this.Month;
            await GetLookupsAsync();
        }

        protected async Task GetLookupsAsync()
        {
            this.IdLookupList = await this.Service.GetLookUpListAsync<DbWeatherStation>("-- ALL STATIONS --");
            this.MonthLookupList = new SortedDictionary<int, string> { { 0, "-- ALL MONTHS --" } };
            for (int i = 1; i < 13; i++) this.MonthLookupList.Add(i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
            {
                var list = await this.Service.GetDistinctListAsync(new DbDistinctRequest() { FieldName = "Year", QuerySetName = "WeatherReport", DistinctSetName = "DistinctList" });
                this.YearLookupList = new SortedDictionary<int, string> { { 0, "-- ALL YEARS --" } };
                list.ForEach(item => this.YearLookupList.Add(int.Parse(item), item));
            }

        }
    }
}
