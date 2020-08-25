using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Pages
{
    public partial class Index : ComponentBase
    {

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
