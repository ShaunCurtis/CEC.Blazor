using CEC.Blazor.Components.Base;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Server.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class WeatherForecastEditor : EditRecordComponentBase<WeatherForecast>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        protected async override Task OnInitializedAsync()
        {
            // Assign the correct controller service
            this.Service = this.ControllerService;
            await base.OnInitializedAsync();
        }
    }
}
