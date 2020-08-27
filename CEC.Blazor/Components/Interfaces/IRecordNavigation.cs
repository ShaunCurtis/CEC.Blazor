using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace CEC.Blazor.Components
{
    public interface IRecordNavigation
    {
        /// <summary>
        /// Injected Navigation Service
        /// </summary>
        [Inject]
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// Injected Router Session Object
        /// </summary>
        [Inject]
        public RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// Generic Navigator.  Uses the Record Configuation information for specific routing
        /// </summary>
        public void NavigateTo(EditorEventArgs e)
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

    }
}
