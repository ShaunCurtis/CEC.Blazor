# Part 3 - View Components - CRUD Edit and View Operations in the UI

## Introduction

This is the third in a series of articles looking at how to build and structure a real Database Application in Blazor. The articles so far are:

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

This article looks in detail at building reusable CRUD presentation layer components, specifically Edit and View functionality - and using them in Server and WASM projects.

## Sample Project and Code

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server version of the project running here](https://cec-blazor-server.azurewebsites.net/).

[You can see the WASM version of the project running here](https://cec-blazor-wasm.azurewebsites.net/).


## The Base Forms

All CRUD UI components inherit from `Component` - see the following article about [Component](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).  Not all the code is shown in the article: some class are simply too big to show everything.  All source files can be viewed on the Github site, and I include references or links to specific code files at appropriate places in the article.  Much of the information detail is in the comments in the code sections.

### FormBase

All Forms inherit from `FormBase`.  `FormBase` provides the following functionality:

1. Replicates the code from `OwningComponentBase` to implement scoped service management.
2. Gets the User if Authentication is enabled.
3. Manages Form closure in Modal or Non-Modal state.
4. Implements the `IForm` and `IDisposable` interfaces. 

The scope management code looks like this.  You can search the Internet for articles on how to use `OwningComponentBase`.
   
```c#
// CEC.Blazor/Components/Base/BaseForm.cs
private IServiceScope _scope;

/// Scope Factory to manage Scoped Services
[Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

/// Gets the scoped IServiceProvider that is associated with this component.
protected IServiceProvider ScopedServices
{
    get
    {
        if (ScopeFactory == null) throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
        if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        _scope ??= ScopeFactory.CreateScope();
        return _scope.ServiceProvider;
    }
}
```
The `IDisposable` interface implementation is tied in with scoped service management.  We'll use it later for removing event handlers.

```c#
protected bool IsDisposed { get; private set; }

/// IDisposable Interface
async void IDisposable.Dispose()
{
    if (!IsDisposed)
    {
        _scope?.Dispose();
        _scope = null;
        Dispose(disposing: true);
        await this.DisposeAsync(true);
        IsDisposed = true;
    }
}

/// Dispose Method
protected virtual void Dispose(bool disposing) { }

/// Async Dispose event to clean up event handlers
public virtual Task DisposeAsync(bool disposing) => Task.CompletedTask;

```
The rest of the properties are:

```c#
[CascadingParameter] protected IModal ModalParent { get; set; }

/// Boolean Property to check if this component is in Modal Mode
public bool IsModal => this.ModalParent != null;

/// Cascaded Authentication State Task from CascadingAuthenticationState in App
[CascadingParameter] public Task<AuthenticationState> AuthenticationStateTask { get; set; }

/// Cascaded ViewManager 
[CascadingParameter] public ViewManager ViewManager { get; set; }

/// Check if ViewManager exists
public bool IsViewManager => this.ViewManager != null;

/// Property holding the current user name
public string CurrentUser { get; protected set; }

/// Guid string for user
public string CurrentUserID { get; set; }

/// UserName without the domain name
public string CurrentUserName => (!string.IsNullOrEmpty(this.CurrentUser)) && this.CurrentUser.Contains("@") ? this.CurrentUser.Substring(0, this.CurrentUser.IndexOf("@")) : string.Empty;

```
The main event methods:

```c#
/// OnRenderAsync Method from Component
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender) await GetUserAsync();
    await base.OnRenderAsync(firstRender);
}

/// Method to get the current user from the Authentication State
protected async Task GetUserAsync()
{
    if (this.AuthenticationStateTask != null)
    {
        var state = await AuthenticationStateTask;
        // Get the current user
        this.CurrentUser = state.User.Identity.Name;
        var x = state.User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"));
        this.CurrentUserID = x?.Value ?? string.Empty;
    }
}
```
Finally, the exit button methods.
   
```c#
public void Exit(ModalResult result)
{
    if (IsModal) this.ModalParent.Close(result);
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void Exit()
{
    if (IsModal) this.ModalParent.Close(ModalResult.Exit());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void Cancel()
{
    if (IsModal) this.ModalParent.Close(ModalResult.Cancel());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void OK()
{
    if (IsModal) this.ModalParent.Close(ModalResult.OK());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}
```

### ControllerServiceFormBase

At this point in the Form heirachy we add some complexity with generics.  We inject the Controller Service through the `IControllerService` interface, and we need to provide it with the RecordType we're loading `TRecord` and the DbContext to use `TContext`.  The class declaration apples the same contraints on the generics as `IControllerService`. The rest of the Properties are descibes in the code block.
   
```c#
// CEC.Blazor/Components/BaseForms/ControllerServiceFormBase.cs
    public class ControllerServiceFormBase<TRecord, TContext> : 
        FormBase 
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// Service with IDataRecordService Interface that corresponds to Type T
        /// Normally set as the first line in the OnRender event.
        public IControllerService<TRecord, TContext> Service { get; set; }

        /// Property to control various UI Settings
        /// Used as a cascadingparameter
        [Parameter] public UIOptions UIOptions { get; set; } = new UIOptions();

        /// The default alert used by all inherited components
        /// Used for Editor Alerts, error messages, ....
        [Parameter] public Alert AlertMessage { get; set; } = new Alert();

        /// Property with generic error message for the Page Manager 
        protected virtual string RecordErrorMessage { get; set; } = "The Application is loading the record.";

        /// Boolean check if the Service exists
        protected bool IsService { get => this.Service != null; }
    }
```
### RecordFormBase

This form is used directly by all the record display forms.  It introduces record managment.  Note the record itself resides in the Data Service. `RecordFormBase` holds the ID and makes the calls to the Record Service to load and reset the record.
   
```c#
// CEC.Blazor/Components/Base/RecordFormBase.cs
    public class RecordFormBase<TRecord, TContext> :
        ControllerServiceFormBase<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// This Page/Component Title
        public virtual string PageTitle => (this.Service?.Record?.DisplayName ?? string.Empty).Trim();

        /// Boolean Property that checks if a record exists
        protected virtual bool IsRecord => this.Service?.IsRecord ?? false;

        /// Used to determine if the page can display data
        protected virtual bool IsError { get => !this.IsRecord; }

        /// Used to determine if the page has display data i.e. it's not loading or in error
        protected virtual bool IsLoaded => !(this.Loading) && !(this.IsError);

        /// Property for the Record ID
        [Parameter]
        public int? ID
        {
            get => this._ID;
            set => this._ID = (value is null) ? -1 : (int)value;
        }

        /// No Null Version of the ID
        public int _ID { get; private set; }

        protected async override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender && this.IsService) await this.Service.ResetRecordAsync();
            await this.LoadRecordAsync(firstRender);
            await base.OnRenderAsync(firstRender);
        }

        /// Reloads the record if the ID has changed
        protected virtual async Task LoadRecordAsync(bool firstload = false)
        {
            if (this.IsService)
            {
                // Set the Loading flag 
                this.Loading = true;
                //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
                if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

                // Get the current record - this will check if the id is different from the current record and only update if it's changed
                await this.Service.GetRecordAsync(this._ID, false);

                // Set the error message - it will only be displayed if we have an error
                this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

                // Set the Loading flag
                this.Loading = false;
                //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
                if (!firstload) await RenderAsync();
            }
        }
    }
```

### EditRecordFormBase

This form is used directly by all record edit forms. It:
1. Manages the Form state based on the record state.  It locks the page in the application when the state is dirty and blocks browser navigation through the browser navigation challenge.
2. Saves the record.   
   
```c#
// CEC.Blazor/Components/Base/EditRecordFormBase.cs
public class EditRecordFormBase<TRecord, TContext> :
    RecordFormBase<TRecord, TContext>
    where TRecord : class, IDbRecord<TRecord>, new()
    where TContext : DbContext
{
    /// Boolean Property exposing the Service Clean state
    public bool IsClean => this.Service?.IsClean ?? true;

    /// EditContext for the component
    protected EditContext EditContext { get; set; }

    /// Property to concatinate the Page Title
    public override string PageTitle
    {
        get
        {
            if (this.IsNewRecord) return $"New {this.Service?.RecordConfiguration?.RecordDescription ?? "Record"}";
            else return $"{this.Service?.RecordConfiguration?.RecordDescription ?? "Record"} Editor";
        }
    }

    /// Boolean Property to determine if the record is new or an edit
    public bool IsNewRecord => this.Service?.RecordID == 0 ? true : false;

    /// property used by the UIErrorHandler component
    protected override bool IsError { get => !(this.IsRecord && this.EditContext != null); }

    protected async override Task LoadRecordAsync(bool firstLoad = false)
    {
        await base.LoadRecordAsync(firstLoad);
        //set up the Edit Context
        this.EditContext = new EditContext(this.Service.Record);
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            // Add the service listeners for the Record State
            this.Service.OnDirty += this.OnRecordDirty;
            this.Service.OnClean += this.OnRecordClean;
        }
    }

    protected void OnRecordDirty(object sender, EventArgs e)
    {
        this.ViewManager.LockView();
        this.AlertMessage.SetAlert("The Record isn't Saved", Bootstrap.ColourCode.warning);
        InvokeAsync(this.Render);
    }

    protected void OnRecordClean(object sender, EventArgs e)
    {
        this.ViewManager.UnLockView();
        this.AlertMessage.ClearAlert();
        InvokeAsync(this.Render);
    }

    /// Event handler for the RecordFromControls FieldChanged Event
    /// <param name="isdirty"></param>
    protected virtual void RecordFieldChanged(bool isdirty)
    {
        if (this.EditContext != null) this.Service.SetDirtyState(isdirty);
    }

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
            }
            // Set the alert message to the return result
            this.AlertMessage.SetAlert(this.Service.TaskResult);
            // Trigger a component State update - buttons and alert need to be sorted
            await RenderAsync();
        }
        else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", Bootstrap.ColourCode.danger);
        return ok;
    }

    /// Save and Exit Method called from the Button
    protected virtual async void SaveAndExit()
    {
        if (await this.Save()) this.ConfirmExit();
    }

    /// Confirm Exit Method called from the Button
    protected virtual void TryExit()
    {
        // Check if we are free to exit ot need confirmation
        if (this.IsClean) ConfirmExit();
    }

    /// Confirm Exit Method called from the Button
    protected virtual void ConfirmExit()
    {
        // To escape a dirty component set IsClean manually and navigate.
        this.Service.SetDirtyState(false);
        // Sort the exit strategy
        this.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        this.Service.OnDirty -= this.OnRecordDirty;
        this.Service.OnClean -= this.OnRecordClean;
        base.Dispose(disposing);
    }
}
```

## Implementing Edit Components

All the forms and Views are implemented in the `CEC.Weather` library.  As this is a Library there's no `_Imports.razor` so all the libraries used by the component must be declared in the Razor file.

The common ASPNetCore set are:

```c#
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Rendering;
@using Microsoft.AspNetCore.Components.Forms
```


### The View

The View is a very simple. It:
1. Declares all the used Libraries.
2. Sets the inheritance to `Component` - Views are simple.
3. Implements IView so it can be loaded as a View.
4. Sets the Namespace
6. Gets the `ViewManager` through the cascaded value
7. Declares an `ID` Parameter
5. Adds the Razor Markup for `WeatherForecastEditorForm`

```c#
// CEC.Weather/Components/Views/WeatherForecastEditorView.razor
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Rendering;
@using Microsoft.AspNetCore.Components.Forms
@using CEC.Blazor.Components
@using CEC.Blazor.Components.BaseForms
@using CEC.Blazor.Components.UIControls
@using CEC.Weather.Data
@using CEC.Weather.Components
@using CEC.Blazor.Components.Base

@inherits Component
@implements IView

@namespace CEC.Weather.Components.Views

<WeatherForecastEditorForm ID="this.ID"></WeatherForecastEditorForm>


@code {

    [CascadingParameter] public ViewManager ViewManager { get; set; }

    [Parameter] public int ID { get; set; } = 0;

}
```

### The Form

The code file is relatively simple, with most of the detail in the razor markup. It:
1. Declares the class with the correct Record and DbContext set.
1. Injects the correct Controller Service.
2. Assigns the controller service to `Service`.

```C#
// CEC.Weather/Components/Forms/WeatherForecastEditorForm.razor
public partial class WeatherForecastEditorForm : EditRecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
{
    [Inject]
    public WeatherForecastControllerService ControllerService { get; set; }

    protected override Task OnRenderAsync(bool firstRender)
    {
        // Assign the correct controller service
        if (firstRender) this.Service = this.ControllerService;
        return base.OnRenderAsync(firstRender);
    }
}
```

The Razor Markup below is an abbreviated version of the full file.  This makes extensive use of UIControls which will be covered in detail in the next article.  See the comments for detail.  The import concept to note here is the Razor Markup is all Controls - there's no HTML in sight. 

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
            <UIErrorHandler IsError="@this.IsError" IsLoading="this.Loading" ErrorMessage="@this.RecordErrorMessage">
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
                        <UIAlert Alert="this.AlertMessage" SizeCode="Bootstrap.SizeCode.sm"></UIAlert>
                    </UIColumn>
                    <UIButtonColumn Columns="5">
                        <UIButton Show="(!this.IsClean) && this.IsLoaded" ClickEvent="this.SaveAndExit" ColourCode="Bootstrap.ColourCode.save">Save &amp; Exit</UIButton>
                        <UIButton Show="(!this.IsClean) && this.IsLoaded" ClickEvent="this.Save" ColourCode="Bootstrap.ColourCode.save">Save</UIButton>
                        <UIButton Show="(!this.IsClean) && this.IsLoaded" ClickEvent="this.ConfirmExit" ColourCode="Bootstrap.ColourCode.danger_exit">Exit Without Saving</UIButton>
                        <UIButton Show="this.IsClean" ClickEvent="this.TryExit" ColourCode="Bootstrap.ColourCode.nav">Exit</UIButton>
                    </UIButtonColumn>
                </UIRow>
            </UIContainer>
        </CascadingValue>
    </Body>
</UICard>
```
### Form Event Code

#### Component Event Code

Looking at what going on in more detail, lets forst look at `OnRenderAsync`.

##### OnInitializedAsync

`OnRenderAsync` is implemented from top down (local code is run before calling the base method).  It:

1. Assigns the right data service to `Service`.
2. Calls `ResetRecordAsync` to reset the Service record data.
3. Loads the record through `LoadRecordAsync`.
4. Gets The user information.

```c#
// CEC.Weather/Components/Forms/WeatherEditorForm.razor.cs
protected override Task OnRenderAsync(bool firstRender)
{
    // Assign the correct controller service
    if (firstRender) this.Service = this.ControllerService;
    return base.OnRenderAsync(firstRender);
}

// CEC.Blazor/Components/BaseForms/RecordFormBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender && this.IsService) await this.Service.ResetRecordAsync();
    await this.LoadRecordAsync(firstRender);
    await base.OnRenderAsync(firstRender);
}

// CEC.Blazor/Components/BaseForms/ApplicationComponentBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender) {
        await GetUserAsync();
    }
    await base.OnRenderAsync(firstRender);
}
```

##### LoadRecordAsync

Record loading code is broken out so it can be used outside the component event driven methods.  It's implemented from bottom up (base method is called before any local code).

The primary record load functionaility is in `RecordFormBase` which gets and loads the record based on ID.  `EditFormBase` adds the extra editing functionality - it creates the edit context for the record. 

```C#
// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
protected virtual async Task LoadRecordAsync(bool firstload = false)
{
    if (this.IsService)
    {
        // Set the Loading flag 
        this.Loading = true;
        //  call Render only if we are not responding to first load
        if (!firstload) await RenderAsync();
        if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

        // Get the current record - this will check if the id is different from the current record and only update if it's changed
        await this.Service.GetRecordAsync(this._ID, false);

        // Set the error message - it will only be displayed if we have an error
        this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

        // Set the Loading flag
        this.Loading = false;
        //  call Render only if we are not responding to first load
        if (!firstload) await RenderAsync();
    }
}

// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
protected async override Task LoadRecordAsync(bool firstLoad = false)
{
    await base.LoadRecordAsync(firstLoad);
    //set up the Edit Context
    this.EditContext = new EditContext(this.Service.Record);
}
```

##### OnAfterRenderAsync

`OnAfterRenderAsync` is implemented from bottom up (base called before any local code is executed). It:

Assigns the record dirty events to local form events.

```C#
// CEC.Blazor/Components/BaseForms/EditFormBase.cs
protected async override Task OnAfterRenderAsync(bool firstRender)
{
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
        this.Service.OnDirty += this.OnRecordDirty;
        this.Service.OnClean += this.OnRecordClean;
    }
}
```

#### Event Handlers

There's one event handler wired up in the Component load events.

```c#
// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
// Event handler for the Record Form Controls FieldChanged Event
// wired to each control through a cascaded parameter
protected virtual void RecordFieldChanged(bool isdirty)
{
    if (this.EditContext != null) this.Service.SetDirtyState(isdirty);
}
```

#### Action Button Events

There are various actions wired up to buttons.  The important one is save.

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
        }
        // Set the alert message to the return result
        this.AlertMessage.SetAlert(this.Service.TaskResult);
        // Trigger a component State update - buttons and alert need to be sorted
        await RenderAsync();
    }
    else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", Bootstrap.ColourCode.danger);
    return ok;
}
```

## Implementing Viewer Pages

### The View

The routed view is a very simple.  It contains the routes and the component to load.

```c#
@using CEC.Blazor.Components
@using CEC.Weather.Components
@using CEC.Blazor.Components.Base

@namespace CEC.Weather.Components.Views
@implements IView

@inherits Component

<WeatherForecastViewerForm ID="this.ID"></WeatherForecastViewerForm>

@code {

    [CascadingParameter] public ViewManager ViewManager { get; set; }

    [Parameter] public int ID { get; set; } = 0;
}
```

### The Form

The code file is relatively simple, with most of the detail in the razor markup.

```C#
// CEC.Weather/Components/Forms/WeatherViewerForm.razor
public partial class WeatherForecastViewerForm : RecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
{
    [Inject]
    private WeatherForecastControllerService ControllerService { get; set; }

    public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

    protected override Task OnRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.Service = this.ControllerService;
        }
        return base.OnRenderAsync(firstRender);
    }

    protected async void NextRecord(int increment) 
    {
        var rec = (this._ID + increment) == 0 ? 1 : this._ID + increment;
        rec = rec > this.Service.BaseRecordCount ? this.Service.BaseRecordCount : rec;
        this.ID = rec;
        await this.ResetAsync();
    }
}
```

This gets and assigns the specific ControllerService through DI to the `IContollerService Service` Property.

The Razor Markup below is an abbreviated version of the full file.  This makes extensive use of UIControls which will be covered in detail in a later article.  See the comments for detail. 
```C#
// CEC.Weather/Components/Forms/WeatherViewerForm.razor.cs
// UI Card is a Bootstrap Card
<UICard IsCollapsible="false">
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        // Error handler - only renders it's content when the record exists and is loaded
        <UIErrorHandler IsError="@this.IsError" IsLoading="this.Loading" ErrorMessage="@this.RecordErrorMessage">
            <UIContainer>
                    .....
                    // Example data value row with label and edit control
                    <UIRow>
                        <UILabelColumn Columns="2">
                            Date
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.Date.AsShortDate()"></FormControlPlainText>
                        </UIColumn>

                        <UILabelColumn Columns="2">
                            ID
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.ID.ToString()"></FormControlPlainText>
                        </UIColumn>

                        <UILabelColumn Columns="2">
                            Frost
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.Frost.AsYesNo()"></FormControlPlainText>
                        </UIColumn>
                    </UIRow>
                    ..... // more form rows here
            </UIContainer>
        </UIErrorHandler>
        // Container for the buttons - not record dependant so outside the error handler to allow navigation if UIErrorHandler is in error.
        <UIContainer>
            <UIRow>
                <UIColumn Columns="6">
                    <UIButton Show="this.IsLoaded" ColourCode="Bootstrap.ColourCode.dark" ClickEvent="(e => this.NextRecord(-1))">
                        Previous
                    </UIButton>
                    <UIButton Show="this.IsLoaded" ColourCode="Bootstrap.ColourCode.dark" ClickEvent="(e => this.NextRecord(1))">
                        Next
                    </UIButton>
                </UIColumn>
                <UIButtonColumn Columns="6">
                    <UIButton Show="true" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.Exit())">
                        Exit
                    </UIButton>
                </UIButtonColumn>
            </UIRow>
        </UIContainer>
    </Body>
</UICard>
```

### Wrap Up
That wraps up this article.  We've looked at the Editor code in detail to see how it works, and then taken a quick look at the Viewer code.  We'll look in more detail at the List components in a separate article.   
Some key points to note:
1. The Blazor Server and Blazor WASM code is the same - it's in the common library.
2. Almost all the functionality is implemented in the library components.  Most of the application code is Razor markup for the individual record fields.
3. The Razor files contains controls, not HTML.
4. Async functionality in used through.


## History

* 19-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
