using CEC.Blazor.Components;
using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Views
{
    public partial class Index : ComponentBase, IView
    {
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }


        protected async override Task OnInitializedAsync()
        {
            await Task.Delay(10000);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

    }
}
