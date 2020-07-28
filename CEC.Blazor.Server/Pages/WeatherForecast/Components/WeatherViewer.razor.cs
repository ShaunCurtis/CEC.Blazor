using CEC.Blazor.Components.Base;
using CEC.Blazor.Extensions;
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
        public int WeatherForecastID { get; set; }

        public override string PageTitle =>  this.Service?.Record?.Date.AsShortDate() ?? string.Empty;

        private string CardCSS => this.IsModal ? "m-0" : "";

        protected async override Task OnInitializedAsync()
        {
            this.Service = this.ControllerService;
            if (this.WeatherForecastID > 0) this.ID = this.WeatherForecastID;
            if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object id)) this.ID = (int)id > 0 ? (int)id : this.ID;
            await base.OnInitializedAsync();
        }
    }
}
