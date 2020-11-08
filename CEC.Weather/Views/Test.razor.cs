using CEC.Blazor.Components;
using CEC.Blazor.Components.Base;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Weather.Views
{
    public partial class Test : ComponentBase, IView
    {
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await Task.Delay(1000);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

        private void RouteTo()
        {
            var dict = new Dictionary<string, object>();
            var viewdata = new ViewData(typeof(Counter), dict);
            if (this.ViewManager != null) ViewManager.LoadViewAsync(viewdata);
        }


    }
}
