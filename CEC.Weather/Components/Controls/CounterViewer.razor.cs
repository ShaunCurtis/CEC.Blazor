using CEC.Blazor.SPA.Components;
using CEC.Weather.Components.Views;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Core;

namespace CEC.Weather.Components
{
    public partial class CounterViewer : Component
    {
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }

        [Parameter] public int CurrentCount { get; set; } = 0;

        [Parameter] public Counter.CounterInfo CounterInfo { get; set; } = new Counter.CounterInfo();

        private int ParameterSetRequests = 0;

        private int currentRenders = 1;

        private string renderType = "None";


        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender) renderType = "First Render";
            else renderType = "Subsequent Render";
            ParameterSetRequests++;
            return base.OnRenderAsync(firstRender);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            currentRenders++;
            return base.OnAfterRenderAsync(firstRender);
        }

    }
}
