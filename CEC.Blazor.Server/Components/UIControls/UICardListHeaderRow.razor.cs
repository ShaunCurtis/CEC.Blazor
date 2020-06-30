using Microsoft.AspNetCore.Components;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.Server.Components.UIControls
{
    public partial class UICardListHeaderRow<TRecord> : Blazor.Components.Base.ApplicationComponentBase
    {
        [CascadingParameter]
        public PagingData<TRecord> Paging { get; set; }

        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        [Parameter]
        public string FieldName { get; set; }

        [Parameter]
        public string DisplayName { get; set; }

        [Parameter]
        public bool Show { get; set; } = true;

        [Parameter]
        public bool MaxRow { get; set; }

        [Parameter]
        public string MoreCss { get; set; }

        private string FieldDisplayName { get => string.IsNullOrEmpty(this.DisplayName) ? FieldName : DisplayName; }

        private string _RowCss { get => this.MaxRow ? "column-max column-sort" : "column-normal column-sort"; }

        private string RowCss { get => string.IsNullOrEmpty(this.MoreCss) ? _RowCss : string.Concat(_RowCss, " ", this.MoreCss); }

        private bool Sorted { get => !string.IsNullOrEmpty(this.FieldName); }

    }
}
