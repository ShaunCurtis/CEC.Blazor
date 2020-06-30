using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.Base;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.Server.Components.UIControls
{
    public partial class UICardListDataRow: ApplicationComponentBase
    {

        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        [CascadingParameter(Name ="ID")]
        public long ID { get; set; } = 0;

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

        private bool DoNavigation { get => this.ID > 0 && IsNavigation; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        private string _RowCss { get => this.MaxRow ? "column-max" : "column-normal"; }

        private string RowCss { get => string.IsNullOrEmpty(this.MoreCss) ? _RowCss : string.Concat(_RowCss, " " ,this.MoreCss); }

        private void Navigate() => this.NavigateTo(new EditorEventArgs(PageExitType.ExitToView, this.ID, this.RecordConfiguration.RecordName));

    }
}
