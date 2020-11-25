using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components.Views
{
    public partial class Index : Component , IView
    {
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }


        protected async override Task OnRenderAsync(bool firstRender)
        {
            await Task.Delay(1000);
            await base.OnRenderAsync(firstRender);
        }

    }
}
