# Building Blazor UI Components

This article is the second half of earlier posted article [A Dive into Blazor Components](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).  

This article looks at how to build reusable UI components.  It's focus is on Bootstrap, but the principles are applicable to most  frameworks.

## ComponentBase or IComponent?

Use *ComponentBase* if you wish, but I prefer to implement *IComponent*.  You get more freedom, and a smaller footprint for what is often static content.  A static component can contain active components in it's *ChildContent*.  The Rendertree holds the structure, not individual components.  

### UIBase

The base component - the commenting explains what each Property/Method is for.  You will see the Css, tagging and content options in action in various real components.

```c#
public abstract class UIBase : IComponent
{
    #region Public Properties

    /// This is where we capture additional attributes that have been added to the Component
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; }

    /// This is the Component Content <UIBase>Child Content</UIBase>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// The component Tag - default is DIV - see_Tag
    [Parameter]
    public virtual string Tag { get; set; } = "div";

    /// Primary Css Style for component - see _Css_
    public virtual string Css { get; set; } = string.Empty;

    /// Additional Css to tag on the end of the base Css - see _Css
    [Parameter]
    public string AddOnCss { get; set; } = string.Empty;

    /// Boolean property that dictates if the component is rendered
    [Parameter]
    public virtual bool Show { get; set; } = true;

    #endregion

    #region Protected properties used by inheriting components

    /// A list of Html attributes that need to be removed from AdditionalAttributes
    /// default is just the class attribute
    protected List<string> UsedAttributes { get; set; } = new List<string>() { "class" };


    /// Html tag for the control - default is use the user set Tag.
    /// Set this to a value in order to override the Tag value in inherited class
    protected virtual string _Tag => this.Tag;

    /// Html style for the control - default is use the user set Css.
    /// Set this to a value in order to override the style value in inherited class
    protected virtual string _BaseCss => this.Css;

    /// Additional Html style for the control - default is use the user set AddOnCss.
    /// Set this to a value in order to override the AddOnCss value in inherited class
    protected virtual string _AddOnCss => this.AddOnCss;

    /// Actual Css string used in the component
    /// Built from the various pieces of Css
    protected virtual string _Css => this.CleanUpCss($"{this._BaseCss} {this._AddOnCss}");

    /// Show Property used by the builder - allows override of the Parameter set version
    protected virtual bool _Show => this.Show;

    /// Property to override the content of the component
    protected virtual string _Content => string.Empty;

    #endregion

    #region Internal class Variables

    /// Render Handle passed when Attach method called
    private RenderHandle _renderHandle;

    /// Render Fragment to render this object
    private readonly RenderFragment _componentRenderFragment;

    /// Boolean Flag to track if there's a pending render event queued
    private bool _RenderEventQueued;

    #endregion

    #region Class initialization/destruction Methods

    /// Class Initialization Event
    /// builds out the component renderfragment to pass to the Renderer when an render event is queued on the renderer
    public UIBase() => _componentRenderFragment = builder =>
    {
        this._RenderEventQueued = false;
        BuildRenderTree(builder);
    };

    #endregion

    #region IComponent Implementation
        
    /// Method called to attach the object to a RenderTree
    /// The render handle gives the component access to the renderer and its render queue
    public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

    /// Method called by the Renderer when one or more object parameters have been set or changed
    public virtual Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        StateHasChanged();
        return Task.CompletedTask;
    }

    #endregion

    #region Methods

    /// Method to queue a render event into the Renderer
    public void StateHasChanged()
    {
        if (!this._RenderEventQueued)
        {
            this._RenderEventQueued = true;
            _renderHandle.Render(_componentRenderFragment);
        }
    }

    /// Method to build a rendertree for the component
    protected virtual void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (this._Show)
        {
            this.ClearDuplicateAttributes();
            int i = -1;
            builder.OpenElement(i++, this._Tag);
            builder.AddMultipleAttributes(i++, AdditionalAttributes);
            builder.AddAttribute(i++, "class", this._Css);
            if (!string.IsNullOrEmpty(this._Content)) builder.AddContent(i++, (MarkupString)this._Content);
            else if (this.ChildContent != null) builder.AddContent(i++, ChildContent);
            builder.CloseElement();
        }
    }

    /// Method to clean up the Additional Attributes and remove any unwanted attributes
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

    /// Method to clean up the Css - remove leading and trailing spaces and any multiple spaces
    protected string CleanUpCss(string css)
    {
        while (css.Contains("  ")) css = css.Replace("  ", " ");
        return css.Trim();
    }

    #endregion
}
```
### UIBootstrapBase

*UIBootstrapBase* adds extra functionality specific to Bootstrap components. Fomratting options such a component colour and sizing is controlled by representing the options as Enums and building Css fragments based on the selected Enum.  

```c#
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
## Some Examples

Now we have the base classes lets look at some real component implementations.

##### UIButton

This is a standard Bootstrap Button. 
1. *ButtonType* and *ClickEvent* are specific to buttons.
2. *CssName* and *_Tag* are hardwired.
3. *ButtonClick* handles the button click event.
4. *BuildRenderTree* builds the markup and wires the JSInterop *onclick* event.
5. *Show* controls whether the button gets rendered.

```c#
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

##### UIAlert

This is a standard Bootstrap Alert. 
1. *Alert* is a class to encapsulate an Alert.
2. *ColourCssFragement*, *Show* and *_Content* are wired into the Alert object instance.

```c#
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

##### UIErrorHandler

This is an interesting component that deals with CRUD and other errors.  It's inherits directly from UIBase.  It has three states:
1. Loading when it displays the loading message and the spinner.
2. Error when it displays an error message.
3. Loaded when it displays the Child Content.

The state is controlled by the two boolean Parameters.  This means that the content is only accessed and rendered when the control knows there's data to render i.e. when *IsError* and *IsLoading* are both false.  We don't need to implement a lot of error checking in the child content.

```c#
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
        var i = -1;
        if (IsLoading)
        {
            builder.OpenElement(i++, "div");
            builder.AddAttribute(i++, "class", this._Css);
            builder.OpenElement(i++, "button");
            builder.AddAttribute(i++, "class", "btn btn-primary");
            builder.AddAttribute(i++, "type", "button");
            builder.AddAttribute(i++, "disabled", "disabled");
            builder.OpenElement(i++, "span");
            builder.AddAttribute(i++, "class", "spinner-border spinner-border-sm pr-2");
            builder.AddAttribute(i++, "role", "status");
            builder.AddAttribute(i++, "aria-hidden", "true");
            builder.CloseElement();
            builder.AddContent(i++, "  Loading...");
            builder.CloseElement();
            builder.CloseElement();
        }
        else if (IsError)
        {
            builder.OpenElement(i++, "div");
            builder.OpenElement(i++, "span");
            builder.AddAttribute(i++, "class", this._Css);
            builder.AddContent(i++, ErrorMessage);
            builder.CloseElement();
            builder.CloseElement();
        }
        else builder.AddContent(i++, ChildContent);
    }
}
```
