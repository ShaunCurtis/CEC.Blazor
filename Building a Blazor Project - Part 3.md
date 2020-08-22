# Building a Blazor Project
# Part 3 - The UI

In the first two articles I looked at project structure and implementing the data layers for a project.  In this article, I'll look at how I implement the Presentation Layer.  I use Bootstrap for my UI design, so I'm splitting this section into two articles.  This article covers building the data components.  The second article covers a structured approach to building the actual HTML components with Bootstrap.

### The Component

The Blazor UI is based on the component implemented as the base class *ComponentBase*.  Everything inherits from this class.  Add a *.razor* file and by default it inherits from *ComponentBase*.

It's hard to get away from, but you need to forget the classic concept of a page in Razor.  The only real page is  *_host.chtml* which is loaded when a Blazor SPA [Single Page Application] starts.  After that, it's all routing, no browser navigation.  Components get loaded into and out of *_host.chtml* as the user gets routed around the application.  Everything beyond the initial page load is DOM manipulation.

Think of two types of component:
1. Components
2. Routed Components

The only difference is Routed Components contain *@page* routing directives and can contain a *@Layout* directive.

```html
@page "/WeatherForecast"
@page "/WeatherForecasts"

@layout MainLayout
```

Routed Components are loaded into the *app* slot in the *_host.chtml* using either the specified Layout or the default Layout defined in *App.razor*.

```html
<body>
    <app>
        <component type="typeof(App)" render-mode="ServerPrerendered" />
    </app>
    .....
</body>
```
Note here the *render-mode*, which in a Blazor Server App is normally set to *ServerPrerendered*.  We'll look at the implications of this later in the Page lifecycle section.

Normal components load where they are declared.

```html
<WeatherList UIOptions="this.UIOptions" ></WeatherList>
```

There are some important aspects to components that need to understood, if you are not going to get confused.

Components have independant lifecycles, and their lifecycles aren't linked.  There's no default overarching co-ordination between the base routed component and other components in the same routed component, or between components.  The first time programmers normally get hit by this is in UI refresh.  Something gets updated on the routed component page, and one of the sub components that should update because the change also affects it doesn't.    You need to tell a sub-component to do a UI update.

### The WeatherForecast Application

I build most of the UI CRUD opertions and formatting into a set of components inheriting from *OwningComponentBase*.

#### ApplicationComponentBase

  *ApplicationComponentBase* is my base component and contains all the common code:

  1. Injection of common services, such as Navigation Manager and Application Configuration.
  2. Authenication and user management.
  3. Navigation and Routing

Anything that is shared and can be migrated to this point in the class hierarchy goes here.

#### ControllerServiceComponent and It's Inherited Classe

*ControllerServiceComponent* is the base CRUD component.
There are three inherited classes for specific CRUD operations:
1. *ListComponentBase* for all list pages
2. *RecordComponentBase* for displaying individual records.
3. *EditComponentBase* for CUD [Create/Update/Delete] operations.

Common code is in ControllerServiceComponent, specific code in the inherited.

### Implementing CRUD Pages

#### Record Diplay

This is the easiest.  The routed Component setts up the page routing and the *WeatherViewer* component.

```html
@page "/WeatherForecast/View"

@namespace CEC.Blazor.Server.Pages

@inherits ApplicationComponentBase

<WeatherViewer></WeatherViewer>
```

*WeatherViewer* contains all the code specific to displaying the WeatherForecast record.

```C#
public partial class WeatherViewer : RecordComponentBase<DbWeatherForecast>
{
    [Inject]
    private WeatherForecastControllerService ControllerService { get; set; }

    public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

    protected async override Task OnInitializedAsync()
    {
        this.Service = this.ControllerService;
        if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object id)) this.ID = (int)id > 0 ? (int)id : this.ID;
        await base.OnInitializedAsync();
    }
}
```
Here we are:
1. Getting the specific ControllerService through DI and assigning it to the Service [IService].
2. Checking if the component is a modal dialog, and if so getting the supplied ID.
3. Setting the Page Title.

Everything else is handled by the call to .*OnInitializedAsync* in *RecordComponentBase*.  This simply sets IsLoading boolean flag (used by the UI to display "On Loading"), and hands on initializaton up the component tree.

```C#
protected async override Task OnInitializedAsync()
{
    this.IsLoading = true;
    await base.OnInitializedAsync();
}
```
Which ends up at *OnInitializedAsync* in *ApplicationComponentBase*.  This calls LoadAsync, followed by *OnInitializedAsync* on *ComponentBase*, and finally (after everything else in the chain is complete) Calls *UpdateState* which updates the UI.

```C#
protected async override Task OnInitializedAsync()
{
    await this.LoadAsync();
    await base.OnInitializedAsync();
    // kick off a manual UI update
    this.UpdateState();
}
```
Next we move to LoadAsync.  The base LoadAsync from *ApplicationComponent* is called first by *RecordComponentBase* so we show it first.  It gets the current user and sets the return page for a "backward" navigation event - such as clicking exit in the viewer.

```C#
protected async virtual Task LoadAsync()
{
    if (this.AuthenticationState != null) await this.GetUserAsync();
    this.ReturnPageUrl = this.RouterSessionService.LastPageUrl;
}
```

The primary work is done in *RecordComponentBase*.  It resets the record in the service, checks if there is a record ID specified in the routing query string, and get the record baased on the *CurrentID*.  It then wires the *UpdateState* event handler to the RecordHasChanged event on the service, the *OnIntraPageNavigation* event handler to Intrapage Routing on the Router. Finally it sets IsLoadigh to false.

```C#
protected async override Task LoadAsync()
{
    await base.LoadAsync();
    if (this.IsService)
    {
        await this.Service.ResetRecordAsync();
        // Check if we have a query string value and use it if we do
        this.NavManager.TryGetQueryString<int>("id", out int id);
        this.ID = id > 0 ? id : this._ID;
        this.CurrentID = this._ID;
        // Get the current ID record
        await this.Service.GetRecordAsync(this._ID);
        this.Service.RecordHasChanged += this.UpdateState;
        this.RouterSessionService.IntraPageNavigation += this.OnIntraPageNavigation;
        this.IsLoading = false;
    }
}
```

