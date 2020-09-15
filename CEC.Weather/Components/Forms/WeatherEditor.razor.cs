using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherEditor : EditRecordComponentBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        private string CardCSS => this.IsModal ? "m-0" : "";

        protected async override Task OnInitializedAsync()
        {
            // Assign the correct controller service
            this.Service = this.ControllerService;
            await base.OnInitializedAsync();
        }
    }
}
