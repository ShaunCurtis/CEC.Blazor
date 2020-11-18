# Building a Database Application in Blazor 
## Part 5 - View Components - CRUD List Operations in the UI

## Introduction

This is the fifth in the series looking at how to build and structure a real Database Application in Blazor. The articles so far are:


1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

Further articles: 
* A walk through detailing how to add more records to the application - in this case weather stations and weather station data.

This article looks in detail at building reusable List Presentation Layer components and deploying them in both Server and WASM projects.

## Repository and Database

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server version of the project running here](https://cec-blazor-server.azurewebsites.net/).

[You can see the WASM version of the project running here](https://cec-blazor-wasm.azurewebsites.net/).

## List Functionality

List components present far more challenges than other CRUD components.  Functionality expected in a professional level list control includes:
* Paging to handle large data sets
* Column formatting to control column width and data overflow
* Sorting on individual columns
* Filtering


## The Base Forms

`ListFormBase` is the base form for all lists. It inherits from `ControllerServiceFormBase`.

Not all the code is shown in the article - some class are simply too big and I only show the most relevant sections.  All source files can be viewed on the Github site, and I include references or links to specific code files at appropriate places in the article.  Much of the detail on what sections of code do is in the code comments.

### Paging

Paging is implemented through the `IControllerPagingService` interface. `BaseControllerService` implements this interface.  It's not shown in detail here because it's too big.  Much of the functionality is pretty obvious - properties to track which page your on, how many pages and blocks you have, page size, etc - so we'll skip to the more interesting sections.

#### Initial Form Loading

Lets start with loading a list form and look at `OnRenderAsync`.

```c#
// CEC.Weather/Components/Forms/WeatherForecastListForm.razor.cs
protected override Task OnRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Sets the specific service
        this.Service = this.ControllerService;
        // Sets the max column
        this.UIOptions.MaxColumn = 3;
    }
    return base.OnRenderAsync(firstRender);
}
```
```c#
// CEC.Blazor/Components/BaseForms/ListFormBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    // set the page to 1
    var page = 1;
    if (this.IsService)
    {
        if (firstRender)
        {
            // Reset the Service if this is the first load
            await this.Service.Reset();
            this.ListTitle = string.IsNullOrEmpty(this.ListTitle) ? $"List of {this.Service.RecordConfiguration.RecordListDecription}" : this.ListTitle;

        }
        // Load the filters for the recordset
        this.LoadFilter();
        // Check if we have a saved Page No in the ViewData
        if (this.IsViewManager && !this.ViewManager.ViewData.GetFieldAsInt("Page", out page)) page = 1;
        // Load the paged recordset
        await this.Service.LoadPagingAsync();
        // go to the correct page
        await this.Paging.GoToPageAsync(page);
        this.Loading = false;
    }
    await base.OnRenderAsync(firstRender);

}
```
```c#
// CEC.Blazor/Components/BaseForms/FormBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender) {
        await GetUserAsync();
    }
    await base.OnRenderAsync(firstRender);
}
```

```c#
// base LoadFilter
protected virtual void LoadFilter()
{
    // Set OnlyLoadIfFilter if the Parameter value is set
    if (IsService) this.Service.FilterList.OnlyLoadIfFilters = this.OnlyLoadIfFilter;
}
```
We're interested in `ListFormBase` calling `LoadPagingAsync()`

```c#
// CEC.Blazor/Components/BaseForms/ListComponentBase.cs

/// Method to load up the Paged Data to display
/// loads the delegate with the default service GetDataPage method and loads the first page
/// Can be overridden for more complex situations
public async virtual Task LoadPagingAsync()
{
    // set the record to null to force a reload of the records
    this.Records = null;
    // if requested adds a default service function to the delegate
    this.PageLoaderAsync = new IControllerPagingService<TRecord>.PageLoaderDelegateAsync(this.GetDataPageWithSortingAsync);
    // loads the paging object
    await this.LoadAsync();
    // Trigger event so any listeners get notified
    this.TriggerListChangedEvent(this);
}
```

`IControllerPagingService` defines a delegate `PageLoaderDelegateAsync` which returns a List of TRecords, and a delegate property `PageLoaderAsync`.  

```c#
// CEC.Blazor/Services/Interfaces/IControllerPagingService.cs

// Delegate that returns a generic Record List
public delegate Task<List<TRecord>> PageLoaderDelegateAsync();

// Holds the reference to the PageLoader Method to use
public PageLoaderDelegateAsync PageLoaderAsync { get; set; }
```

By default, `LoadPagingAsync` loads the method `GetDataPageWithSortingAsync` as `PageLoaderAsync`, and then calls `LoadAsync`.

`LoadAsync` resets various properties (We did this in `OnRenderAsync` by resetting the service, but `LoadAsync` is called from elsewhere where we need to reset the record list - but not the whole service), and calls the `PageLoaderAsync` delegate.

```c#
// CEC.Blazor/Services/BaseControllerService.cs
public async Task<bool> LoadAsync()
{
    // Reset the page to 1
    this.CurrentPage = 1;
    // Check if we have a sort column, if not set to the default column
    if (!string.IsNullOrEmpty(this.DefaultSortColumn)) this.SortColumn = this.DefaultSortColumn;
    // Set the sort direction to the default
    this.SortingDirection = DefaultSortingDirection;
    // Check if we have a method loaded in the PageLoaderAsync delegate and if so run it
    if (this.PageLoaderAsync != null) this.PagedRecords = await this.PageLoaderAsync();
    // Set the block back to the start
    this.ChangeBlock(0);
    //  Force a UI update as everything has changed
    this.PageHasChanged?.Invoke(this, this.CurrentPage);
    return true;
}
```
`GetDataPageWithSortingAsync` is shown below.  See the comments for detail. `GetFilteredListAsync` is always called to refresh the list recordset if necessary. 

```c#
// CEC.Blazor/Services/BaseControllerService.cs
public async virtual Task<List<TRecord>> GetDataPageWithSortingAsync()
{
    // Get the filtered list - will only get a new list if the Records property has been set to null elsewhere
    await this.GetFilteredListAsync();
    // Reset the start record if we are outside the range of the record set - a belt and braces check as this shouldn't happen!
    if (this.PageStartRecord > this.Records.Count) this.CurrentPage = 1;
    // Check if we have to apply sorting, in not get the page we want
    if (string.IsNullOrEmpty(this.SortColumn)) return this.Records.Skip(this.PageStartRecord).Take(this._PageSize).ToList();
    else
    {
        //  If we do order the record set and then get the page we want
        if (this.SortingDirection == SortDirection.Ascending)
        {
            return this.Records.OrderBy(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
        }
        else
        {
            return this.Records.OrderByDescending(x => x.GetType().GetProperty(this.SortColumn).GetValue(x, null)).Skip(this.PageStartRecord).Take(this._PageSize).ToList();
        }
    }
}
```
`GetFilteredListAsync` gets a filter list recordset.  It's overridden in form components where filtering is applied - such as from a filter control.  The default implementation gets the full recordset.  It uses `IsRecords` to check if it needs to reload the recordset.  It only reloads if `Records` is null.

```c#
// CEC.Blazor/Services/BaseControllerService.cs
public async virtual Task<bool> GetFilteredListAsync()
{
    // Check if the record set is null. and only refresh the record set if it's null
    if (!this.IsRecords)
    {
        //gets the filtered record list
        this.Records = await this.Service.GetFilteredRecordListAsync(FilterList);
        return true;
    }
    return false;
}
```
To summarise:
1. On Form loading `Service` (the specific data service for the record type) gets reset.
2. `GetDataPageWithSortingAsync()` on the `Service` gets loaded into the `Service` delegate
3. The Delegate gets called with `Records` in the `Service` set to null
4. `GetFilteredListAsync()` loads `Records`
5. `GetDataPageWithSortingAsync()` loads the first page
6. `IsLoading` is set to false so the `UIErrorHandler` UI control displays the page.
7. The Form is refreshed after `OnParametersSetAsync` automatically so no manual call to `StateHasChanged` is required. `OnInitializedAsync` is part of `OnParametersSetAsync` and only completes when `OnInitializedAsync` completes. 
   
#### Form Events

The `PagingControl` interfaces directly with the Form `Service` through the `IPagingControlService` interface, and links button clicks to `IPagingControlService` methods:

1. `ChangeBlockAsync(int direction,bool supresspageupdate)`
2. `MoveOnePageAsync(int direction)`
3. `GoToPageAsync(int pageno)`

```c#
// CEC.Blazor/Services/BaseControllerService.cs

/// Moves forward or backwards one block
/// direction 1 for forwards
/// direction -1 for backwards
/// suppresspageupdate 
///  - set to true (default) when user changes page and the block changes with the page
///  - set to false when user changes block rather than changing page and the page needs to be updated to the first page of the block
public async Task ChangeBlockAsync(int direction, bool suppresspageupdate = true)
{
    if (direction == 1 && this.EndPage < this.TotalPages)
    {
        this.StartPage = this.EndPage + 1;
        if (this.EndPage + this.PagingBlockSize < this.TotalPages) this.EndPage = this.StartPage + this.PagingBlockSize - 1;
        else this.EndPage = this.TotalPages;
        if (!suppresspageupdate) this.CurrentPage = this.StartPage;
    }
    else if (direction == -1 && this.StartPage > 1)
    {
        this.EndPage = this.StartPage - 1;
        this.StartPage = this.StartPage - this.PagingBlockSize;
        if (!suppresspageupdate) this.CurrentPage = this.StartPage;
    }
    else if (direction == 0 && this.CurrentPage == 1)
    {
        this.StartPage = 1;
        if (this.EndPage + this.PagingBlockSize < this.TotalPages) this.EndPage = this.StartPage + this.PagingBlockSize - 1;
        else this.EndPage = this.TotalPages;
    }
    if (!suppresspageupdate) await this.PaginateAsync();
}
```
```c#
/// Moves forward or backwards one page
/// direction 1 for forwards
/// direction -1 for backwards
public async Task MoveOnePageAsync(int direction)
{
    if (direction == 1)
    {
        if (this.CurrentPage < this.TotalPages)
        {
            if (this.CurrentPage == this.EndPage) await ChangeBlockAsync(1);
            this.CurrentPage += 1;
        }
    }
    else if (direction == -1)
    {
        if (this.CurrentPage > 1)
        {
            if (this.CurrentPage == this.StartPage) await ChangeBlockAsync(-1);
            this.CurrentPage -= 1;
        }
    }
    await this.PaginateAsync();
}
```
```c#
/// Moves to the specified page
public Async Task GoToPageAsync(int pageno)
{
    this.CurrentPage = pageno;
    await this.PaginateAsync();
}

```
All the above methods set up the `IPagingControlService` properties and then call `PaginateAsync()` which calls the PageLoaderAsync delegate and forces a UI update.

```c#
// CEC.Blazor/Services/BaseControllerService.cs

/// Method to trigger the page Changed Event
public async Task PaginateAsync()
{
    // Check if we have a method loaded in the PageLoaderAsync delegate and if so run it
    if (this.PageLoaderAsync != null) this.PagedRecords = await this.PageLoaderAsync();
    //  Force a UI update as something has changed
    this.PageHasChanged?.Invoke(this, this.CurrentPage);
}
```
#### PageControl

The `PageControl` code is shown below and documented with comments.

```c#
// CEC.Blazor/Components/FormControls/PagingControl.razor

@if (this.IsPagination)
{
    <div class="pagination ml-2 flex-nowrap">
        <nav aria-label="Page navigation">
            <ul class="pagination mb-1">
                @if (this.DisplayType != PagingDisplayType.Narrow)
                {
                    @if (this.DisplayType == PagingDisplayType.FullwithoutPageSize)
                    {
                        <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.ChangeBlockAsync(-1, false))">1&laquo;</button></li>
                    }
                    else
                    {
                        <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.ChangeBlockAsync(-1, false))">&laquo;</button></li>
                    }
                    <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.MoveOnePageAsync(-1))">Previous</button></li>
                    @for (int i = this.Paging.StartPage; i <= this.Paging.EndPage; i++)
                    {
                        var currentpage = i;
                        <li class="page-item @(currentpage == this.Paging.CurrentPage ? "active" : "")"><button class="page-link" @onclick="(e => this.Paging.GoToPageAsync(currentpage))">@currentpage</button></li>
                    }
                    <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.MoveOnePageAsync(1))">Next</button></li>
                    @if (this.DisplayType == PagingDisplayType.FullwithoutPageSize)
                    {
                        <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.ChangeBlockAsync(1, false))">&raquo;@this.Paging.TotalPages</button></li>
                    }
                    else
                    {
                        <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.ChangeBlockAsync(1, false))">&raquo;</button></li>
                    }
                }
                else
                {
                    <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.MoveOnePageAsync(-1))">1&laquo;</button></li>
                    @for (int i = this.Paging.StartPage; i <= this.Paging.EndPage; i++)
                    {
                        var currentpage = i;
                        <li class="page-item @(currentpage == this.Paging.CurrentPage ? "active" : "")"><button class="page-link" @onclick="(e => this.Paging.GoToPageAsync(currentpage))">@currentpage</button></li>
                    }
                    <li class="page-item"><button class="page-link" @onclick="(e => this.Paging.MoveOnePageAsync(1))">&raquo;@this.Paging.TotalPages</button></li>
                }
            </ul>
        </nav>
        @if (this.DisplayType == PagingDisplayType.Full)
        {
            <span class="pagebutton btn btn-link btn-sm disabled mr-1">Page @this.Paging.CurrentPage of @this.Paging.TotalPages</span>
        }
    </div>
}
```
```c#
// CEC.Blazor/Components/FormControls/PagingControl.razor.cs

public partial class PagingControl<TRecord> : ComponentBase where TRecord : IDbRecord<TRecord>, new()
{
    [CascadingParameter] public IControllerPagingService<TRecord> _Paging { get; set; }

    [Parameter] public IControllerPagingService<TRecord> Paging { get; set; }

    [Parameter] public PagingDisplayType DisplayType { get; set; } = PagingDisplayType.Full;

    [Parameter] public int BlockSize { get; set; } = 0;

    private bool IsPaging => this.Paging != null;

    private bool IsPagination => this.Paging != null && this.Paging.IsPagination;

    protected override Task OnRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Check if we have a cascaded IControllerPagingService if so use it
            this.Paging = this._Paging ?? this.Paging;
        }
        if (this.IsPaging)
        {
            this.Paging.PageHasChanged += this.UpdateUI;
            if (this.DisplayType == PagingDisplayType.Narrow) Paging.PagingBlockSize = 4;
            if (BlockSize > 0) Paging.PagingBlockSize = this.BlockSize;
        }
        return base.OnRenderAsync(firstRender);
    }

    protected async void UpdateUI(object sender, int recordno) => await RenderAsync();

    private string IsCurrent(int i) => i == this.Paging.CurrentPage ? "active" : string.Empty;
}
```

#### WeatherForecastListForm

The Code for the `WeatherForecastListForm` is fairly self explanatory.  `OnView` and `OnEdit` either route to the Viewer or Editor, or open the dialogs if `UIOptions` specifies Use Modals.

```c#
// CEC.Weather/Components/Forms/WeatherForecastListForm.razor.cs

public partial class WeatherForecastListForm : ListFormBase<DbWeatherForecast, WeatherForecastDbContext>
{
    /// The Injected Controller service for this record
    [Inject] protected WeatherForecastControllerService ControllerService { get; set; }

    protected override Task OnRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Sets the specific service
            this.Service = this.ControllerService;
            // Sets the max column
            this.UIOptions.MaxColumn = 3;
        }
        return base.OnRenderAsync(firstRender);
    }

    /// Method called when the user clicks on a row in the viewer.
    protected void OnView(int id)
    {
        if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastViewerForm>(id);
        else this.OnViewAsync<WeatherForecastViewerView>(id);
    }

    /// Method called when the user clicks on a row Edit button.
    protected void OnEdit(int id)
    {
        if (this.UIOptions.UseModalViewer && this.ViewManager.ModalDialog != null) this.OnModalAsync<WeatherForecastEditorForm>(id);
        else this.OnViewAsync<WeatherForecastEditorView>(id);
    }
}
```

The Razor Markup below is an abbreviated version of the full file.  This makes extensive use of UIControls which were covered in the previous article.  See the comments for detail. 
```C#
// CEC.Weather/Components/Forms/WeatherForecastListForm.razor

// Wrapper that cascades the values including event handlers
<UIWrapper UIOptions="@this.UIOptions" RecordConfiguration="@this.Service.RecordConfiguration" OnView="@OnView" OnEdit="@OnEdit">

    // UI CardGrid is a Bootstrap Card
    <UICardGrid TRecord="DbWeatherForecast" IsCollapsible="true" Paging="this.Paging" IsLoading="this.Loading">

        <Title>
            @this.ListTitle
        </Title>

        // Header Section of UICardGrid
        <TableHeader>
            // Header Grid columns
            <UIGridTableHeaderColumn TRecord="DbWeatherForecast" Column="1" FieldName="WeatherForecastID">ID</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherForecast" Column="2" FieldName="Date">Date</UIGridTableHeaderColumn>
            .....
            <UIGridTableHeaderColumn TRecord="DbWeatherForecast" Column="7"></UIGridTableHeaderColumn>
        </TableHeader>

        // Row Template Section of UICardGrid
        <RowTemplate>
            // Cascaded ID for the specific Row
            <CascadingValue Name="RecordID" Value="@context.WeatherForecastID">
                <UIGridTableColumn TRecord="DbWeatherForecast" Column="1">@context.WeatherForecastID</UIGridTableColumn>
                <UIGridTableColumn TRecord="DbWeatherForecast" Column="2">@context.Date.ToShortDateString()</UIGridTableColumn>
                .....
                <UIGridTableEditColumn TRecord="DbWeatherForecast"></UIGridTableEditColumn>
            </CascadingValue>

        </RowTemplate>
        // Navigation Section of UUCardGrid
        <Navigation>

            <UIListButtonRow>
                // Paging part of UIListButtonRow
                <Paging>
                    <PagingControl TRecord="DbWeatherForecast" Paging="this.Paging"></PagingControl>
                </Paging>
            </UIListButtonRow>

        </Navigation>
    </UICardGrid>
</UIWrapper>
```

The Razor component uses a set of UIControls designed for list building.  `UICardGrid` builds the Bootstrap Card and the table framework.  You can explore the components code in the Github Repository.

#### WeatherForcastListModalView

This is pretty simple.  Razor markup for a `WeatherForecastListForm` and a `UIOptions` object.
```c#
// CEC.Weather/Components/Views/WeatherForecastListModalView.razor

@using CEC.Blazor.Components
@using CEC.Blazor.Components.BaseForms
@using CEC.Blazor.Components.UIControls
@using CEC.Weather.Data
@using CEC.Weather.Components
@using CEC.Blazor.Components.Base

@namespace CEC.Weather.Components.Views

@inherits Component
@implements IView

<WeatherForecastListForm UIOptions="this.UIOptions"></WeatherForecastListForm>

@code {

    [CascadingParameter]
    public ViewManager ViewManager { get; set; }

    public UIOptions UIOptions => new UIOptions()
    {
        ListNavigationToViewer = true,
        ShowButtons = true,
        ShowAdd = true,
        ShowEdit = true,
        UseModalEditor = true,
        UseModalViewer = true
    };

}
```

### Wrap Up
That wraps up this article.  Some key points to note:
1. There's no differences in code between a Blazor Server and a Blazor WASM.
2. Almost all functionality is implemented in the library components.  Most of the application code is Razor markup for the individual record fields.
3. Async functionality is used throughout the components and CRUD data access.
    
## History

* 25-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
