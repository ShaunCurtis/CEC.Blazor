using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using System;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGridDataColumn : UIGridColumn
    {
        [CascadingParameter]
        public UICardGridBase CardGrid { get; set; }

        public bool Show { get; set; } = true;

        protected override string Css => this.CardGrid.IsNavigation ? $"{base.Css}{this.OverflowCss}{this.AltRowCss} cursor-hand" : $"{base.Css}{this.OverflowCss}";

        protected string OverflowCss => this.IsMaxColumn ? " grid-col-overflow" : string.Empty;

        protected string AltRowCss => this.CardGrid.AltRow ? "grid-alt-row" : string.Empty;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.Show)
            {
                int i = 0;
                builder.OpenElement(i, "div");
                builder.AddAttribute(i++, "class", this.Css);
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.CardGrid.NavigateToView())));
                if (this.ChildContent != null) builder.AddContent(i++, this.ChildContent);
                builder.CloseElement();
            }
        }

        protected void NavigateToView()
        {
            this.CardGrid.NavigateToView();
        }

    }
}
