using CEC.Blazor.Components.Modal;
using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Services;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.Base
{
    public abstract class FormBase : Component, IForm, IDisposable
    {
        /// <summary>
        /// Injected Navigation Service
        /// </summary>
        [Inject]
        protected NavigationManager NavManager { get; set; }

        /// <summary>
        /// Injected Configuration service giving access to the Application Configurstion Data
        /// </summary>
        [Inject]
        protected IConfiguration AppConfiguration { get; set; }

         /// <summary>
        /// Injected Router Session Object
        /// </summary>
        [Inject]
        protected RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// Injected Property for the Browser Service
        /// Provides access to JS based browser routiines such as fining the browser dimensions and resetting
        /// the browser history
        /// </summary>
        [Inject]
        public BrowserService BrowserService { get; set; }

        /// <summary>
        /// Cascaded Parameter if the Component is used in Modal mode
        /// </summary>
        [CascadingParameter]
        protected BootstrapModal ModalParent { get; set; }

        /// <summary>
        /// Boolean Property to check if this component is in Modal Mode
        /// </summary>
        public bool IsModal => this.ModalParent != null;

        /// <summary>
        /// Cascaded Authentication State Task from <CascadingAuthenticationState> in App
        /// </summary>
        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        /// <summary>
        /// Property holding the current user name
        /// Used in SessionState Service Management
        /// </summary>
        public string CurrentUser { get; protected set; }

        /// <summary>
        /// Guid string for user
        /// </summary>
        public string CurrentUserID { get; set; }

        /// <summary>
        /// UserName without the domain name
        /// </summary>
        public string CurrentUserName => (!string.IsNullOrEmpty(this.CurrentUser)) && this.CurrentUser.Contains("@") ? this.CurrentUser.Substring(0, this.CurrentUser.IndexOf("@")) : string.Empty;

        /// <summary>
        /// OnRenderAsync Method from Component
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        public async override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender) {
                await GetUserAsync();
            }
            await base.OnRenderAsync(firstRender);
        }

        /// <summary>
        /// Method to get the current user from the Authentication State
        /// </summary>
        protected async Task GetUserAsync()
        {
            if (this.AuthenticationStateTask != null)
            {
                var state = await AuthenticationStateTask;
                // Get the current user
                this.CurrentUser = state.User.Identity.Name;
                var x = state.User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"));
                this.CurrentUserID = x?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Generic Navigator.  Uses the Record Configuation information for specific routing
        /// </summary>
        protected virtual void NavigateTo(EditorEventArgs e)
        {
            switch (e.ExitType)
            {
                case PageExitType.ExitToList:
                    this.NavManager.NavigateTo($"/{e.RecordName}/");
                    break;
                case PageExitType.ExitToView:
                    this.NavManager.NavigateTo($"/{e.RecordName}/View?id={e.ID}");
                    break;
                case PageExitType.ExitToEditor:
                    this.NavManager.NavigateTo($"/{e.RecordName}/Edit?id={e.ID}");
                    break;
                case PageExitType.SwitchToEditor:
                    this.NavManager.NavigateTo($"/{e.RecordName}/Edit?id={e.ID}");
                    break;
                case PageExitType.ExitToNew:
                    this.NavManager.NavigateTo($"/{e.RecordName}/New?qid={e.ID}");
                    break;
                case PageExitType.ExitToLast:
                    if (!string.IsNullOrEmpty(this.RouterSessionService.ReturnRouteUrl)) this.NavManager.NavigateTo(this.RouterSessionService.ReturnRouteUrl);
                    this.NavManager.NavigateTo("/");
                    break;
                case PageExitType.ExitToRoot:
                    this.NavManager.NavigateTo("/");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Modal Exit
        /// </summary>
        public void ModalExit()
        {
            if (IsModal) this.ModalParent.Close(BootstrapModalResult.Exit());
        }

        /// <summary>
        /// Modal Cancel
        /// </summary>
        public void ModalCancel()
        {
            if (IsModal) this.ModalParent.Close(BootstrapModalResult.Cancel());
        }

        /// <summary>
        /// Modal OK
        /// </summary>
        public void ModalOK()
        {
            if (IsModal) this.ModalParent.Close(BootstrapModalResult.OK());
        }

        /// <summary>
        /// Async Dispose event to clean up event handlers
        /// </summary>
        public virtual Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Dispose event to clean up event handlers
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// IDisposable Method to dispose of resources
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose();
            var task = this.DisposeAsync();
            task.Wait();
        }
    }
}

