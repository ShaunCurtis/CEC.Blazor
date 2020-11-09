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
    public class ListFormBase<TRecord, TContext> :
        ControllerServiceFormBase<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
         where TContext : DbContext
    {

        /// <summary>
        /// Paging Interface giving access to the specific service that sorts the dataset into pages and tracks the page being displayed
        /// </summary>
        public IControllerPagingService<TRecord> Paging => this.Service != null ? (IControllerPagingService<TRecord>)this.Service : null;

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

        protected async override Task OnRenderAsync(bool firstRender)
        {
            var page = 1;
            if (this.IsService)
            {
                if (firstRender)
                {
                    // Reset the Service if this is the first load
                    await this.Service.Reset();
                    this.ListTitle = string.IsNullOrEmpty(this.ListTitle) ? $"List of {this.Service.RecordConfiguration.RecordListDecription}" : this.ListTitle;

                }
                // Load the filters for the recordset
                this.LoadFilter();
                if (this.IsViewManager && !this.ViewManager.ViewData.GetFieldAsInt("Page", out page)) page = 1;
                // Load the paged recordset
                await this.Service.LoadPagingAsync(page);
                this.Loading = false;
            }
            await base.OnRenderAsync(firstRender);

        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.Paging.PageHasChanged += this.OnPageChanged;
                this.Service.ListHasChanged += this.OnRecordsUpdate;
                this.Service.FilterHasChanged += this.FilterUpdated;
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Method called to load the filter
        /// </summary>
        protected virtual void LoadFilter()
        {
            if (IsService) this.Service.FilterList.OnlyLoadIfFilters = this.OnlyLoadIfFilter;
        }

        protected void OnPageChanged(object sender, int page)
        {
            if (this.IsViewManager) this.ViewManager.ViewData.SetField("Page", page);
            InvokeAsync(this.Render);
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
                this.Render();
                await this.Paging.LoadAsync();
            }
            this.Loading = false;
            this.Render();
        }

        /// <summary>
        /// Handler for a UpdatePage event - normally linked to the Pager PageHasChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recordno"></param>
        protected void UpdateUI(object sender, int recordno) => InvokeAsync(Render);

        /// <summary>
        /// Event Handler for when the List is updated Externally i.e. not by the Pager 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual async void ListUpdated(object sender, EventArgs e)
        {
            await this.Paging.LoadAsync();
            await InvokeAsync(this.Render);
        }

        /// <summary>
        /// Event Handler for when the filter is updated Externally 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual async void FilterUpdated(object sender, EventArgs e) => await this.Service.ResetListAsync();

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected async void OnModalAsync<TForm>(int id) where TForm : IComponent
        {
                var modalOptions = new ModalOptions()
                {
                    HideHeader = true
                };
                modalOptions.Parameters.Add("ModalBodyCSS", "p-0");
                modalOptions.Parameters.Add("ModalCSS", "modal-xl");
                modalOptions.Parameters.Add("ID", id);
                await this.ViewManager.ShowModalAsync<TForm>(modalOptions);
        }

        /// <summary>
        /// Method called when the user clicks on a row in the viewer.
        /// </summary>
        /// <param name="id"></param>
        protected async void OnViewAsync<TView>(int id) where TView : IView
        {
            await this.ViewManager.LoadViewAsync<TView>("ID", id);
        }

        public override void Dispose()
        {
            try
            {
                this.Paging.PageHasChanged -= this.OnPageChanged;
                this.Service.ListHasChanged -= this.OnRecordsUpdate;
                this.Service.FilterHasChanged -= this.FilterUpdated;
            }
            catch { }
            base.Dispose();
        }
    }
}
