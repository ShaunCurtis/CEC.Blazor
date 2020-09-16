# Building a Database Appication in Blazor 
## Part 3 - CRUD Operations in the UI

## Introduction

Part 2 describes techniques and methodologies for abstracting the data and business logic layers into boilerplate code in a library.  This article does the same with the presentation layer.

### Sample Project and Code

See the [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) for the libraries and sample projects.

### The Base Forms

The CRUD UI is implemented as a set of boilerplate components inheriting from *OwningComponentBase*.  *OwningComponentBase* is used for control over the scope of Scoped Services.  Code is available on the Github site and linked at appropriate places in this article.

#### ApplicationComponentBase

[CEC.Blazor/Components/Base.ApplicationComponentBase.cs](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/ApplicationComponentBase.cs)

*ApplicationComponentBase* is the base component and contains all the common client application code:

  1. Injection of common services, such as Navigation Manager and Application Configuration.
  2. Authentication and user management.
  3. Navigation and Routing.

#### ControllerServiceComponent and Its Children

[CEC.Blazor/Components/Base.ControllerServiceComponentBase.cs](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/ControllerServiceComponentBase.cs)

[*ControllerServiceComponentBase*](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/ControllerServiceComponentBase.cs) is the base CRUD component.

There are three inherited classes for specific CRUD operations:
1. [*ListComponentBase*](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/ListComponentBase.cs) for all list pages
2. [*RecordComponentBase*]((https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/RecordComponentBase.cs)) for displaying individual records.
3. [*EditComponentBase*](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/Base/EditComponentBase.cs) for CUD [Create/Update/Delete] operations.

All common code resides in *ControllerServiceComponent*, specific code in the inherited class.

### Implementing CRUD Pages

We'll look at the editor in detail to see how the components are structured and edit functionality implemented.

#### The View

The routed view is a very simple component.  We separate the actual view component from the routed view.  It's used in other pages such as the modal dialog viewer.

```c#
// CEC.Blazor.WASM.Client/Routes/WeatherForecastEditorView.razor
@page "/WeatherForecast/New"
@page "/WeatherForecast/Edit"
@inherits ApplicationComponentBase
@namespace CEC.Blazor.WASM.Client.Routes

<WeatherEditorForm></WeatherEditorForm>
```

#### The Form

Again a relatively simple component programmatically. 

```C#
// CEC.Weather/Components/Forms/WeatherForecastEditorForm.razor
public partial class WeatherEditorForm : EditRecordComponentBase<DbWeatherForecast, WeatherForecastDbContext>
{
    [Inject]
    public WeatherForecastControllerService ControllerService { get; set; }

    private string CardCSS => this.IsModal ? "m-0" : "";

    protected async override Task OnInitializedAsync()
    {
        // Assign the correct controller service
        this.Service = this.ControllerService;
        await base.OnInitializedAsync();
    }
}
```

This gets and assigns the specific ControllerService through DI to the Service Property [IContollerService].

The Razor Markup below is an abbreviated version of the actual file.  This makes extensive use of UIControls which will be discussed in detail in the next article.  The comments provide explanation. 
```C#
// CEC.Weather/Components/Forms/WeatherForecastEditorForm.razor.cs
// UI Card is a Bootstrap Card
<UICard IsCollapsible="false">
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        // Cascades the Event Handler in the form for RecordChanged.  Picked up by each FormControl and fired when a value changes in the FormControl
        <CascadingValue Value="@this.RecordFieldChanged" Name="OnRecordChange" TValue="Action<bool>">
            // Error handler - only renders it's content when the record exists and is loaded
            <UIErrorHandler IsError="@this.IsError" IsLoading="this.IsDataLoading" ErrorMessage="@this.RecordErrorMessage">
                <UIContainer>
                    // Standard Blazor EditForm control
                    <EditForm EditContext="this.EditContext">
                        // Fluent ValidationValidator for the form
                        <FluentValidationValidator DisableAssemblyScanning="@true" />
                        .....
                        // Example data value row with label and edit control
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Record Date:
                            </UILabelColumn>
                            <UIColumn Columns="4">
                                // Note the Record Value bind to the record shadow copy to detect changes from the orginal stored value
                                <FormControlDate class="form-control" @bind-Value="this.Service.Record.Date" RecordValue="this.Service.ShadowRecord.Date"></FormControlDate>
                            </UIColumn>
                        </UIFormRow>
                        ..... // more form rows here
                    </EditForm>
                </UIContainer>
            </UIErrorHandler>
            // Container for the buttons - not record dependant so outside the error handler to allow navigation if UIErrorHandler is in error.
            <UIContainer>
                <UIRow>
                    <UIColumn Columns="7">
                        // Bootstrap alert to display any messages
                        <UIAlert Alert="this.AlertMessage" SizeCode="Bootstrap.SizeCode.sm"></UIAlert>
                    </UIColumn>
                    <UIButtonColumn Columns="5">
                        ....
                        // UIButton is a Bootstrap button.  Show controls whether it's displayed.
                        // For example Save is displayed when the Service Record is Dirty and the record has loaded. 
                        <UIButton Show="(!this.IsClean) && this.IsLoaded" ClickEvent="this.Save" ColourCode="Bootstrap.ColourCode.save">Save</UIButton>
                        <UIButton Show="this.ShowExitConfirmation && this.IsLoaded" ClickEvent="this.ConfirmExit" ColourCode="Bootstrap.ColourCode.danger_exit">Exit Without Saving</UIButton>
                        <UIButton Show="(!this.NavigationCancelled) && !this.ShowExitConfirmation" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToList))" ColourCode="Bootstrap.ColourCode.nav">Exit To List</UIButton>
                        <UIButton Show="(!this.NavigationCancelled) && !this.ShowExitConfirmation" ClickEvent="this.Exit" ColourCode="Bootstrap.ColourCode.nav">Exit</UIButton>
                    </UIButtonColumn>
                </UIRow>
            </UIContainer>
        </CascadingValue>
    </Body>
</UICard>
```
#### Base Form Code

##### OnInitializedAsync

The code block below shows the two OnInitializedAsync methods in the class hierarchy.

*OnInitializedAsync* is implemented from top down (local code is run before calling the base method).

```c#
// CEC.Weather/Components/Forms/WeatherEditorForm.razor.cs
protected async override Task OnInitializedAsync()
{
    // Assign the correct controller service
    this.Service = this.ControllerService;
    // Set the delay on the record load as this is a demo project
    this.DemoLoadDelay = 250;
    await base.OnInitializedAsync();
}

// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
protected async override Task OnInitializedAsync()
{
    // Resets the record to blank 
    await this.Service.ResetRecordAsync();
    await base.OnInitializedAsync();
}

// CEC.Blazor/Components/BaseForms/ApplicationComponentBase.cs
protected async override Task OnInitializedAsync()
{
    // Gets the user if we have an AuthenticationState
    if (this.AuthenticationState != null) await this.GetUserAsync();
    await base.OnInitializedAsync();
}
```

##### OnParametersSetAsync

*OnParametersSetAsync* is implemented from bottom up (the base method is called before any local code).

```C#
// CEC.Blazor/Components/BaseForms/ApplicationComponentBase.cs
protected async override Task OnParametersSetAsync()
{
    await base.OnParametersSetAsync();
    // Get the record if required - see below for method detail
    await this.LoadRecordAsync();
}
```

##### LoadRecordAsync

The record loading code is broken out of *OnParametersSetAsync* as it's used outside the component lifecycle methods.  It's implemented from bottom up (the base method is called before any local code).

```C#
// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
protected virtual async Task LoadRecordAsync()
{
    if (this.IsService)
    {
        // Set the Loading flag and call StateNasChanged to force UI changes 
        // in this case making the UIErrorHandler show the loading spinner 
        this.IsDataLoading = true;
        StateHasChanged();

        // Check if we have a query string value in the Route for ID.  If so use it
        if (this.NavManager.TryGetQueryString<int>("id", out int querystringid)) this.ID = querystringid > -1 ? querystringid : this._ID;

        // Check if the component is a modal.  If so get the supplied ID
        else if (this.IsModal && this.Parent.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

        // make this look slow to demo the spinner
        if (this.DemoLoadDelay > 0) await Task.Delay(this.DemoLoadDelay);

        // Get the current record - this will check if the id is different from the current record and only update if it's changed
        await this.Service.GetRecordAsync(this._ID, false);

        // Set the error message - it will only be displayed if we have an error
        this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

        // Set the Loading flag and call statehaschanged to force UI changes 
        // in this case making the UIErrorHandler show the record or the error message 
        this.IsDataLoading = false;
        StateHasChanged();
    }
}

// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
protected async override Task LoadRecordAsync()
{
    await base.LoadRecordAsync();

    //set up the Edit Context
    this.EditContext = new EditContext(this.Service.Record);

    // Get the actual page Url from the Navigation Manager
    this.RouteUrl = this.NavManager.Uri;
    // Set up this page as the active page in the router service
    this.RouterSessionService.ActiveComponent = this;
    // Wire up the router NavigationCancelled event
    this.RouterSessionService.NavigationCancelled += this.OnNavigationCancelled;
}
```

##### OnAfterRenderAsync

*OnAfterRenderAsync* is implemented from bottom up (base is called before any local code is executed).

```C#
// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
protected async override Task OnAfterRenderAsync(bool firstRender)
{
    await base.OnAfterRenderAsync(firstRender);
    // Wire up the SameComponentNavigation Event - i.e. we potentially have a new record to load in the same View 
    if (firstRender) this.RouterSessionService.SameComponentNavigation += this.OnSameRouteRouting;
}
```

##### Event Handlers

There are three event handlers wired up in the Component load events.

```c#
// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
// Event handler for a navigation cancelled event raised by the router
protected virtual void OnNavigationCancelled(object sender, EventArgs e)
{
    // Set the boolean properties
    this.NavigationCancelled = true;
    this.ShowExitConfirmation = true;
    // Set up the alert
    this.AlertMessage.SetAlert("<b>THIS RECORD ISN'T SAVED</b>. Either <i>Save</i> or <i>Exit Without Saving</i>.", Bootstrap.ColourCode.danger);
    // Trigger a component State update - buttons and alert need to be sorted
    InvokeAsync(this.StateHasChanged);
}
```
```c#
// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
// Event handler for the RecordFromControls FieldChanged Event
protected virtual void RecordFieldChanged(bool isdirty)
{
    if (this.EditContext != null)
    {
        // Sort the Service Edit State
        this.Service.SetClean(!isdirty);
        // Set the boolean properties
        this.ShowExitConfirmation = false;
        this.NavigationCancelled = false;
        // Sort the component state based on the edit state
        if (this.IsClean)
        {
            this.AlertMessage.ClearAlert();
            this.RouterSessionService.SetPageExitCheck(false);
        }
        else
        {
            this.AlertMessage.SetAlert("The Record isn't Saved", Bootstrap.ColourCode.warning);
            this.RouterSessionService.SetPageExitCheck(true);
        }
        // Trigger a component State update - buttons and alert need to be sorted
        InvokeAsync(this.StateHasChanged);
    }
}
```
```c#
// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
// Event handler for SameRoute event raised by the router.  Check if we need to load a new record
protected async void OnSameRouteRouting(object sender, EventArgs e)
{
    // Gets the record - checks for a new ID in the querystring and if we have one loads the records
    await LoadRecordAsync();
}
```

##### Action Button Events

There are four action events.

```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
/// Save Method called from the Button
protected virtual async Task<bool> Save()
{
    var ok = false;
    // Validate the EditContext
    if (this.EditContext.Validate())
    {
        // Save the Record
        ok = await this.Service.SaveRecordAsync();
        if (ok)
        {
            // Set the EditContext State
            this.EditContext.MarkAsUnmodified();
            // Set the boolean properties
            this.ShowExitConfirmation = false;
            // Sort the Router session state
            this.RouterSessionService.NavigationCancelledUrl = string.Empty;
        }
        // Set the alert message to the return result
        this.AlertMessage.SetAlert(this.Service.TaskResult);
        // Trigger a component State update - buttons and alert need to be sorted
        this.UpdateState();
    }
    else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", Bootstrap.ColourCode.danger);
    return ok;
}
```
```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
/// Save and Exit Method called from the Button
protected virtual async void SaveAndExit()
{
    if (await this.Save()) this.ConfirmExit();
}
```
```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
/// Exit Method called from the Button
protected virtual void Exit()
{
    // Check if we are free (we have a clean record) to exit or need confirmation
    if (this.IsClean) ConfirmExit();
    else this.ShowExitConfirmation = true;
}
```
```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
/// Confirm Exit Method called from the Button
protected virtual void ConfirmExit()
{
    // To escape a dirty component set IsClean manually and navigate.
    this.Service.SetClean();
    // Sort the Router session state
    this.RouterSessionService.NavigationCancelledUrl = string.Empty;
    //turn off page exit checking
    this.RouterSessionService.SetPageExitCheck(false);
    // Sort the exit strategy
    if (this.IsModal) ModalExit();
    else
    {
        // Check if we have a Url the user tried to navigate to - default exit to the root
        if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
        else if (!string.IsNullOrEmpty(this.RouterSessionService.ReturnRouteUrl)) this.NavManager.NavigateTo(this.RouterSessionService.ReturnRouteUrl);
        else this.NavManager.NavigateTo("/");
    }
}
```
```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
// Cancel Method called from the Button
protected void Cancel()
{
    // Set the boolean properties
    this.ShowExitConfirmation = false;
    this.NavigationCancelled = false;
    // Sort the Router session state
    this.RouterSessionService.NavigationCancelledUrl = string.Empty;
    // Sort the component state based on the edit state
    if (this.IsClean) this.AlertMessage.ClearAlert();
    else this.AlertMessage.SetAlert($"{this.Service.RecordConfiguration.RecordDescription} Changed", Bootstrap.ColourCode.warning);
    // Trigger a component State update - buttons and alert need to be sorted
    this.UpdateState();
}
```

##### Navigation Buttons

Various exit buttons are wired to and button handler call *NavigateTo*.

```C#
// CEC.Blazor/Components/BaseForms/ControllerServiceComponentBase.cs
protected virtual void NavigateTo(PageExitType exittype)
{
    this.NavigateTo(new EditorEventArgs(exittype));
}

protected override void NavigateTo(EditorEventArgs e)
{
    if (IsService)
    {
        //check if record name is populated and if not populate it
        if (string.IsNullOrEmpty(e.RecordName)) e.RecordName = this.Service.RecordConfiguration.RecordName;

        // check if the id is set for view or edit.  If not, sets it.
        if ((e.ExitType == PageExitType.ExitToEditor || e.ExitType == PageExitType.ExitToView) && e.ID == 0) e.ID = this._ID;
        base.NavigateTo(e);
    }
}
```
These propagate down to *NavigateTo* in *ApplicationComponentBase*
```c#
// CEC.Blazor/Components/BaseForms/ApplicationComponentBase.cs
// Structured approach to organising record CRUD routing
protected virtual void NavigateTo(EditorEventArgs e)
{
    switch (e.ExitType)
    {
        case PageExitType.ExitToList:
            this.NavManager.NavigateTo($"/{e.RecordName}/");
            break;
        case PageExitType.ExitToView:
            this.NavManager.NavigateTo($"/{e.RecordName}/View?id={e.ID}");
            break;
        case PageExitType.ExitToEditor:
            this.NavManager.NavigateTo($"/{e.RecordName}/Edit?id={e.ID}");
            break;
        case PageExitType.SwitchToEditor:
            this.NavManager.NavigateTo($"/{e.RecordName}/Edit?id={e.ID}");
            break;
        case PageExitType.ExitToNew:
            this.NavManager.NavigateTo($"/{e.RecordName}/New?qid={e.ID}");
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

### Wrap Up
That wraps up this section.  We've looked at the Edit process in detail to see how the code works.  The next section looks in detail at UI Controls seen in the razor markup in this article.

Some key points to note:
1. The differences in code between a Blazor Server and a Blazor WASM project are very minor.
2. Most of the code resides in generic boilerplate classes.
