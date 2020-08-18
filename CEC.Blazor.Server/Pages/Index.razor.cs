using CEC.Blazor.Server.Services;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class Index : ComponentBase
    {

        protected async override Task OnInitializedAsync()
        {
            await Task.Delay(10000);
            var x = true;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

    }
}
