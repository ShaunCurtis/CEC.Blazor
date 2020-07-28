using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// Base UI Rendering Wrapper to build a Botstrap Component
    /// </summary>

    public class UITag : UIBase
    {

        [Parameter]
        public virtual string Tag { get; set; } = "div";

        protected override string _Tag => this.Tag;

    }
}
