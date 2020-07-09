using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using System;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListDataColumn : UICardListBase
    {

        protected override void OnInitialized()
        {
            this.IsNavigation = UIOptions?.ListNavigationToViewer ?? this.IsNavigation;
            base.OnInitialized();
        }

        //protected override string _RowCss => this.MaxColumn ? "column-max" : "column-normal";
    }
}
