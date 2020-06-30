using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace CEC.Blazor.Services
{
    public class BrowserService
    {
        private readonly IJSRuntime _js;

        public BrowserService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<BrowserDimension> GetDimensions()
        {
            return await _js.InvokeAsync<BrowserDimension>("getDimensions");
        }

        public async Task<bool> ClearBrowserHistory()
        {
            return await _js.InvokeAsync<bool>("clearHistory");
        }

        public async Task<bool> SetFocus(string control, bool firstRender)
        {
            if (firstRender) return await _js.InvokeAsync<bool>("setFocus", control);
            return false;
        }
    }

    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
