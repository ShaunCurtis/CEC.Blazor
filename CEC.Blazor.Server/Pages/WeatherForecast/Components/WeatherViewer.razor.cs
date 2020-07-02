using CEC.Blazor.Components.Base;
using CEC.Blazor.Server.Data;
using CEC.Blazor.Server.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages.Components
{
    public partial class WeatherViewer : RecordComponentBase<WeatherForecast>
    {
        [Inject]
        private WeatherForecastControllerService ControllerService { get; set; }

        [CascadingParameter(Name ="WeatherForecastID")]
        public int WeatherStationID { get; set; }

        private string CardCSS => this.IsModal ? "m-0" : "";

        protected async override Task OnInitializedAsync()
        {
            this.Service = this.ControllerService;
            if (this.WeatherStationID > 0) this.ID = this.WeatherStationID;
            if (this.IsModal)
            {
                if (this.Parent.Options.Parameters.TryGetValue("ID", out object id)) this.ID = (int)id > 0 ? (int)id : this.ID; 
            }
            await base.OnInitializedAsync();
        }
    }
}
