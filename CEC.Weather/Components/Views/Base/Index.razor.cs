﻿using CEC.Blazor.SPA.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using CEC.Blazor.Core;

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
