using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.SPA.Components.UIControls
{
    public class UIHelloDiv : IComponent
    {
        /// <summary>
        /// Child Content to render
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Render Handle passed when Attach method called
        /// </summary>
        private RenderHandle _renderHandle;

        /// <summary>
        /// Render Fragment to render this object
        /// </summary>
        private readonly RenderFragment _componentRenderFragment;

        /// <summary>
        /// Boolean Flag to track if there's a pending render event queued
        /// </summary>
        private bool _RenderEventQueued;

        /// <summary>
        /// Class Initialization Event
        /// builds out the component renderfragment to pass to the Renderer when an render event is queued on the renderer
        /// </summary>
        public UIHelloDiv() => _componentRenderFragment = builder =>
        {
            this._RenderEventQueued = false;
            BuildRenderTree(builder);
        };

        /// <summary>
        /// Method called to attach the object to a RenderTree
        /// The render handle gives the component access to the renderer and its render queue
        /// </summary>
        /// <param name="renderHandle"></param>
        public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

        /// <summary>
        /// Method called by the Renderer when one or more object parameters have been set or changed
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            if (!this._RenderEventQueued) _renderHandle.Render(_componentRenderFragment);
            return Task.CompletedTask;
        }

        /// <summary>
        /// inherited
        /// Builds the render tree for the component
        /// </summary>
        /// <param name="builder"></param>
        protected void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = -1;
            builder.OpenElement(i++, "div");
            builder.AddAttribute(i++, "class", "hello-world");
            if (this.ChildContent != null) builder.AddContent(i++, ChildContent);
            else builder.AddContent(i++, (MarkupString)"<h4>Hello World</h4>");
            builder.CloseElement();
        }
    }
}
