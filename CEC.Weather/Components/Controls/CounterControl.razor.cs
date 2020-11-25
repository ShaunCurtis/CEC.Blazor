using CEC.Blazor.Components;
using CEC.Blazor.Components;
using CEC.Weather.Components.Views;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class CounterControl : Component
    {
        public class CounterInfo
        {
            public int Counter { get; set; } = 0;
        }

        [Parameter] public int CurrentCount { get; set; } = 0;

        [CascadingParameter]
        public ViewManager ViewManager { get; set; }

        [Parameter] public Counter.CounterInfo counterInfo { get; set; } = new Counter.CounterInfo();

        private int objectcount => counterInfo.Counter;
      
        private int ParameterSetRequests = 0;

        private int currentRenders = 1;

        private string renderType = "None";

        private void IncrementCount()
        {
            CurrentCount++;
        }
        private void IncrementObjectCount()
        {
            counterInfo.Counter++;
        }

        private void Reload()
        {
            this.ViewManager.LoadViewAsync(this.ViewManager.ViewData);
        }

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
