using CEC.Blazor.Components;
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Core;

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
            if (state) this.Parent.Close(ModalResult.Exit());
            else this.Parent.Close(ModalResult.Cancel());
        }

    }
}
