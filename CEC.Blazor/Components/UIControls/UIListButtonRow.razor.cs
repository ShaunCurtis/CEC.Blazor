using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UIListButtonRow : ApplicationComponentBase
    {

        [CascadingParameter]
        public UIOptions UIOptions { get; set; } 

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

        [Parameter]
        public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

        protected override void OnInitialized()
        {
            if (this.Record_Configuration != null) this.RecordConfiguration = this.Record_Configuration;
            base.OnInitialized();
        }

        private void Navigate(PageExitType exitType) => this.NavigateTo(new EditorEventArgs(exitType, 0, this.RecordConfiguration.RecordName));

    }
}
