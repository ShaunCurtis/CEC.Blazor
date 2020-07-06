using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.Components.UIControls
{
    public partial class UICardListHeaderRow<TRecord> : UICardListBase
    {
        [CascadingParameter]
        public PagingData<TRecord> Paging { get; set; }

        [Parameter]
        public string FieldName { get; set; }

        [Parameter]
        public string DisplayName { get; set; }

        private string FieldDisplayName { get => string.IsNullOrEmpty(this.DisplayName) ? FieldName : DisplayName; }

        protected override string _RowCss { get => this.MaxRow ? "column-max column-sort" : "column-normal column-sort"; }

        private bool Sorted { get => !string.IsNullOrEmpty(this.FieldName); }

    }
}
