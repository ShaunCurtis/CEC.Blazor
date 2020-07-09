using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;
using System;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardGridButtonRow : UICardListBase, IRecordNavigation
    {
        public bool ShowButtons => this.UIOptions?.ShowButtons ?? true;

        public bool ShowAdd => this.UIOptions?.ShowAdd ?? true;

        [Parameter]
        public bool IsPagination { get; set; } = true;

        [Parameter]
        public RenderFragment Paging { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

        [CascadingParameter]
        public RecordConfigurationData Record_Configuration { get; set; } = new RecordConfigurationData();

        protected override void OnInitialized()
        {
            if (this.Record_Configuration != null) this.RecordConfiguration = this.Record_Configuration;
            base.OnInitialized();
        }

        private void Navigate(PageExitType exitType) {
            switch (exitType)
            {
                case PageExitType.ExitToEditor:
                case PageExitType.ExitToNew:
                    if (this.OnEdit != null) this.OnEdit.Invoke(0);
                    else ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(exitType, 0, this.RecordConfiguration.RecordName));
                    break;
                default:
                    ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(exitType, 0, this.RecordConfiguration.RecordName));
                    break;
            }
        }
    }
}
