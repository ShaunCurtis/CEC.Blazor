# Buildinging a Database Appication in Blazor 
## Part 4 - UI Components

Part 3 described the techniques and methodologies for building boilerplate CRUD UI Components in a library.  This article takes a more general look at UI components and how to structure them.

### Sample Project and Code

See the [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) for the libraries and sample projects.

### Components

For a detailed look at components read my Article [A Dive into Blazor Compoents](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).

I divide components into four categories:
1. Views - these are routed components/views.
2. Forms - there can be one or more forms within a View, and one or more UIControls within a form.  Edit/Display/List components are all forms.
3. UIControls - these are collections of HTML markup and CSS. Similar to Form Controls.
4. Layouts - these a special components used to layout a View.

### Views

Views are specific to the application.  in my applications Views live in the *Routes* folder.

The Weather Forecast Viewer and List Views are shown below.
```html
// CEC.Blazor.Server/Routes/WeatherForecastViewer.cs
@page "/WeatherForecast/View"

@namespace CEC.Blazor.Server.Pages

@inherits ApplicationComponentBase

<WeatherViewer></WeatherViewer>
```
The list view defines a UIOptions object that control various list control display options.
```html
// CEC.Blazor.Server/Routes/WeatherForecasts.cs
@page "/WeatherForecast"

@layout MainLayout

@namespace CEC.Blazor.Server.Routes

@inherits ApplicationComponentBase

<WeatherList UIOptions="this.UIOptions" ></WeatherList>

@code {
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

Forms are also Project specific.  In the Weather Application they reside in the CEC.Weather library as they are used by both the Server and WASM projects.  

The code below shows the Weather Viewer.  It's all UI Controls, no HTML markup.  The markup lives inside the controls - we'll look at some example UI Controls later.

```html
// CEC.Weather/Components/Forms/WeatherViewer.razor
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

The code behind page is relatively simple - the complexity is in the boilerplate code in *RecordComponentBase*.  It loads the record specific Controller service.

```C#
// CEC.Weather/Components/Forms/WeatherViewer.razor.cs
public partial class WeatherViewer : RecordComponentBase<DbWeatherForecast>
{
    public partial class WeatherViewer : RecordComponentBase<DbWeatherForecast, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherForecastControllerService ControllerService { get; set; }

        public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

        protected async override Task OnInitializedAsync()
        {
            this.Service = this.ControllerService;
            await base.OnInitializedAsync();
        }

        protected void NextRecord(int increment) 
        {
            var rec = (this._ID + increment) == 0 ? 1 : this._ID + increment;
            rec = rec > this.Service.BaseRecordCount ? this.Service.BaseRecordCount : rec;
            this.NavManager.NavigateTo($"/WeatherForecast/View?id={rec}");
        }
    }
}
```

### UI Controls

The application uses UI Controls to separate HTML and CSS markup from Views and Forms.  Bootstrap is used as the UI Framework.

##### UIBase

All UI Controls inherit from *UIBase*.  This implements *IComponent*.  It doesn't inherit from *ComponentBase* because we don't need the complexity it implements.   The complete class is too long to show - you can view it [here](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Components/UIControls/UI/UIBase.cs).

It builds a HTML DIV block that you can turn on or off.

##### UIBootstrapBase

*UIBootstrapBase* adds extra functionality for Bootstrap components. Formatting options such a component colour and sizing are represented as Enums, and Css fragments built based on the selected Enum.

```c#
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

##### UIButton

This is a standard Bootstrap Button. 
1. *ButtonType* and *ClickEvent* are specific to buttons.
2. *CssName* and *_Tag* are hardwired.
3. *ButtonClick* handles the button click event.
4. *BuildRenderTree* builds the markup and wires the JSInterop *onclick* event.
5. *Show* controls whether the button gets rendered.

```c#
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
            var i = -1;
            builder.OpenElement(i++, this._Tag);
            builder.AddAttribute(i++, "type", this.ButtonType);
            builder.AddAttribute(i++, "class", this._Css);
            builder.AddAttribute(i++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, this.ButtonClick));
            builder.AddContent(i++, ChildContent);
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
1. *Alert* is a class to encapsulate an Alert.
2. *ColourCssFragement*, *Show* and *_Content* are wired into the Alert object instance.

```c#
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

This component deals with loading operations and errors.  It's inherits directly from UIBase.  It has three states:
1. Loading when it displays the loading message and the spinner.
2. Error when it displays an error message.
3. Loaded when it displays the Child Content.

The state is controlled by the two boolean Parameters.  Content is only accessed and rendered when the control knows there's data to render i.e. when *IsError* and *IsLoading* are both false.  This saves implementing a lot of error checking in the child content.

```c#
// CEC.Blazor/Components/UIControls/UI/UIErrorHandler.cs
public class UIErrorHandler : UIBase
{
    /// Boolean Property that determines if the child content or an error message is diplayed
    [Parameter]
    public bool IsError { get; set; } = false;

    /// Boolean Property that determines if the child content or an loading message is diplayed
    [Parameter]
    public bool IsLoading { get; set; } = true;

    /// CSS Override
    protected override string _BaseCss => this.IsLoading? "text-center p-3": "label label-error m-2";

    /// Customer error message to display
    [Parameter]
    public string ErrorMessage { get; set; } = "An error has occured loading the content";

        
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        this.ClearDuplicateAttributes();
        if (IsLoading)
        {
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
        }
        else if (IsError)
        {
            builder.OpenElement(1, "div");
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", this._Css);
            builder.AddContent(4, ErrorMessage);
            builder.CloseElement();
            builder.CloseElement();
        }
        else builder.AddContent(1, ChildContent);
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

Thess creates a BootStrap Container, Row and Column.  They build out DIVs with the correct Css.

```c#
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIContainer.cs
    public class UIContainer : UIBase
    {
        // Overrides the _BaseCss property to force the css_
        protected override string _BaseCss => "container-fluid";
    }
```


```c#
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIRow.cs
    public class UIRow : UIBase
    {
        protected override string _BaseCss => "row";
    }
```

```c#
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UIColumn.cs
public class UIColumn : UIBase
{
    [Parameter]
    public int Columns { get; set; } = 1;

    protected override string _BaseCss => $"col-{Columns}";
}
```

```c#
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
This article gives an overview on building UI Controls with components, and examines some example components in more detail.

Some key points to note:
1. Using this methodology, you have control over the HTML and Css markup.
2. You can use as little or as much abstraction as you wish.

