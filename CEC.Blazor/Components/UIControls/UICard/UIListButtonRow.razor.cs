using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UIListButtonRow : UICardListBase, IRecordNavigation
    {
        public bool ShowButtons => this.UIWrapper?.UIOptions?.ShowButtons ?? true;

        public bool ShowAdd => this.UIWrapper?.UIOptions?.ShowAdd ?? true;

        [Parameter]
        public bool IsPagination { get; set; } = true;

        [Parameter]
        public RenderFragment Paging { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

        private void Navigate(PageExitType exitType) {
            switch (exitType)
            {
                case PageExitType.ExitToEditor:
                case PageExitType.ExitToNew:
                    if (this.UIWrapper.OnEdit != null) this.UIWrapper.OnEdit.Invoke(0);
                    else ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(exitType, 0, this.RecordConfiguration.RecordName));
                    break;
                default:
                    ((IRecordNavigation)this).NavigateTo(new EditorEventArgs(exitType, 0, this.RecordConfiguration.RecordName));
                    break;
            }
        }
    }
}
