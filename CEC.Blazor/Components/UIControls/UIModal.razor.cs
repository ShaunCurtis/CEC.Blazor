using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UIModal : ComponentBase
    {

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public RenderFragment Body { get; set; }

        [Parameter]
        public RenderFragment Footer { get; set; }

        [Parameter]
        public EventCallback<bool> Dismiss { get; set; }

        public bool ShowModal { get; set; }

        [Parameter]
        public bool ShowDismiss { get; set; }

        private bool IsDirty { get; set; }

        private string BorderCSS => this.IsDirty ? " border-danger" : string.Empty;

        private string HeaderCSS => this.IsDirty ? " bg-danger text-white" : " bg-light";

        protected override void OnInitialized()
        {
        }

        public void Show()
        {
            this.ShowModal = true;
            this.StateHasChanged();
        }
        
        public void Hide()
        {
            this.ShowModal = false;
            this.StateHasChanged();
        }

        public void SetCleanStatus(bool status)
        {
            IsDirty = !status;
            this.StateHasChanged();
        }

    }
}
