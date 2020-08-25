# Building a Blazor Project
# Part 3 - The UI

In the first two articles I looked at project structure and implementing the data layers.  In this article, I look at how the presentation layer is implemented in Blazor and an overview of how I use implement it in projects.  I've split this section into three articles: an overview; boilerplating CRUD operations; and finally implementing UI code in components.

The Blazor UI is build from components - we'll look at them in more detail later in this article.

To understand what's really going on let's look at the pieces that make up the Blazor Application UI and how they interact.

## The Application

A Blazor Server Application has two application contexts:
1. The Server Application - this is the DotNetCore application running by the Web Server.  There's only one Server Application running for all clients, with one instance of all the SingleTon Services defined in *Startup.cs*.
2. The Client Application [aka the SPA] - this is the Blazor web page running in the Client Web Browser.

You need to get these two contexts clear in your head.

## First Load

As with any application there's an initial Client Application load.  This is an HTTP request to a routed page within the DotNet Core Server Application running on the web server.  Note a routed page,  static pages requests are just that, normal HTTP operations, there's no Client Application.

The Server Application uses *_Host.html* as it's template HTML page.  This is a standard HTML page, with stylesheet and script references and a short *\<body\>* section containing a *\<app\>* tag.  App is the root component of Blazor client application. The Server Application inserts the App component HTML and Javascript content into the *\<app\>* tag and passes the page to the browser.

The browser takes the supplied page and renders it. The key bit for Blazor is:

```html
<script src="_framework/blazor.server.js"></script>
```

This initiates the Blazor Client Application and establishes a SignalR connection to the server.  It's first action is to make a request for App component content, and update the DOM with the returned content.  Why the second request?  The first load creates a static page, there's no wiring between the server side object and the client side events.  *Blazor.Server.js* needs to be running to provide the infrastructure for this to happen.

At this point the Client Application is up and running and further routed page changes take place through the SignalR connection - there's no standard http requests.  A request to navigate to a page outside the routed context causes a full browser load, closing the SignalR connection and thus the Client Application.

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
The component initializes Routing through the Router Component.  This loads a list of all the Server Application routes - Routed Components on the site -  finds the Routed Component and passes it to the *RouteView* component.  If no route exists, it builds the *NotFound* section.  Note that *RouteView* defines a default layout page for the application.

#### MainLayout.razor
 
*MainLayout.razor* inherits from *LayoutComponentBase*.  A basic layout page looks like this:

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
It's just another Blazor Component.  A sidebar containing a *\<NavMenu\>* component, a top bar with standard HTML and a content area where the Routed Component is inserted and rendered at the *@Body* placeholder.

## The Component

Everything in the Blazor UI is a component from the *App* component down.  Components contain sub-components, contain sub-sub-components in a component tree.  All components inherit from the base class *ComponentBase*.   A *.razor* file by default inherits from *ComponentBase*.

It's hard, but forget the classic concept of a page in Razor.  The only HTML page is  *_Host.chtml*.  Everything else is a component.  After the Clinet Application starts it's all routing, no browser navigation.  Components get loaded into and out of the *Router* component in *App.razor* as the user gets routed around the application.  Beyond the initial page load, all communication takes place over a SignalR connection and only involves DOM manipulation on the browser.

There are two types of component:
1. Components
2. Routed Components

The only difference is Routed Components contain *@page* routing directives and optionally a *@Layout* directive.

```html
@page "/WeatherForecast"
@page "/WeatherForecasts"

@layout MainLayout
```

Routed Components *Router* component in *App.razor* using either the explicitly specified Layout or the default Layout defined in *App.razor*.

Normal components load where they are declared.

```html
<WeatherList UIOptions="this.UIOptions" ></WeatherList>
```

#### Building Components

Components can be defined in three ways:
1. As a *.razor* file with an code inside an *@code* block.
2. As a *.razor* file and a code behind *.razor.cs* file.
3. As a pure *.cs* class file inheriting from *ComponentBase* or a *ComponentBase* inherited class.

#### The Blazor RenderTree and Client Application Updating

Everything is a component, so the current page exists in two forms:
1. A C# component class tree representing *App*.
2. The HTML and JS representation presented in the browser.

At compile time, components defined in *.razor* files with markup code are converted into RenderTreeBuilder code in C# classes or partial classes.  The output files are called *.razor.g.cs* files and reside in the *obj* folder.

The render tree for a *ComponentBase* is obtained by calling the *BuildRenderTree* method.

When an event causes a component to be rendered, the new DOM built from the RenderTree is compared with the existing DOM through a process called Diffing.  Any differences are transmitted to the Client Application through the SignalR connection, and applied to the client DOM.


#### The Component Lifecycle and Events

There are plenty of regurgatated articles and information covering basic component lifecycle.  I am going to concentrate on certain often misunderstood aspects of the lifecycle: there's more to the lifecycle that just the initial component load covered in most articles.

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
5. *Dispose* - if IDisposable is implemented
6. *StateHasChanged*

Blazor interfaces with Components though the IComponent Interface.  A quick look at the interface gives us somme insight into how components work (I've removed the commenting for brevity):

```C#
public interface IComponent
{
    void Attach(RenderHandle renderHandle);
    Task SetParametersAsync(ParameterView parameters);
}
```

Blazor uses *Attach* to connect the component to something and *SetParametersAsync* to "run" the component. The only other exposed method is *SetParametersAsync*.  This is **IMPORTANT**, it gets called whenever any component parameters are set or changed.

*SetParametersAsync* looks like this:

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
```

It sets the properties from the submitted parameters, but only runs *OnInitialized* and *OnInitializedAsync* on initialization.  It also waits for *OnInitializedAsync* to complete. On either path the final call is to *CallOnParametersSetAsync*.

```C#
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
}
```
Lastly, lets look at *StateHasChanged*:

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
It aborts if rendering is already queued to happen.  If not (ShouldRender() always returns true) it sets the queued flag and then calls render on the render handle.  This in turn calls BuildRenderTree on the component.

Some key points to note:

1. *OnInitialized* and *OnInitializedAsync* only get called when the component is initialized. They are overused.  The only code that belongs in them is stuff that never changes after the initial load event.

2. *OnParametersSet* and *OnParametersSetAsync* get called whenever the parent component makes changes to the parameter set for the component or a captured cascaded parameter changes.  Any code that needs to respond to parameter changes need to live here.

3. Component rendering (either through the markupcode or *BuildRenderTree*) happens after the *OnParametersSet* events either on initialization or a parameter change, and after an Event Callback occurs (such as responding to a mouse or keyboard event).

4. *OnAfterRender* and *OnAfterRenderAsync* occur at the end of all four events.  *firstRender* is only true on component initialization.  Any code that

5. *StateHasChanged* is called automatically after the Initialized events, after OnParametersSet events and after any event callback.  You don't need to call it separately.

