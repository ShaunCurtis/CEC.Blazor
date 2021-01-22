using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace CEC.Blazor.Services
{
    public class BrowserService
    {
        private readonly IJSRuntime _js;

        private bool ExitCheckState { get; set; }

        public BrowserService(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>
        /// Method to Get the dimmensions of the browser window
        /// </summary>
        /// <returns></returns>
        public async Task<BrowserDimension> GetDimensions() => await _js.InvokeAsync<BrowserDimension>("getDimensions");

        /// <summary>
        /// Method to clear the browser history
        /// </summary>
        /// <returns></returns>
        public void ClearBrowserHistory() => _js.InvokeAsync<bool>("clearHistory");

        /// <summary>
        /// Method to set the focus on a specific HTML control/element
        /// </summary>
        /// <param name="control"></param>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        public void SetFocus(string control, bool firstRender)
        {
            if (firstRender) _js.InvokeAsync<bool>("setFocus", control);
        }

        /// <summary>
        /// Method to set or unset the browser onbeforeexit challenge
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void SetPageExitCheck(bool action)
        {
            if (action != ExitCheckState) _js.InvokeAsync<bool>("setExitCheck", action);
            ExitCheckState = action;
        }

    }

    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
