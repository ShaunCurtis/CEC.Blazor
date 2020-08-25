# Building a Blazor Project
# Part 4 - Boilerplating CRUD Operations in the UI

### The WeatherForecast Application

I build the CRUD UI in a set of boilerplate components inheriting from *OwningComponentBase*.

#### ApplicationComponentBase

  *ApplicationComponentBase* is the base component and contains all the common client application code:

  1. Injection of common services, such as Navigation Manager and Application Configuration.
  2. Authenication and user management.
  3. Navigation and Routing

Everything that is shared and can be migrated down to this point in the class hierarchy goes here.

#### ControllerServiceComponent and Its Children

*ControllerServiceComponent* is the base CRUD component.
There are three inherited classes for specific CRUD operations:
1. *ListComponentBase* for all list pages
2. *RecordComponentBase* for displaying individual records.
3. *EditComponentBase* for CUD [Create/Update/Delete] operations.

Common code is in ControllerServiceComponent, specific code in the inherited class.

### Implementing CRUD Pages

#### Viewing a Record

This is the simplest, so we'll start here.  I keep the routed Component as simple as possible.  The Weather Forecast Viewer looks like this:

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
We:
1. Get the specific ControllerService through DI and assigning it to the Service [IService].
2. Set the Page Title.
3. An event handler to handle navigating between records.  A demonstration of responding to Intra Page Routing.

*Base.OnInitializedAsync* calls down into *RecordComponentBase*.  This sets IsDataLoading boolean flag (used by the UI to display "On Loading"), resets the record in the Service, and hands on initializaton down the component tree.

```C#
protected async override Task OnInitializedAsync()
{
    this.IsDataLoading = true;
    await this.Service.ResetRecordAsync();
    await base.OnInitializedAsync();
}
```
Which ends up at *OnInitializedAsync* in *ApplicationComponentBase*.  This gets the user, sets the return url for the page exit buttons and calls *OnInitializedAsync* on *OwningComponentBase*.

```C#
protected async override Task OnInitializedAsync()
{
    if (this.AuthenticationState != null) await this.GetUserAsync();
    this.ReturnPageUrl = this.RouterSessionService.LastPageUrl;
    await base.OnInitializedAsync();
}
```
Next we move to *OnParametersSetAsync* in *RecordComponentBase*.  This calls *OnParametersSetAsync* down the component tree and then loads the record (if a load is required i.e. the ID has changed):

```C#
protected async override Task OnParametersSetAsync()
{
    await base.OnParametersSetAsync();
    // Get the record if required
    await this.LoadRecord();
}
```

Finally we move on to *OnAfterRender* in *RecordComponentBase*.  This wires up the Event handlers and sets up the error message if an error has occured loading the record.

```C#
protected async override Task OnAfterRenderAsync(bool firstRender)
{
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
        this.RouterSessionService.IntraPageNavigation += this.OnIntraPageRouting;
    }
    if (this.IsError)
    {
        this.RecordErrorMessage = "The Application can't load the Record";
        this.UpdateState();
    }
}
```

And *OnAfterRender* in *ApplicationComponentBase*.

```C#
protected override void OnAfterRender(bool firstRender)
{
    this.FirstLoad = false;
    base.OnAfterRender(firstRender);
}
```

