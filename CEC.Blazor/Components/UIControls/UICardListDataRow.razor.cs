using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using System;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListDataRow : ApplicationComponentBase
    {

        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        [CascadingParameter(Name = "ID")]
        public int ID { get; set; } = 0;

        [CascadingParameter]
        public UIOptions UIOptions { get; set; } = new UIOptions();

        [CascadingParameter(Name = "OnView")]
        protected Action<int> OnView { get; set; }

        [Parameter]
        public bool MaxRow { get; set; }

        [Parameter]
        public bool Show { get; set; } = true;

        [Parameter]
        public bool IsNavigation { get; set; } = true;

        [Parameter]
        public string MoreCss { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private bool DoNavigation => this.ID > 0 && IsNavigation;

        protected override void OnInitialized()
        {
            this.IsNavigation = UIOptions?.ListNavigationToViewer ?? this.IsNavigation;
            base.OnInitialized();
        }

        private string _RowCss => this.MaxRow ? "column-max" : "column-normal";

        private string RowCss => string.IsNullOrEmpty(this.MoreCss) ? _RowCss : string.Concat(_RowCss, " ", this.MoreCss);

        private void Navigate()
        {
            if (this.OnView is null) this.NavigateTo(new EditorEventArgs(PageExitType.ExitToView, this.ID, this.RecordConfiguration.RecordName));
            else this.OnView.Invoke(this.ID);
        }
    }
}
