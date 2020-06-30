using CEC.Blazor.Components.Modal;
using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    public partial class YesNo : ComponentBase
    {
        [CascadingParameter]
        public BootstrapModal Parent { get; set; }

        [Parameter]
        public string Message { get; set; } = "Are You Sure?";

        public void Close(bool state)
        {
            if (state) this.Parent.Close(BootstrapModalResult.Exit());
            else this.Parent.Close(BootstrapModalResult.Cancel());
        }

    }
}
