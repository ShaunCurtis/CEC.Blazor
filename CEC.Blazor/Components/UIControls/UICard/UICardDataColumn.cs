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
    public partial class UICardDataColumn : UITColumn<int>
    {
        public bool Show { get; set; } = true;

        protected override string Css => this.Card.IsNavigation ? $"{base.Css}{this.OverflowCss} cursor-hand" : $"{base.Css}{this.OverflowCss}";

        protected bool IsMaxColumn => (this.Card != null && this.Card.MaxColumn == this.Column );

        protected string OverflowCss => this.IsMaxColumn ? " td-max td-overflow" : " td-normal";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.Tag = "td";
            if (this.Show)
            {
                int i = 0;
                builder.OpenElement(i, this.Tag);
                builder.AddAttribute(i++, "class", this.Css);
                if (!string.IsNullOrEmpty(this.ComponentId)) builder.AddAttribute(i++, "id", this.ComponentId);
                builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (e => this.Card.NavigateToView(this.RecordID))));
                if (this.ChildContent != null) builder.AddContent(i++, this.ChildContent);
                builder.CloseElement();
            }
        }
    }
}
