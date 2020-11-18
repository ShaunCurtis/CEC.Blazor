# Building a Database Application in Blazor 
## Part 4 - UI Components

## Introduction

This is the fourth article in the series looking at how to build and structure a real Database Application in Blazor. The articles so far are:

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

This article looks at the components we use in the UI and then focuses on how to build generic UI Components from HTML and CSS.

## Repository and Database

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server version of the project running here](https://cec-blazor-server.azurewebsites.net/).

[You can see the WASM version of the project running here](https://cec-blazor-wasm.azurewebsites.net/).

### Components

For a detailed look at components read my article [A Dive into Blazor Components](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).

Everything in the Blazor UI, other than the start page, is a component.  Yes App, Router,...

There are 4 categories of component in the application:
1. Views - these are get displayed on screen.  Views are combined with a Layout to make the display window.
2. Layouts - Layouts combine with Views to make up the display window.
3. Forms - Forms are logical collections of controls.  Edit forms, display forms, list forms, data entry wizards are all classic forms.  Forms contain controls - not HTML.
4. Controls - Controls display something - the emit HTML.  Text boxes, dropdowns, buttons, grids are all classic controls.

### Views

Views are specific to the application, but are common to WASM and Server, so live in */Components/Views* of the application library.

The Weather Forecast Viewer and List Views are shown below.
```cs
// CEC.Weather/Components/Views/WeatherForecastViewerView.cs
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
The list view defines a UIOptions object that control various list control display options.
```cs
// CEC.Blazor.Server/Routes/WeatherForecastListView.cs
@using CEC.Blazor.Components
@using CEC.Blazor.Components.UIControls
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
        ShowEdit = true
    };
}
```

### Forms

Forms are specific to the application, but are common to WASM and Server, so live in */Components/Views* of the application library.

The code below shows the Weather Viewer.  It's all UI Controls, no HTML markup.

```html
// CEC.Weather/Components/Forms/WeatherForecastViewerForm.razor
<UICard>
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        <UIErrorHandler IsError="this.IsError" IsLoading="this.IsDataLoading" ErrorMessage="@this.RecordErrorMessage">
            <UIContainer>
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
            ..........
            </UIContainer>
        </UIErrorHandler>
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
                    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToList))">
                        Exit To List
                    </UIButton>
                    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToLast))">
                        Exit
                    </UIButton>
                    <UIButton Show="this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.ModalExit())">
                        Exit
                    </UIButton>
                </UIButtonColumn>
            </UIRow>
        </UIContainer>
    </Body>
```

The code behind page is relatively simple - the complexity is in the boilerplate code in parent classes.  It loads the record specific Controller service.

```cs
// CEC.Weather/Components/Forms/WeatherForecastViewerForm.razor.cs
    public partial class WeatherForecastViewerForm : RecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherForecastControllerService ControllerService { get; set; }

        public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

        protected override Task OnRenderAsync(bool firstRender)
        {
            if (firstRender) this.Service = this.ControllerService;
            return base.OnRenderAsync(firstRender);
        }
    }
```

## UI Controls

UI Controls emit HTML and CSS markup based on Bootstrap as the UI CSS Framework.  All controls inherit from `ControlBase` and UI Controls inherit from `UIBase`.

##### UIBase

`UIBase` inherits from `ControlBase` which inherits from `Component`.  It builds an HTML DIV block that you can turn on or off.

Lets look at some of the bits of `UIBase` in detail.

The HTML block tag can be set using the `Tag` parameter.  However, the component doesn't use this property directly to build the HTML.  It uses a second protected property `_Tag`.  In the base implementation this returns `Tag`.

```cs
/// Css for component - can be overridden and fixed in inherited components
[Parameter]
public virtual string Tag { get; set; } = "div";

protected virtual string _Tag => this.Tag;
```
  `_Tag` is declared virtual so in derived classes you can override it and set the tag.  So in in `UIAnchor` class you would set it and thus override anything set in `Tag`.

```cs
protected overridden string _Tag => "a";
```

Css works in a similar way.

```cs
/// Css for component - can be overridden and fixed in inherited components
[Parameter]
public virtual string Css { get; set; } = string.Empty;

/// Additional Css to tag on the end of the base Css
[Parameter]
public string AddOnCss { get; set; } = string.Empty;

/// Property for fixing the base Css.  Base returns the Parameter Css, but can be overridden in inherited classes
protected virtual string _BaseCss => this.Css;

/// Property for fixing the Add On Css.  Base returns the Parameter AddOnCss, but can be overridden say to String.Empty in inherited classes
protected virtual string _AddOnCss => this.AddOnCss;

/// Actual calculated Css string used in the component
protected virtual string _Css => this.CleanUpCss($"{this._BaseCss} {this._AddOnCss}");

/// Method to clean up the Css - remove leading and trailing spaces and any multiple spaces
protected string CleanUpCss(string css)
{
    while (css.Contains("  ")) css = css.Replace("  ", " ");
    return css.Trim();
}

```
So to fix the base Css.

```cs
protected overridden string _BaseCss => "button";
```

Razor markup declared attributes are captured in `AdditionalAttributes`.  `UsedAttributes` is a list of those not to add to the component.  `ClearDuplicateAttributes()` removes the UsedAttributes.

```cs
/// Gets or sets a collection of additional attributes that will be applied to the created <c>form</c> element.
[Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; }

/// Html attributes that need to be removed if set on the control default is only the class attribute
protected List<string> UsedAttributes { get; set; } = new List<string>() { "class" };

/// Method to clean up the Additional Attributes
protected void ClearDuplicateAttributes()
{
    if (this.AdditionalAttributes != null && this.UsedAttributes != null)
    {
        foreach (var item in this.UsedAttributes)
        {
            if (this.AdditionalAttributes.ContainsKey(item)) this.AdditionalAttributes.Remove(item);
        }
    }
}
```
Finally `BuildRenderTree` builds out the HTML for the component.  In this case we are doing this in code and not using a Razor markup file. It:
1. Checks if it should be displayed.
2. Clears the unwanted attributes from `AdditionalAttributes`.
3. Creates the element with the correct tag.
4. Adds the `AdditionalAttributes`.
5. Adds the CSS.
6. Adds the Child Content 

```cs
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    if (this._Show)
    {
        this.ClearDuplicateAttributes();
        builder.OpenElement(0, this._Tag);
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", this._Css);
        if (!string.IsNullOrEmpty(this._Content)) builder.AddContent(3, (MarkupString)this._Content);
        else if (this.ChildContent != null) builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
```

##### UIBootstrapBase

`UIBootstrapBase` adds extra functionality for Bootstrap components. Formatting options such a component colour and sizing are represented as Enums, and Css fragments built based on the selected Enum.

```cs
// CEC.Blazor/Components/UIControls/UIBootstrapBase.cs
public class UIBootstrapBase : UIBase
{
    protected virtual string CssName { get; set; } = string.Empty;

    /// Bootstrap Colour for the Component
    [Parameter]
    public Bootstrap.ColourCode ColourCode { get; set; } = Bootstrap.ColourCode.info;

    /// Bootstrap Size for the Component
    [Parameter]
    public Bootstrap.SizeCode SizeCode { get; set; } = Bootstrap.SizeCode.normal;

    /// Property to set the HTML value if appropriate
    [Parameter]
    public string Value { get; set; } = "";

    /// Property to get the Colour CSS
    protected virtual string ColourCssFragment => GetCssFragment<Bootstrap.ColourCode>(this.ColourCode);

    /// Property to get the Size CSS
    protected virtual string SizeCssFragment => GetCssFragment<Bootstrap.SizeCode>(this.SizeCode);

    /// CSS override
    protected override string _Css => this.CleanUpCss($"{this.CssName} {this.SizeCssFragment} {this.ColourCssFragment} {this.AddOnCss}");

    /// Method to format as Bootstrap CSS Fragment
    protected string GetCssFragment<T>(T code) => $"{this.CssName}-{Enum.GetName(typeof(T), code).Replace("_", "-")}";
}
```
### Some Examples

The rest of the article looks at a few of the UI controls in more detail.

##### UIButton

This is a standard Bootstrap Button. 
1. `ButtonType` and `ClickEvent` are specific to buttons.
2. `CssName` and `_Tag` are hardwired.
3. `ButtonClick` handles the button click event.
4. `BuildRenderTree` builds the markup and wires the JSInterop `onclick` event.
5. `Show` controls whether the button gets rendered.

```cs
// CEC.Blazor/Components/UIControls/UIButton.cs
public class UIButton : UIBootstrapBase
{
    /// Property setting the button HTML attribute Type
    [Parameter]
    public string ButtonType { get; set; } = "button";

    /// Override the CssName
    protected override string CssName => "btn";

    /// Override the Tag
    protected override string _Tag => "button";

    /// Callback for a button click event
    [Parameter]
    public EventCallback<MouseEventArgs> ClickEvent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (this.Show)
        {
            builder.OpenElement(0, this._Tag);
            builder.AddAttribute(1, "type", this.ButtonType);
            builder.AddAttribute(2, "class", this._Css);
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }

    /// Event handler for button click
    protected void ButtonClick(MouseEventArgs e) => this.ClickEvent.InvokeAsync(e);
}
```

Here's some code showing the control in use.

```html
// CEC.Weather/Components/Forms/WeatherViewer.razor
<UIButtonColumn Columns="6">
    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToList))">
        Exit To List
    </UIButton>
    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToLast))">
        Exit
    </UIButton>
    <UIButton Show="this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.ModalExit())">
        Exit
    </UIButton>
</UIButtonColumn>
```

##### UIAlert

This is a standard Bootstrap Alert. 
1. `Alert` is a class to encapsulate an Alert.
2. `ColourCssFragement`, `Show` and `_Content` are wired into the Alert object instance.

```cs
// CEC.Blazor/Components/UIControls/UI/UIAlert.cs
public class UIAlert : UIBootstrapBase
{
    /// Alert to display
    [Parameter]
    public Alert Alert { get; set; } = new Alert();

    /// Set the CssName
    protected override string CssName => "alert";

    /// Property to override the colour CSS
    protected override string ColourCssFragment => this.Alert != null ? GetCssFragment<Bootstrap.ColourCode>(this.Alert.ColourCode) : GetCssFragment<Bootstrap.ColourCode>(this.ColourCode);

    /// Boolean Show override
    protected override bool _Show => this.Alert?.IsAlert ?? false;

    /// Override the content with the alert message
    protected override string _Content => this.Alert?.Message ?? string.Empty;
}
```

Here's some code showing the control in use.

```html
// CEC.Weather/Components/Forms/WeatherEditor.razor
<UIContainer>
    <UIRow>
        <UIColumn Columns="7">
            <UIAlert Alert="this.AlertMessage" SizeCode="Bootstrap.SizeCode.sm"></UIAlert>
        </UIColumn>
        <UIButtonColumn Columns="5">
             .........
        </UIButtonColumn>
    </UIRow>
</UIContainer>
```

##### UIErrorHandler

This is a wrapper control designed to save implementing error checking in child content. It has three states controlled by `IsError` and `IsLoading`:
1. Loading - when it displays the loading message and the spinner.
2. Error - when it displays an error message.
3. Loaded - when it displays the Child Content.
   
Any controls within the child content only get added to the RenderTree when loading is complete and `IsError` is false.  

The control  saves implementing a lot of error checking in the child content.

```cs
// CEC.Blazor/Components/UIControls/UI/UIErrorHandler.cs
public class UIErrorHandler : UIBase
{
    /// Enum for the Control State
    public enum ControlState { Error = 0, Loading = 1, Loaded = 2}

    /// Boolean Property that determines if the child content or an error message is diplayed
    [Parameter]
    public bool IsError { get; set; } = false;

    /// Boolean Property that determines if the child content or an loading message is diplayed
    [Parameter]
    public bool IsLoading { get; set; } = true;

    /// Control State
    public ControlState State
    {
        get
        {
            if (IsError && !IsLoading) return ControlState.Error;
            else if (!IsLoading) return ControlState.Loaded;
            else return ControlState.Loading;
        }
    }

    /// CSS Override
    protected override string _BaseCss => this.IsLoading? "text-center p-3": "label label-error m-2";

    /// Customer error message to display
    [Parameter]
    public string ErrorMessage { get; set; } = "An error has occured loading the content";

        
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        this.ClearDuplicateAttributes();
        switch (this.State)
        {
            case ControlState.Loading:
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", this._Css);
                builder.OpenElement(3, "button");
                builder.AddAttribute(4, "class", "btn btn-primary");
                builder.AddAttribute(5, "type", "button");
                builder.AddAttribute(6, "disabled", "disabled");
                builder.OpenElement(7, "span");
                builder.AddAttribute(8, "class", "spinner-border spinner-border-sm pr-2");
                builder.AddAttribute(9, "role", "status");
                builder.AddAttribute(10, "aria-hidden", "true");
                builder.CloseElement();
                builder.AddContent(11, "  Loading...");
                builder.CloseElement();
                builder.CloseElement();
                break;
            case ControlState.Error:
                builder.OpenElement(1, "div");
                builder.OpenElement(2, "span");
                builder.AddAttribute(3, "class", this._Css);
                builder.AddContent(4, ErrorMessage);
                builder.CloseElement();
                builder.CloseElement();
                break;
            default:
                builder.AddContent(1, ChildContent);
                break;
        };
    }
}
```

Here's some code showing the control in use.

```html
// CEC.Weather/Components/Forms/WeatherViewer.razor
<UICard>
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        <UIErrorHandler IsError="this.IsError" IsLoading="this.IsDataLoading" ErrorMessage="@this.RecordErrorMessage">
            <UIContainer>
            ..........
            </UIContainer>
        </UIErrorHandler>
        .......
    </Body>
```

##### UIContainer/UIRow/UIColumn

These controls create the BootStrap grid system - i.e. container, row and column - by building out DIVs with the correct Css.

```cs
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIContainer.cs
    public class UIContainer : UIBase
    {
        // Overrides the _BaseCss property to force the css_
        protected override string _BaseCss => "container-fluid";
    }
```


```cs
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIRow.cs
    public class UIRow : UIBase
    {
        protected override string _BaseCss => "row";
    }
```

```cs
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIColumn.cs
public class UIColumn : UIBase
{
    [Parameter]
    public int Columns { get; set; } = 1;

    protected override string _BaseCss => $"col-{Columns}";
}
```

```cs
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UILabelColumn.cs
public class UILabelColumn : UIColumn
{
    protected override string _BaseCss => $"col-{Columns} col-form-label";
}
```

Here's some code showing the controls in use.

```html
// CEC.Weather/Components/Forms/WeatherViewer.razor
<UIContainer>
    <UIRow>
        <UILabelColumn Columns="2">
            Date
        </UILabelColumn>
        ............
    </UIRow>
..........
</UIContainer>
```

### Wrap Up
This article provides an overview on how to build UI Controls with components, and examines some example components in more detail.  You can see all the library UIControls in the GitHub Repository - [CEC.Blazor/Components/UIControls](https://github.com/ShaunCurtis/CEC.Blazor/tree/master/CEC.Blazor/Components/UIControls)

Some key points to note:
1. UI Controls let you abstract markup from higher level components such as Forms and Views.
2. UI Controls give you control and allies some discipline over the HTML and CSS markup.
3. Your main View and Form components are much cleaner and easier to view.
4. You can use as little or as much abstraction as you wish.
5. Controls, such as `UIErrorHandler`, make life easier!

## History

* 21-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
