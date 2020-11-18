# An Alternative to Routing in Blazor

SPA's and routing seem intrinsically joined at the hip.  This article looks at an alternative approach: no router or URLs in sight.

SPA's are applications, not web sites.  I think we sometimes forget and let the web paradigm constrain our thinking.  A standard desktop application doesn't use URLs to move around the application, open new forms, edit information,...  So why do it in a SPA?  Is there an alternative?

Step forward the *ViewManager*.

You can see a routerless version of my take on the standard Blazor Weather Application in action at these sites:

[WASM Weather Version](https://cec-blazor-wasm.azurewebsites.net/)

[Blazor Server Version](https://cec-blazor-server.azurewebsites.net/)

The GitHub repository is [here](https://github.com/ShaunCurtis/CEC.Blazor/tree/Experimental).  Note:
* You need to be on the Experimental Branch.
* *ViewManager* is not standalone code.  It's part of the CEC.Blazor project.


## Terminology

To be clear about the terms I use in this article:

1. **Page** - used sparsely.  A page is a classic HTML web page served up by a web server.  The only page in Blazor is either _Host.cshtml or index.html.
2. **View** - a View is what gets loaded by the Router.  Combined with a Layout it's what gets displayed in the browser window.
3. **Layout** - is the Layout component specified by the View.  If a Layout isn't specified in the View, the default Layout is used.
4. **Form** - is a "logical unit of code and markup" that contains a set of Control components to display information.   Editors/Viewers/Lists are classic forms.  A View contains one or more Forms. Forms can be displayed in a View or inside a modal dialog.
5. **Control** - is a component that displays something. Buttons, edit boxes, switches and dropdown lists are all controls.

## App.razor

The Blazor Application runs inside the *\<app\>\</app\>* HTML tags in the base web page.

In a routed Blazor application the *Router* component is next in the RenderTree after *App*.  URL navigation is managed by the *NavigationManager*. The router registers with the *NavigationManager* - *OnLocationChanged* event and reacts to navigation events.  It works out which component needs loading and loads it into the *Layout* component.  The standard issue *App.razor* looks like this:

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
And the new one looks like this:

```html
<ViewManager DefaultViewData="viewData" ModalType="typeof(CEC.Blazor.Components.UIControls.BootstrapModal)" DefaultLayout="typeof(BaseLayout)">
</ViewManager>

@code {
    public ViewData viewData = new ViewData(typeof(CEC.Weather.Components.Views.Index), null);
}
```

The router has been replaced with the *ViewManager* component.  This becomes the sub-root component of the RenderTree.

1. *DefaultViewData* is a configuration object that describes the default View for the application - the "Home" View that gets loaded at startup.  All Views must implement *IView*.  The *ViewData* object passed to the control is constructed in the code as a Property.
2. *ModalType* - specified as a *Type* - is the modal dialog component *ViewManager* uses to display forms in modal dialog format.  It must implement *IModal*.  More about this later.
3. *DefaultLayout* - specified as a *Type* - is the default layout to use for Views.  This is a standard Blazor Layout.

## ViewData

The *ViewData* class holds the data required to render a View.

```cs
public sealed class ViewData
{
    /// The type of the View.
    public Type PageType { get; private set; }

    /// Parameter values to add to the View when created
    public Dictionary<string, object> ViewParameters { get; private set; } = new Dictionary<string, object>();

    /// View values that can be used by the view and subcomponents
    public Dictionary<string, object> ViewFields { get; private set; } = new Dictionary<string, object>();

    /// Constructs an instance of <see cref="ViewData"/>.
    public ViewData(Type pageType, Dictionary<string, object> viewValues)
    {
        if (pageType == null) throw new ArgumentNullException(nameof(pageType));
        if (!typeof(IView).IsAssignableFrom(pageType)) throw new ArgumentException($"The view must implement {nameof(IView)}.", nameof(pageType));
        this.PageType = pageType;
        this.ViewParameters = viewValues;
    }
    .....
    // various methods to update and get Parameters and Fields from ViewParameters and ViewFields
}
```

## ViewManager

The *ViewManager* implements *IComponent*.  The first section gets the NavigationManager and JSRuntime Interop through DI and declares the three parameter based properties.

1. We use *NavigationManager* to get the initial URL and extract any QueryString data.
2. We use *IJSRuntime* to manipulate the browser *beforeunload*. More later.

```cs
public class ViewManager : IComponent
{
    [Inject] private NavigationManager NavManager { get; set; }

    [Inject] private IJSRuntime _js { get; set; }

    [Parameter] public ViewData DefaultViewData { get; set; }

    [Parameter] public Type ModalType { get; set; } = typeof(BootstrapModal);

    [Parameter] public Type DefaultLayout { get; set; }

    public IModal ModalDialog { get; protected set; }

```

### ViewData

We manage the ViewData through the *ViewData* property.

```cs
public ViewData ViewData
{
    get
    {
        if (this._ViewData == null) this._ViewData = this.DefaultViewData;
        return this._ViewData;
    }
    protected set
    {
        this.AddViewToHistory(this._ViewData);
        this._ViewData = value;
    }
}

private ViewData _ViewData { get; set; }
```

We keep the View History in *ViewHistory* - it's size is controlled by *ViewHistorySize*.

```cs
public SortedList<DateTime, ViewData> ViewHistory { get; private set; } = new SortedList<DateTime, ViewData>();


public ViewData LastViewData
{
    get
    {
        var newest = ViewHistory.Max(item => item.Key);
        if (newest != null) return ViewHistory[newest];
        else return null;
    }
}
```

New Views are added through *AddViewToHistory* which also manages the list size.

```cs
[Parameter] public int ViewHistorySize { get; set; } = 10;

private void AddViewToHistory(ViewData value)
{
    while (this.ViewHistory.Count >= this.ViewHistorySize)
    {
        var oldest = ViewHistory.Min(item => item.Key);
        this.ViewHistory.Remove(oldest);
    }
    this.ViewHistory.Add(DateTime.Now, value);
}
```

### Initialization and Event Driven Methods

There are two *InvokeAsync* helper methods to run Actions and Functions on the Renderer's Dispatcher i.e. the UI context.

```cs
protected Task InvokeAsync(Action workItem) => _renderHandle.Dispatcher.InvokeAsync(workItem);

protected Task InvokeAsync(Func<Task> workItem) => _renderHandle.Dispatcher.InvokeAsync(workItem);
```

Class initialization builds the component render fragment.  This is passed to the renderer whenever the component needs rendering.

```cs
private readonly RenderFragment _componentRenderFragment;

private bool _RenderEventQueued;

private RenderHandle _renderHandle;

public ViewManager() => _componentRenderFragment = builder =>
{
    this._RenderEventQueued = false;
    BuildRenderTree(builder);
};
```
*Attach* saves the RenderHandle - it gets called by the Renderer when the component is attached to the RenderTree.

```cs
public void Attach(RenderHandle renderHandle)
{
    _renderHandle = renderHandle;
}
```

*SetParametersAsync* is fairly simple. It only gets called on startup - being a root component there's nothing to update it's parameters.  It checks for querystring data and loads any found into ViewData.

```cs
public Task SetParametersAsync(ParameterView parameters)
{
    parameters.SetParameterProperties(this);
    this._ViewData = this.DefaultViewData;
    /// Get any query string data and load it into ViewData.
    this.ReadViewDataFromQueryString();
    this.Render();
    return Task.CompletedTask;
}
```
The View is updated by calling *LoadViewAsync* on the cascaded instance of *ViewManager*.  There are various versions of *LoadViewAsync*, constructing the *ViewData* in various ways.  They all call the core method shown below.

The method updates the *ViewData* property and then calls *Render* to queue a re-render of the component.

```cs
public Task LoadViewAsync(ViewData viewData = null)
{
    // can be locked by a component if it has a dirty dataset
    if (!this.IsLocked)
    {
        if (viewData != null) this.ViewData = viewData;
        if (ViewData == null)
        {
            throw new InvalidOperationException($"The {nameof(ViewManager)} component requires a non-null value for the parameter {nameof(ViewData)}.");
        }
        this.Render();
    }
    return Task.CompletedTask;
}
```

The Render method uses *InvokeAsync* to make sure the code runs on the Renderer's synchronisation context.

```cs
private void Render() => InvokeAsync(() =>
{
    if (!this._RenderEventQueued)
    {
        this._RenderEventQueued = true;
        _renderHandle.Render(_componentRenderFragment);
    }
}
);
```
It queues the component render fragment on the Renderer's render queue.  Note *_RenderEventQueued* is set to true when a render event is queued, and set to false within the render fragment when the render fragment is actually run.

### RenderTree Builder

The RenderTree Builder process wraps all the child content in a cascading value of the *ViewManager* instance - all sub components have access the ViewManager.  The *ChildContent* is the Layout either defined by the View or the default Layout.

```cs
/// Renders the component.
protected virtual void BuildRenderTree(RenderTreeBuilder builder)
{
    // Adds cascadingvalue for the ViewManager
    builder.OpenComponent<CascadingValue<ViewManager>>(0);
    builder.AddAttribute(1, "Value", this);
    // Get the layout render fragment
    builder.AddAttribute(2, "ChildContent", this._layoutViewFragment);
    builder.CloseComponent();
}
```

The Layout fragment - the child content of the *ViewManager* - consists of the Modal Dialog component and the Layout.  The View is the *ChildComponent* of the Layout.

```cs
private RenderFragment _layoutViewFragment =>
    builder =>
    {
        // Gets the Layout to use
        var pageLayoutType = ViewData?.PageType?.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;
        // Adds the Modal Dialog infrastructure
        builder.OpenComponent(0, ModalType);
        builder.AddComponentReferenceCapture(1, modal => this.ModalDialog = (IModal)modal);
        builder.CloseComponent();
        // Adds the Layout component
        if (pageLayoutType != null)
        {
            builder.OpenComponent<LayoutView>(2);
            builder.AddAttribute(3, nameof(LayoutView.Layout), pageLayoutType);
            // Adds the view render fragment into the layout component
            if (this._ViewData != null)
                builder.AddAttribute(4, nameof(LayoutView.ChildContent), this._viewFragment);
            else
            {
                builder.AddContent(2, this._fallbackFragment);
            }
            builder.CloseComponent();
        }
        else
        {
            builder.AddContent(0, this._fallbackFragment);
        }
    };
```
*_viewFragment* reads *ViewData* and loads the View. 

```cs
/// Render fragment that renders the View
private RenderFragment _viewFragment =>
    builder =>
    {
        try
        {
            // Adds the defined view with any defined parameters
            builder.OpenComponent(0, _ViewData.PageType);
            if (this._ViewData.ViewParameters != null)
            {
                foreach (var kvp in _ViewData.ViewParameters)
                {
                    builder.AddAttribute(1, kvp.Key, kvp.Value);
                }
            }
            builder.CloseComponent();
        }
        catch
        {
            // If the pagetype causes an error - load the fallback
            builder.AddContent(0, this._fallbackFragment);
        }
    };
```

The final method defines a fallback fragment to display if there's problems with the Layout or View.

```cs
/// Fallback render fragment if there's no Layout or View specified
private RenderFragment _fallbackFragment =>
    builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, "This is the ViewManager's fallback View.  You have no View and/or Layout specified.");
        builder.CloseElement();
    };
```
### Locking the View

The View can be locked - useful when there's unsaved data - so the user:
1. Within the application, can't leave the View.
2. Gets the "Do You Really Want To Leave This Page?" browser popup if they try to navigate elsewhere or reload the application.
  
```cs
public bool IsLocked { get; private set; }

public void LockView()
{
    this.IsLocked = true;
    this.SetPageExitCheck(true);
}

public void UnLockView()
{
    this.IsLocked = false;
    this.SetPageExitCheck(false);
}
```

*SetPageExitCheck* adds or removes a listener on the *beforeunload* browser event through JSInterop.

```cs
public void SetPageExitCheck(bool action)
{
    if (action != ExitState) _js.InvokeAsync<bool>("cecblazor_setEditorExitCheck", action);
    ExitState = action;
}
```
The JS functions are:
 
```js
// in CEC.Blazor/wwwroot/site.js
window.cecblazor_setEditorExitCheck = function (show) {
    if (show) {
        window.addEventListener("beforeunload", cecblazor_showExitDialog);
    }
    else {
        window.removeEventListener("beforeunload", cecblazor_showExitDialog);
    }
}

window.cecblazor_showExitDialog = function (event) {
    event.preventDefault();
    event.returnValue = "There are unsaved changes on this page.  Do you want to leave?";
}
```

You will need to add a reference in either *_host.cshtml* or *index.html*.

```js
<script src="_content/CEC.Blazor/site.js"></script>
```

### URL Navigation to a specific View on Startup

Once nice thing about Routing is the ability to point at user straight at a specific View.  You can do this with *ViewManager* with querystring data.

The link shows it in action [Load Weather Report ID 5 - https://cec-blazor-wasm.azurewebsites.net/?View=CEC.Weather.Components.Views.WeatherForecastViewerView&Param-ID=5](https://cec-blazor-wasm.azurewebsites.net/?View=CEC.Weather.Components.Views.WeatherForecastViewerView&Param-ID=5).

The *ReadViewDataFromQueryString* method reads any querystring data into the initial ViewData object.  You define the View, Parameter data and View specific data in the querystring.

1. *View=* defines the fully qualified classname.
2. *Param-ID=1* adds a Parameter *ID* to the Parameter List with a value of 1.
3. *Field-DataValue=5.01* adds a *DataValue* to the Field List with a value of 5.01. 
4. Only four data types are recognised - DateTime, Int, Decimal and String.  If you want more add the code!

```cs
private void ReadViewDataFromQueryString()
{
    ViewData viewData = null;
    var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
    var vals = QueryHelpers.ParseQuery(uri.Query);
    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("View", out var classname))
    {
        var type = this.FindType(classname);
        if (type != null)
        {
            viewData = new ViewData(type, null);
            foreach (var set in vals)
            {
                if (set.Key.StartsWith("Param-"))
                {
                    object value;
                    if (DateTime.TryParse(set.Value, out DateTime datevalue)) value = datevalue;
                    else if (Int32.TryParse(set.Value, out int intvalue)) value = intvalue;
                    else if (Decimal.TryParse(set.Value, out decimal decvalue)) value = decvalue;
                    else value = set.Value;
                    viewData.SetParameter(set.Key.Replace("Param-", ""), value);
                }
                if (set.Key.StartsWith("Field-"))
                {
                    object value;
                    if (DateTime.TryParse(set.Value, out DateTime datevalue)) value = datevalue;
                    else if (Int32.TryParse(set.Value, out int intvalue)) value = intvalue;
                    else if (Decimal.TryParse(set.Value, out decimal decvalue)) value = decvalue;
                    else value = set.Value;
                    viewData.SetField(set.Key.Replace("Field-", ""), value);
                }
            }
        }
    }
    if (viewData != null) this.ViewData = viewData;
}
```
### Modal Dialogs

The final bit of *ViewManager* is *ShowModalAsync* which opens any Form - defined by implementing IForm - in the modal dialog.  The code for the modal dialog can be found in the project.  There'll be another article shortly covering this in more detail.

```cs
/// Method to open a Modal Dialog
public async Task<ModalResult> ShowModalAsync<TForm>(ModalOptions modalOptions) where TForm : IComponent => await this.ModalDialog.Show<TForm>(modalOptions);
```

## UIViewLink
*UIViewLink* is similar to *NavLink*.  It constructs a clickable link to navigate to a defined View.  The important bit is *this.ViewManager.LoadViewAsync(viewData)*.

```cs
    public class UIViewLink : UIBase
    {
        /// View Type to Load
        [Parameter] public Type ViewType { get; set; }

        /// View Paremeters for the View
        [Parameter] public Dictionary<string, object> ViewParameters { get; set; } = new Dictionary<string, object>();

        /// Cascaded ViewManager
        [CascadingParameter] public ViewManager ViewManager { get; set; }

        /// Boolean to check if the ViewType is the current loaded view
        /// if so it's used to mark this component's CSS with "active" 
        private bool IsActive => this.ViewManager.IsCurrentView(this.ViewType);

        /// Builds the render tree for the component
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var css = string.Empty;
            // builds the ViewData object
            var viewData = new ViewData(ViewType, ViewParameters);

            // Gets any css set on the control
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var obj))
            {
                css = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            // checks if this is the active View and if so sets the active CSS attribute
            if (this.IsActive) css = $"{css} active";
            // Add onclick to the Attributes to remove from the Captured Attributes
            this.UsedAttributes.Add("@onclick");
            this.UsedAttributes.Add("onclick");
            // and clears the attributes
            this.ClearDuplicateAttributes();
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", css);
            // adds the user supplied attributes
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            // Adds the onclick event to load the supplied View
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, e => this.ViewManager.LoadViewAsync(viewData)));
            // Adds the child content
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }
```

## Using the ViewManager

The project in the Github Repo uses the ViewManager to replace routing.  If you're interested in *ViewManager*, I suggest you grab the project and dig into the code.

The code below shows *UIViewLink* used in the left nav control.
### LeftNavMenu

```html
....
    <li class="nav-item px-3">
        <UIViewLink class="nav-link" ViewType="typeof(WeatherForecastListView)">
            <span class="oi oi-cloudy" aria-hidden="true"></span> Normal Weather
        </UIViewLink>
    </li>
    <li class="nav-item px-3">
        <UIViewLink class="nav-link" ViewType="typeof(WeatherForecastListModalView)">
            <span class="oi oi-cloud-upload" aria-hidden="true"></span> Modal Weather
        </UIViewLink>
    </li>
.....
```
```cs
 @code {
    [CascadingParameter] public ViewManager ViewManager { get; set; }
}
```
## Conclusions

Hopefully I've opened your eyes a little.  A Router isn't compulsory.  You can code an SPA just like an old style desktop application - no router, no URLs.  

Commments and suggestions gratefuly received.
