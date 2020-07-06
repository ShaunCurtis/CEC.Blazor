using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Components;
using System;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListEditButtonColumn : UICardListBase
    {

        [Parameter]
        public bool IsHeader { get; set; }

    }
}
