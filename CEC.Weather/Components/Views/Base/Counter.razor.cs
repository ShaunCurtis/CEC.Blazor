using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Core;

namespace CEC.Weather.Components.Views
{
    public partial class Counter : Component, IView
    {
        public class CounterInfo
        {
            public int Counter { get; set; } = 0;
        }

        [CascadingParameter]
        public ViewManager ViewManager { get; set; }

        public CounterInfo counterInfo { get; set; } = new CounterInfo();
        private int objectcount => counterInfo.Counter;

        private int currentCount = 1;

        private int ParameterSetRequests = 0;

        private int currentRenders = 1;

        private string renderType = "None";

        private void IncrementCount()
        {
            currentCount++;
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
