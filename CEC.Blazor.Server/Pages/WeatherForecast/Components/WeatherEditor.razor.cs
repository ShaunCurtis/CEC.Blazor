using CEC.Blazor.Components.Base;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Server.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class WeatherEditor : EditRecordComponentBase<WeatherForecast>
    {
        [Inject]
        public WeatherForecastControllerService ControllerService { get; set; }

        [CascadingParameter(Name = "WeatherForecastID")]
        public int WeatherForecastID { get; set; }

        private string CardCSS => this.IsModal ? "m-0" : "";

        protected async override Task OnInitializedAsync()
        {
            // Assign the correct controller service
            this.Service = this.ControllerService;
            // Try to get the ID from either the cascaded value or a Modal passed in value
            if (this.WeatherForecastID > 0) this.ID = this.WeatherForecastID;
            if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object id)) this.ID = (int)id > 0 ? (int)id : this.ID;
            await base.OnInitializedAsync();
        }
    }
}
