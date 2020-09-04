# Boilerplating a Database Appication in Blazor 
## Part 2 - CRUD Operations in the UI

Part 1 describes methodologies for boilerplating the data and business logic layers of a Blazor Application.  This article demonstrates methodologies for boilerplating the presentation layer.

The article uses a demonstration solution, with code split between a library where (almost) all the reusable classes reside, and the project where project specific code resides.  It relies heavily on inheritance.  The developement process and code refactoring should always seek to migrate code downwards through the inheritance tree, and from specific (project) code to generic (library) code.  While this may seem like stating the obvious - yes "we" always do it - it's amazing how often what we preach is not what we implement.  I'm sure people will point out code in the demo project where I'm as guilty as everyone else. 

### Sample Project and Code

Here

### The WeatherForecast Application

The CRUD UI is implemented as a set of boilerplate components inheriting from *OwningComponentBase*.  We use *OwningComponentBase* to give us control on the scope of Scoped Services.

#### ApplicationComponentBase

  *ApplicationComponentBase* is the base component and contains all the common client application code:

  1. Injection of common services, such as Navigation Manager and Application Configuration.
  2. Authentication and user management.
  3. Navigation and Routing.

Everything shared that can be migrated down to this point in the class hierarchy goes here.

#### ControllerServiceComponent and Its Children

*ControllerServiceComponent* is the base CRUD component.
There are three inherited classes for specific CRUD operations:
1. *ListComponentBase* for all list pages
2. *RecordComponentBase* for displaying individual records.
3. *EditComponentBase* for CUD [Create/Update/Delete] operations.

All common code resides in *ControllerServiceComponent*, specific code in the inherited class.

### Implementing CRUD Pages

#### Viewing a Record

This is the simplest, so we'll start here.  The routed component is simple.  The Weather Forecast Viewer looks like this:

```html
@page "/WeatherForecast/View"

@namespace CEC.Blazor.Server.Pages

@inherits ApplicationComponentBase

<WeatherViewer></WeatherViewer>
```

*WeatherViewer* contains only code specific to displaying the WeatherForecast record:

```C#
public partial class WeatherViewer : RecordComponentBase<DbWeatherForecast>
{
    [Inject]
    private WeatherForecastControllerService ControllerService { get; set; }

    public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

    protected async override Task OnInitializedAsync()
    {
        this.Service = this.ControllerService;
        await base.OnInitializedAsync();
    }

    protected void NextRecord(int increment) => this.NavManager.NavigateTo($"/WeatherForecast/View?id={this._ID + increment}");

}
```

This:
1. Gets the specific ControllerService through DI and assigning it to the Service [IService].
2. Sets the Page Title.
3. Has an event handler to handle navigating between records.  This is purely for demo purposes - responding to Intra Page Routing.

#### A detailed look at the Events and Initialization Methods

##### OnInitializedAsync

*OnInitializeAsync* is implemented from top down (code is executed from the top of inheritance hierarchy down).

WeatherViewer (code above) runs *OnInitializedAsync*.  This sets up the Service (IControllerService) and then calls down the hierarchy.

*RecordComponentBase* resets the record and then calls down the hierarchy.

```C#
protected async override Task OnInitializedAsync()
{
    await this.Service.ResetRecordAsync();
    await base.OnInitializedAsync();
}
```

*ApplicationComponentBase* gets the user and then calls down into the *OwningComponentBase*.

```C#
protected async override Task OnInitializedAsync()
{
    if (this.AuthenticationState != null) await this.GetUserAsync();
    await base.OnInitializedAsync();
}
```

##### OnParametersSetAsync

*OnParametersSetAsync* is implemented from bottom up (code is executed from the bottom of inheritance hierarchy up).

*RecordComponentBase* calls down the hierarchy (in this case into *OwningComponentBase*) and then loads the record.

```C#
protected async override Task OnParametersSetAsync()
{
    await base.OnParametersSetAsync();
    // Get the record if required
    await this.LoadRecordAsync();
}
```
*LoadRecordAsync* gets the record from the database through the *IDataControllerService* (which gets it through it's *IDataService*).  The commenting explains the steps.

```C#
protected virtual async Task LoadRecordAsync()
{
    if (this.IsService)
    {
        // Set the Loading flag and call statehaschanged to force UI changes 
        // in this case making the UIErrorHandler show the loading spinner 
        this.IsDataLoading = true;
        StateHasChanged();

        // Check if we have a query string value in the Route for ID.  If so use it
        if (this.NavManager.TryGetQueryString<int>("id", out int querystringid)) this.ID = querystringid > 0 ? querystringid : this._ID;

        // Check if the component is a modal.  If so get the supplied ID
        else if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > 0 ? (int)modalid : this.ID;

        // make this look async by adding a load delay
        await Task.Delay(500);

        // Get the current record - this will check if the id is different from the current record and only update if it's changed
        await this.Service.GetRecordAsync(this._ID, false);

        // Set the error message - it will only be displayed if we have an error
        this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

        // Set the Loading flag and call statehaschanged to force UI changes 
        // in this case making the UIErrorHandler show the record or the erro message 
        this.IsDataLoading = false;
        StateHasChanged();
    }
}
```

##### OnAfterRenderAsync

*OnAfterRenderAsync* is implemented from bottom up (code is executed from the bottom of inheritance hierarchy up).

*RecordComponentBase* calls down the hierarchy (in this case into *OwningComponentBase*) and then wires up the RouterSessionService *SameComponentNavigation*.  This is for demo purposes to show record last/previous functionality in the record Viewer.

```C#
protected async override Task OnAfterRenderAsync(bool firstRender)
{
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
        this.RouterSessionService.SameComponentNavigation += this.OnSameRouteRouting;
    }
}
```

##### Event Handling

The only events that ned handling are button events:

1. Previous and Next Buttons which call *NextRecord* in *WeatherViewer*.  This navigates to the same route with the new ID in the query string.  The router translates this into *SameComponentNavigation* event which is wired to *OnSameRouteRouting* in *OnAfterRenderAsync* in *RecordComponentBase*, and triggers *LoadRecordAsync*.  See above.

```C#
// WeatherViewer Code
protected void NextRecord(int increment) 
{
    var rec = (this._ID + increment) == 0 ? 1 : this._ID + increment;
    rec = rec > this.Service.BaseRecordCount ? this.Service.BaseRecordCount : rec;
    this.NavManager.NavigateTo($"/WeatherForecast/View?id={rec}");
}
```
```C#
// RecordComponentBase Code
protected async void OnSameRouteRouting(object sender, EventArgs e)
{
    // Gets the record - checks for a new ID in the querystring and if we have one loads the records
    await LoadRecordAsync();
}
```

2. Various Exit Actions which call NavigateTo in *ControllerServiceComponentBase*. 

```C#
protected virtual void NavigateTo(PageExitType exittype)
{
    this.NavigateTo(new EditorEventArgs(exittype));
}

protected override void NavigateTo(EditorEventArgs e)
{
    if (IsService)
    {
        //check if record name is populated and if not populates it
        if (string.IsNullOrEmpty(e.RecordName) && e.ExitType == PageExitType.ExitToList) e.RecordName = this.Service.RecordConfiguration.RecordListName;
        else if (string.IsNullOrEmpty(e.RecordName)) e.RecordName = this.Service.RecordConfiguration.RecordName;

        // check if the id is set for view or edit.  If not, sets it.
        if ((e.ExitType == PageExitType.ExitToEditor || e.ExitType == PageExitType.ExitToView) && e.ID == 0) e.ID = this._ID;
        base.NavigateTo(e);
    }
}
```

```c#
// ApplicationComponentBase Code
// based on structured approach to organising record CRUD routes
// and RecordConfiguration class informstion for each record type
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
```






