using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Components.Modal;
using CEC.Blazor.Data;
using CEC.Blazor.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CEC.Blazor.Components.BaseForms
{
    public class ListComponentBase<TRecord, TContext> :
        ControllerServiceComponentBase<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
         where TContext : DbContext
    {

        /// <summary>
        /// Paging Interface giving access to the specific service that sorts the dataset into pages and tracks the page being displayed
        /// </summary>
        public IControllerPagingService<TRecord> Paging => this.Service != null ? (IControllerPagingService<TRecord>)this.Service : null;

        /// <summary>
        /// Property referencing the Bootstrap modal instance
        /// </summary>
        protected BootstrapModal _BootstrapModal { get; set; }

        /// <summary>
        /// Boolean used by Filter set to true if you want empty recordset if no filter is set
        /// </summary>
        [Parameter]
        public bool OnlyLoadIfFilter { get; set; } = false;

        /// <summary>
        /// constructed Value for the List Title based on the RecordConfiguration
        /// </summary>
        [Parameter]
        public string ListTitle { get; set; }

        /// <summary>
        /// Should be overridden in inherited components
        /// and called after setting the Service
        /// </summary>
        protected async override Task OnInitializedAsync()
        {
            // Reset the Service i.e. clear the record list, filter etc.
            if (this.IsService)
            {
                await this.Service.Reset();
                this.ListTitle = string.IsNullOrEmpty(this.ListTitle) ? $"List of {this.Service.RecordConfiguration.RecordListDecription}" : this.ListTitle;
            }
            await base.OnInitializedAsync();
        }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            // Load the page - as we've reset everything this will be the first page with the default filter
            if (this.IsService)
            {
                // Load the filters for the recordset
                this.LoadFilter();
                // Load the paged recordset
                await this.Service.LoadPagingAsync();
            }
            this.Loading = false;
        }

        /// <summary>
        /// Method called to load the filter
        /// </summary>
        protected virtual void LoadFilter()
        {
            if (IsService) this.Service.FilterList.OnlyLoadIfFilters = this.OnlyLoadIfFilter;
        }

        /// <summary>
        /// Inherited
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                this.Paging.PageHasChanged += this.UpdateUI;
                this.Service.ListHasChanged += this.OnRecordsUpdate;
                this.Service.FilterHasChanged += this.FilterUpdated;
            }
            base.OnAfterRender(firstRender);
        }

        /// <summary>
        /// Async Event Handler that resets the Paging record count and reloads the Pager
        /// Triggers a page reload when complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void OnRecordsUpdate(object sender, EventArgs e)
        {
            if (this.IsService)
            {
                this.Loading = true;
                this.StateHasChanged();
                await this.Paging.LoadAsync();
            }
            this.Loading = false;
            this.StateHasChanged();
        }

        /// <summary>
        /// Handler for a UpdatePage event - normally linked to the Pager PageHasChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recordno"></param>
        protected void UpdateUI(object sender, int recordno) => InvokeAsync(StateHasChanged);

        /// <summary>
        /// Event Handler for when the List is updated Externally i.e. not by the Pager 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual async void ListUpdated(object sender, EventArgs e)
        {
            await this.Paging.LoadAsync();
            await InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Event Handler for when the filter is updated Externally 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual async void FilterUpdated(object sender, EventArgs e) => await this.Service.ResetListAsync();
        //{
        //    await this.Service.ResetListAsync();
        //    await this.Paging.LoadAsync();
        //    await InvokeAsync(this.StateHasChanged);
        //}

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected async void OnViewAsync<TForm>(int id) where TForm: IComponent
        {
            if (this.UIOptions.UseModalViewer && this._BootstrapModal != null)
            {
                var modalOptions = new BootstrapModalOptions()
                {
                    ModalBodyCSS = "p-0",
                    ModalCSS = "modal-xl",
                    HideHeader = true,
                };
                modalOptions.Parameters.Add("ID", id);
                await this._BootstrapModal.Show<TForm>(modalOptions);
            }
            else this.NavigateTo(new EditorEventArgs(PageExitType.ExitToView, id, this.Service.RecordConfiguration.RecordName));
        }

        /// <summary>
        /// Method called when the user clicks on a row Edit button.
        /// </summary>
        /// <param name="id"></param>
        protected async void OnEditAsync<TForm>(int id) where TForm : IComponent
        {
            if (this.UIOptions.UseModalEditor && this._BootstrapModal != null)
            {
                var modalOptions = new BootstrapModalOptions()
                {
                    ModalBodyCSS = "p-0",
                    ModalCSS = "modal-xl",
                    HideHeader = true
                };
                modalOptions.Parameters.Add("ID", id);
                await this._BootstrapModal.Show<TForm>(modalOptions);
            }
            else this.NavigateTo(new EditorEventArgs(PageExitType.ExitToEditor, id, this.Service.RecordConfiguration.RecordName));
        }

        public override void Dispose()
        {
            try
            {
                this.Service.ListHasChanged -= this.OnRecordsUpdate;
                this.Paging.PageHasChanged -= this.UpdateUI;
            }
            catch { }
            base.Dispose();
        }
    }
}
