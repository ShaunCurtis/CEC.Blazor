using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UIAlert
    {
        [Parameter]
        public Alert Alert { get; set; } = new Alert();

        [Parameter]
        public bool Boxing { get; set; } = true;

        [Parameter]
        public bool Small { get; set; }

        protected bool IsAlert => this.Alert != null && this.Alert.IsAlert;

        protected string Css => this.Small ? "alert alert-sm" : "alert";

    }
}
