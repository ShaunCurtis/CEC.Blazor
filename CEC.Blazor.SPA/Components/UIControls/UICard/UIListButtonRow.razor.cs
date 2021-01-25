using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public partial class UIListButtonRow : UICardListBase
    {
        public bool ShowButtons => this.UIWrapper?.UIOptions?.ShowButtons ?? true;

        public bool ShowAdd => this.UIWrapper?.UIOptions?.ShowAdd ?? true;

        [Parameter]
        public bool IsPagination { get; set; } = true;

        [Parameter]
        public RenderFragment Paging { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

        // TODO - sort this exit strategy now we don't have a navigator
        private void Navigate(PageExitType exitType) {
            switch (exitType)
            {
                case PageExitType.ExitToEditor:
                case PageExitType.ExitToNew:
                    if (this.UIWrapper.OnEdit != null) this.UIWrapper.OnEdit.Invoke(0);
                    break;
                default:
                    break;
            }
        }
    }
}
