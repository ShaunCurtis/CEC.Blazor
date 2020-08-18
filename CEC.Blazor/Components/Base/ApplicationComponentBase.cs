using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Authorization;
using CEC.Blazor.Services;
using CEC.Blazor.Data;
using CEC.Routing.Services;
using System.Linq;
using CEC.Blazor.Components.UIControls;
using CEC.Blazor.Components.Modal;

namespace CEC.Blazor.Components.Base
{
    public class ApplicationComponentBase : ComponentBase, IGuidComponent, IDisposable
    {
        /// <summary>
        /// Injected Property for the Browser Service
        /// Provides access to JS based browser routiines such as fining the browser dimensions and resetting
        /// the browser history
        /// </summary>
        [Inject]
        public BrowserService BrowserService { get; set; }

        /// <summary>
        /// Injected Navigation Service
        /// </summary>
        [Inject]
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// Injected Configuration service giving access to the Application Configurstion Data
        /// </summary>
        [Inject]
        protected IConfiguration AppConfiguration { get; set; }

        /// <summary>
        /// Injected Authentication Service giving access to logged in  user data
        /// </summary>
        [Inject]
        public AuthenticationStateProvider AuthenticationState { get; set; }

        /// <summary>
        /// Injected Router Session Object
        /// </summary>
        [Inject]
        public RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// The default alert used by all inherited components
        /// Used for Editor Alerts, error messages, ....
        /// </summary>
        [Parameter] public Alert AlertMessage { get; set; } = new Alert();

        /// <summary>
        /// Cascaded Parameter if the Component is used in Modal mode
        /// </summary>
        [CascadingParameter]
        public BootstrapModal Parent { get; set; }

        /// <summary>
        /// A unique Guid for this  instance of the Component
        /// set by the dubugger
        /// </summary>
        public Guid GUID { get; } = Guid.NewGuid();

        /// <summary>
        /// The Page Type - used in logging
        /// set by the dubugger
        /// </summary>
        public string ClassName => this.GetType().ToString();

        /// <summary>
        /// The last page visited. Exiting this page returns to this page
        /// </summary>
        public string ReturnPageUrl { get; set; }

        /// <summary>
        /// Property to check the state of the page during re-rendering
        /// </summary>
        protected bool FirstLoad { get; set; } = true;

        /// <summary>
        /// Property to check if the page is loading set internally
        /// </summary>
        public bool Loading { get; protected set; } = true;

        /// <summary>
        /// Property that tells us if we are navigating within the component 
        /// such as where the component has multiple routes and we are navigating between them
        /// </summary>
        protected bool IsIntraComponentNavigation => (!this.FirstLoad) && NavManager.Uri != RouterSessionService.LastPageUrl;


        /// <summary>
        /// Boolean Property to check if this component is in Modal Mode
        /// </summary>
        public bool IsModal => this.Parent != null;

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

        protected async override Task OnInitializedAsync()
        {
            await this.LoadAsync();
            await base.OnInitializedAsync();
            // kick off a manual UI update
            this.UpdateState();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            this.FirstLoad = false;
            base.OnAfterRender(firstRender);
        }

        /// <summary>
        /// Load routine to call at startup and during program execution
        /// Put stuff in here that you may want to run on a reload of the object as well at intialisition
        /// </summary>
        protected async virtual Task LoadAsync()
        {
            if (this.AuthenticationState != null) await this.GetUserAsync();
            this.ReturnPageUrl = this.RouterSessionService.LastPageUrl;
        }

        /// <summary>
        /// Method to get the current user from the Authentication State
        /// </summary>
        protected async Task GetUserAsync()
        {
            // Get the current user
            var auth = await this.AuthenticationState.GetAuthenticationStateAsync();
            this.CurrentUser = auth.User.Identity.Name;
            foreach (var c in auth.User.Claims.ToList())
            {
                if (c.Type.Contains("nameidentifier"))
                {
                    this.CurrentUserID = c.Value;
                    break;
                }
            }
        }

        /// <summary>
        /// Event Method avaiable to force a UI state update
        /// </summary>
        public virtual void UpdateState() => InvokeAsync(this.StateHasChanged);

        /// <summary>
        /// Event method to force a UI Update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void UpdateState(object sender, EventArgs e) => this.UpdateState();

        /// <summary>
        /// Generic Navigator.  Uses the Record Configuation information for specific routing
        /// </summary>
        protected virtual void NavigateTo(EditorEventArgs e)
        {
            switch (e.ExitType)
            {
                case PageExitType.ExitToList:
                    this.NavManager.NavigateTo(string.Format("/{0}/", e.RecordName));
                    break;
                case PageExitType.ExitToView:
                    this.NavManager.NavigateTo(string.Format("/{0}/View?id={1}", e.RecordName, e.ID));
                    break;
                case PageExitType.ExitToEditor:
                    this.NavManager.NavigateTo(string.Format("/{0}/Edit?id={1}", e.RecordName, e.ID));
                    break;
                case PageExitType.SwitchToEditor:
                    this.NavManager.NavigateTo(string.Format("/{0}/Edit/?id={1}", e.RecordName, e.ID));
                    break;
                case PageExitType.ExitToNew:
                    this.NavManager.NavigateTo(string.Format("/{0}/New?qid={1}", e.RecordName, e.ID));
                    break;
                case PageExitType.ExitToLast:
                    if (!string.IsNullOrEmpty(this.RouterSessionService.LastPageUrl)) this.NavManager.NavigateTo(this.RouterSessionService.LastPageUrl);
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
            if (IsModal) this.Parent.Close(BootstrapModalResult.Exit());
        }

        /// <summary>
        /// Modal Cancel
        /// </summary>
        public void ModalCancel()
        {
            if (IsModal) this.Parent.Close(BootstrapModalResult.Cancel());
        }

        /// <summary>
        /// Modal OK
        /// </summary>
        public void ModalOK()
        {
            if (IsModal) this.Parent.Close(BootstrapModalResult.OK());
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
