# A Deep Dive into Blazor Components

The Blazor UI is build from components.  In this article we'll look at the anatomy of a component, the component life cycle and how Blazor uses and manages components to run the UI. 

To understand where components fit in, let's look at how a Blazor Application functions.

A Blazor Server Application has three contexts:

1. The Server Application - this is the DotNetCore application running on the Web Server.  There's only one Server Application running for all clients, with one instance of all the Singleton Services defined in *Startup.cs*.
2. The Client Application [aka the SPA] - this is the Blazor web page running in the client web browser.  It's the client end of a SignalR session with the Server Application.
3. The SignalR Hub Session - this is the Blazor Hub running within the Server Application.  There's one Hub Session per Client Application.  It's the server end of the SignalR session between the browser and the server.

To expand on this so there's no misunderstanding, two browser windows open on the same application are two totally separate client applications, with two Hub Sessions.  They share the same singleton services, but that's all.  It the same as having two copies of Visual Studio running.   \<F5\> the browser and the application restarts - the same as closing and then re-starting a desktop application.

A Blazor WASM Application has one context:

1. The Client Application [aka the SPA] - the Blazor web page running in the client web browser. It's standalone.
Again so there's no misunderstanding, two browser windows or tabs open on the same application are two totally separate client applications. The same as having two copies of Visual Studio running. <F5> the browser and the application restarts - just like closing and then re-starting a desktop application.

## The Client Application

### Blazor Server

A Blazor Client Application starts with an HTTP request to a Blazor configured DotNet Core Server Application running on a web server.

1. If static pages is configured, the server first checks if a static file exists.  If one does, it servers it - no client application involved, just standard HTTP operations.  This is how requests for CSS, JS and other resource files are handled.
2. If no static page exists, it assumes it's a Blazor page. The Server Application builds the initial page from *_Host.chtml*.  This is a standard HTML page, with stylesheet and script references and a single *\<app\>* component.

#### _Host.chtml

The important sections in *_Host.chtml* are:
```html
<app>
    <component type="typeof(App)" render-mode="ServerPrerendered" />
</app>
```

App is a component class defined in *App.razor*. It's the root component in the component tree that represents the requested route.  What get's built depends on the *render-mode* setting. *Server* renders a blank page while *ServerPrerendered* creates a static version of the rendertree.  It's important to understand what's going on at this point.  We don't have a Client application running, we're in the bootstrap process.  The page contains the code - in *blazor.server.js* - to start the Client Application, but until it's first rendered and *blazor.server.js* is run there's no SignalR Session and therefore no Client Application. The Blazor Client Application gets loaded by:

```html
<script src="_framework/blazor.server.js"></script>
```

Once *blazor.server.js* loads, the client application is established in the browser page and a SignalR connection estabished with the server.  However, at this point we have a static page, not a live page - there's no wiring into JSInterop and the server site components.  To complete the initial load, the Client Application calls the Blazor Hub Session and requests a complete server render of the rendertree.  It then applies the resultant DOM changes to the Client Application DOM.

Everything is now wired up an running.  We have a Client Application running with a live SignalR connection to the Blazor Hub.  The Hub Session has a Renderer object that maintains a server side copy of the DOM and pushes any changes down to the Client Application through the SignalR connection.  Client Application events are now routed via SignalR to the Blazor Hub Renderer and mapped to component events/methods in the rendertree.  All page changes within the client application are routed through the SignalR connection - there's no standard http requests - and handled by the configured router.  A request to navigate to a page outside the routed context causes a full browser load, closing the SignalR connection and thus the Client Application.

### Blazor WASM
A Blazor Client Application starts with an HTTP request to a web server swith the Blazor files installed with a index.html.

#### Index.html
The Blazor specific section are:

```html
<app>
    ....
</app>
```

App is a component class defined in App.razor. It's the root component for the render tree. It's important to understand what's going on at this point. We don't have a Client Application running, we're in the "bootstrap" process. The page contains the code - in blazor.webassembly.js - to start the Client Application, but until it's first rendered by the web browser, and blazor.webassemblyjs is run, and the WASM code downloaded and installed there's no Client Application. The Client Application gets loaded by:

```html
<script src="_framework/blazor.webassembly.js"></script>
```

Once the WASM code is loaded, it creates the render tree from *App* down and re-renders the page.  We now have a live apllication.

From this point on Server and WASM are the same.

#### App.razor
 
*App.razor* is in the root component and looks like this:

```html
<Router AppAssembly="@typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
    <NotFound>
        <LayoutView Layout="@typeof(MainLayout)">
            <p>Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```
The component initializes Routing through the Router Component.  It loads a list of the Server Application routes - Routed Components on the site - and passes the Router Component selected to the *RouteView* component.  If no route exists, it builds the *NotFound* section.  Note that *RouteView* defines a default layout page for the application.

#### MainLayout.razor
 
Layout pages inherit from *LayoutComponentBase*.  A basic layout page looks like this:

```html
@inherits LayoutComponentBase

<div class="sidebar">
    <NavMenu />
</div>

<div class="main">
    <div class="top-row px-4">
        <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
    </div>

    <div class="content px-4">
        @Body
    </div>
</div>
```
Another Blazor Component.  A sidebar containing a *\<NavMenu\>* component, a top bar with standard HTML and a content area where the Routed Component is inserted and rendered at the *@Body* placeholder.

## Components

Everything in the Blazor UI is a component: components are normal C# classes that implement *IComponent*.  The *IComponent* interface definition is:

```C#
public interface IComponent
{
    void Attach(RenderHandle renderHandle);
    Task SetParametersAsync(ParameterView parameters);
}
```

My first reaction on seeing this was "What? Something missing here.  Where's all those events and initialization methods?"

Lets look at what is defined in more detail.  The Blazor Hub Session has a Renderer object that holds the App component rendertree for the current URL.  To quote the class documentation:

Renderer provides mechanisms:
1. For rendering hierarchies of *IComponent* instances;
2. Dispatching events to them;
3. Notifying when the user interface is being updated.

A RenderHandle structure:
1. Allows a component to interact with its renderer.

Going back to the *IComponent* interface:
1. *Attach* lets us attach an *IComponent* object to the Rendertree of a Renderer object through a RenderHandle structure.  The IComponent uses the Render Handle to queue individual component renders onto the Renderer RenderQueue.
2. *SetParametersAsync* lets the Renderer pass parameter changes to the component.

#### A Simple *IComponent* Implementation

Lets look at a simple *IComponent* implementation - building a standard HTML Div with content.

```C#
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CEC.Blazor.Components.UIControls
{
    public class UIHelloDiv : IComponent
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private RenderHandle _renderHandle;
        private readonly RenderFragment _componentRenderFragment;
        private bool _RenderEventQueued;

        public UIHelloDiv() => _componentRenderFragment = builder =>
        {
            this._RenderEventQueued = false;
            BuildRenderTree(builder);
        };

        public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            if (!this._RenderEventQueued) _renderHandle.Render(_componentRenderFragment);
            return Task.CompletedTask;
        }

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
```

Import points:
1. There's one Parameter - the Child Content.
2. When an instance of the class initializes, it builds a RenderFragment in *_componentRenderFragment*.  This gets executed by the Renderer whenever the RenderFragment is queued into the Renderer *RenderQueue*.
3. When *SetParametersAsync* is called any relevant parameters are applied to the class properties, and *_componentRenderFragment* is queued into the the renderer's *RenderQueue* through the *RenderHandle*.
4. Any components in the child content are not this component's responsibility.  The renderer will call their *SetParametersAsync* method with the relevant up to date parameters, and the components are responsible for queueing their updated renderfragments. 
5. There's no *OnInitialized*, *OnAfterRender*, *StateHasChanged*,...  These are all part of the *ComponentBase* implementation of *IComponent*.

#### Routed Components

Everything's a component, but not all components are equal.  **Routed Components** are a little special.

They contain *@page* routing directives and optionally a *@Layout* directive.

```html
@page "/WeatherForecast"
@page "/WeatherForecasts"

@layout MainLayout
```

The router builds a list of all the routed components and the routes they advertise.  When a new route is called by the Navigation Manager, the appropriate routed component is loaded by the *RouterView* component in *App.razor* using either the explicitly specified Layout or the default Layout defined in *App.razor*.

Don't think of routed components as pages. It seems obvious to do so, but don't.  You'll attribute lots of page properties to routed components that don't apply, then get confused when routed components don't behave like a page!


#### *ComponentBase*

*ComponentBase* is the core Blazor implementation of *IComponent*.  All *.razor* files by default inherit from it.  It's important to understand that *ComponentBase* is just one implementation of the *IComponent* interface.  It doesn't define a component.  *OnInitialized* is not a component lifecycle method, it's a *ComponentBase* lifecycle method.  The simple *IComponent* implementation above has no connection with *ComponentBase*.

#### *ComponentBase* Lifecycle and Events

There's plenty of articles and information on the web regurgitating the same basic lifecycle information.  I'm going to concentrate here on certain often misunderstood aspects of the lifecycle: there's more to the lifecycle that just the initial component load covered in most articles.

We need to consider four types of event:
1. Initialization of the component
2. Component parameter changes
3. Component events
4. Component disposal

There are six exposed Events/Methods and their async equivalents:
1. *SetParametersAsync*
2. *OnInitialized* and *OnInitializedAsync*
3. *OnParametersSet* and *OnParametersSetAsync*
4. *OnAfterRender* and *OnAfterRenderAsync*
5. *Dispose* - if *IDisposable* is implemented
6. *StateHasChanged*

The standard class initialization method builds a RenderFragment that gets run by the Renderer whenever it's queued.  It sets the two private class variables to false and runs BuildRenderTree.

```C#
public ComponentBase()
{
    _renderFragment = builder =>
    {
        _hasPendingQueuedRender = false;
        _hasNeverRendered = false;
        BuildRenderTree(builder);
    };
}
```
 

*SetParametersAsync* sets the properties for the submitted parameters. It only runs *RunInitAndSetParametersAsync* - and thus *OnInitialized* and *OnInitializedAsync* - on initialization.  Either way, the final call is to *CallOnParametersSetAsync*.  Note that *CallOnParametersSetAsync* waits on *OnInitializedAsync* to complete before calling *CallOnParametersSetAsync*.

```C#
public virtual Task SetParametersAsync(ParameterView parameters)
{
    parameters.SetParameterProperties(this);
    if (!_initialized)
    {
        _initialized = true;
        return RunInitAndSetParametersAsync();
    }
    else return CallOnParametersSetAsync();
}

private async Task RunInitAndSetParametersAsync()
{
    OnInitialized();
    var task = OnInitializedAsync();
    if (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled)
    {
        StateHasChanged();
        try { await task;}
        catch { if (!task.IsCanceled) throw; }
    }
    await CallOnParametersSetAsync();

```

*CallOnParametersSetAsync* calls *OnParametersSet* and *OnParametersSetAsync*, followed by *StateHasChanged*.  If *OnParametersSetAsync()* needs waiting on, it waits and after it completes re-runs *StateHasChanged*. 

```C#
private Task CallOnParametersSetAsync()
{
    OnParametersSet();
    var task = OnParametersSetAsync();
    var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
        task.Status != TaskStatus.Canceled;

    StateHasChanged();

    return shouldAwaitTask ?
        CallStateHasChangedOnAsyncCompletion(task) :
        Task.CompletedTask;
}

private async Task CallStateHasChangedOnAsyncCompletion(Task task)
{
    try { await task; }
    catch 
    {
        if (task.IsCanceled) return;
        throw;
    }
    StateHasChanged();
}
```

Lastly, lets look at *StateHasChanged*.  If a render is pending i.e. the renderer hasn't got round to running the queued render request, it closes - whatever changes have been made will be captured in the queued render.  If not, it sets the  *_hasPendingQueuedRender* class flag and calls the Render method on the RenderHandle.  This queues *_renderFragement* into the Renderer RenderQueue.  When the queue runs *_renderFragment* - see above - it sets the two class flags to false and runs BuildRenderTree.

```C#
protected void StateHasChanged()
{
    if (_hasPendingQueuedRender) return;
    if (_hasNeverRendered || ShouldRender())
    {
        _hasPendingQueuedRender = true;
        try { _renderHandle.Render(_renderFragment);}
        catch {
            _hasPendingQueuedRender = false;
            throw;
        }
    }
}
```

Some key points to note:

1. *OnInitialized* and *OnInitializedAsync* only get called during initialization. They are overused.  The only code that belongs in them is stuff that never changes after the initial load event.

2. *OnParametersSet* and *OnParametersSetAsync* get called whenever the parent component makes changes to the parameter set for the component or a captured cascaded parameter changes.  Any code that needs to respond to parameter changes need to live here.

3. Component rendering (either through the markupcode or *BuildRenderTree*) happens after the *OnParametersSet* events either on initialization or a parameter change, and after an Event Callback occurs (such as responding to a mouse or keyboard event).

4. *OnAfterRender* and *OnAfterRenderAsync* occur at the end of all four events.  *firstRender* is only true on component initialization.

5. *StateHasChanged* is called automatically after the Initialized events, after OnParametersSet events and after any event callback.  You don't need to call it separately.


#### Building Components

Components can be defined in three ways:
1. As a *.razor* file with an code inside an *@code* block.
2. As a *.razor* file and a code behind *.razor.cs* file.
3. As a pure *.cs* class file inheriting from *ComponentBase* or a *ComponentBase* inherited class, or implementing *IComponent*.

##### All in One Razor File

HelloWorld.razor

```html
<div>
@HelloWorld
</div>

@code {
[Parameter]
public string HelloWorld {get; set;} = "Hello?";
}
```

##### Code Behind

HelloWorld.razor

```html
@inherits ComponentBase
@namespace CEC.Blazor.Server.Pages

<div>
@HelloWorld
</div>
```
HelloWorld.razor.cs

```c#
namespace CEC.Blazor.Server.Pages
{
    public partial class HelloWorld : ComponentBase
    {
        [Parameter]
        public string HelloWorld {get; set;} = "Hello?";
    }
}
```

##### C# Class

HelloWorld.cs

```c#
namespace CEC.Blazor.Server.Pages
{
    public class HelloWorld : ComponentBase
    {
        [Parameter]
        public string HelloWorld {get; set;} = "Hello?";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, (MarkupString)this._Content);
            builder.CloseElement();
        }
    }
}
```

#### Some Observations

1. There is a tendancy to pile all the component code into *OnInitialized* and *OnInitializedAsync* and then use events to drive StateHasChanged updates in the component tree.  Getting the relevant code into the right places in the liefcycle will remove most of the reliance on events.

2. *StateHasChanged* is trigger far to often, normally because code is in the wrong place in the component lifecycle.

3. Components are underused in the UI.  The same code/markup blocks are used repeatedly.  The same rules apply to code/markup blocks as to C# code.





