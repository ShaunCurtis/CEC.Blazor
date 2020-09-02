using CEC.Blazor.Components.Base;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherEditor : EditRecordComponentBase<DbWeatherForecast>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        private string CardCSS => this.IsModal ? "m-0" : "";

        protected async override Task OnInitializedAsync()
        {
            // Assign the correct controller service
            this.Service = this.ControllerService;
            // Try to get the ID from either the cascaded value or a Modal passed in value
            if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object id)) this.ID = (int)id > -1 ? (int)id : this.ID;
            await base.OnInitializedAsync();
        }
    }
}
