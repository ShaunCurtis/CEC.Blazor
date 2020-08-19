# Building a Blazor Project
# Part 1 - Overview and Project Structure

Any programmer worth his salt knows that a well organised and structured  project is worth the initial investment.  Once you know a development framework, you develop libraries and a structure that makes development quicker and easier.  While you don't start from scratch, a move to fresh fields means a lot to learn.  With that in mind, this set of articles describes my organisation and coding strategies for Blazor Server applications.  Note the MY, this is not a "Best Practice" manual.  I'm an old timer: I may have kept up with the times, but my scars make me opinionated.  I'm sharing knowledge in the hope it will help you make your own more informed decisions.

## Some Key Concepts to Understand with Blazor

##### Blazor is a Single Page Application [SPA]
An SPA is a pseudo-application running in a browser.  Navigate to an SPA, and that first page load is the only browser page load event that takes place.  Everything now runs through that page.  What looks like navigation to a new page isn't: it's just changes and re-rendering of the DOM in the original page.  So get it through your head that moving around an SPA is routing - not navigation - whatever terminology or class names are used. Navigate to a new browser page and you destroy the application and any unsaved data with it.  An F5 page refresh closes and re-opens the application.

##### Blazor implements Dependency Inject[DI] and Inversion of Control [IOC] Principles

If you don't understand these concepts do some reading: don't shy away from services and DI.  Blazor Services is the Blazor IOC container.  Any object can run as a service: there's no Interface to implement.  However, you can only define only service per class or interface.  The service scope defines how many copies get run.  The three types of service have different life cycles:
1. **Singleton** - A Blazor application instance has only copy of a singleton service object.  It's shared by all users.  It only gets reset when the Blazor server instance is reset.  Use a singleton service where you are sharing the same resource, such as access to a database.
2. **Scoped** - a scoped service exists while the user's application instance exists.  Don't think of this as a ASP.Net session.  If your unclear re-visit the section above on the life cycle of a SPA. An F5 page refresh resets a scoped service object.  Navigate forward from an SPA and then back and scoped services are reset.  Duplicate the Tab in the browser and the new window gets a new unique scoped service object.  Use a scoped service where you want each user session to be unique.  You can apply a restriction to scoped services by using OwningComponentBase instead of ComponentBase for your top level component.  You can then register which services get destroyed when the component goes out of scope.
3. **Transient** - transient services are unique per request.  Two components requesting a transient service will each get a unique instance.  Once the requesting object goes out of the scope so does the transient service object.

Services are managed by the Services Container and created and destroyed as required.  As a programmer you just use them, leave their management to the Services Container.  Services get other services through dependancy injection at startup: be careful you can't inject a scoped or transient service into a singleton service, nor a transient service into a scoped service.

##### Blazor is DotNet Core and Razor

It's new, but 90+% is existing DotNet Core and Razor. You don't need to re-learn everything.

## Some Guiding Principles

1. Use the classic three tier model - Data (Data), Business Logic (Controller), Presentation (UI).
2. Separate formatting code [HTML] from program code [C#]
3. Boilerplate everything you.
4. Use a separate library for all shared/multi-project code.
4. SASS for CSS.
5. Async all the way. 

Lets examine these in detail.

#### Three Tier Data Model

##### Data Tier

I use Entity Framework [EF] for database access.  I don't use the full functionality of EF because I'm old school: the application gets nowhere near my tables.  CUD [CRUD without the Read] through stored procedures, Read access data through views. My data tier therefore has two layers - the EF Database Context and a Data Service.  In Blazor both of these are implemented as singleton services - all user requests go through the same objects.

My DataService functionality is defined through an IDataService Interface which implements generics.  The TRecord geneneric object itself implements a IDBRecord Interface which defines common data object functionality.  We'll look at these later in more detail.  The IDataService core functionality is implemented in a boilerplate abstract BaseDataService class.  All higher level DataService inherit from this class.

##### Business Logic Tier

I implement all the business logic in ControllerService objects.  The core functionality is again defined in a generic IControllerService Interface, and boilerplated in a BaseControllerService class.        

##### Presentation Tier

In Blazor this is the Component Model.  All components inherit from BaseComponent.  Needless to say, I implement a lot of boilerplating here.  More later.

#### Separate Formatting and Programming Code

This is probably personal.  I hate to see C# code all jumbled up with HTML.  So I:

1. Use code behind with .razor and .razor.cs.
2. Build components to implement HTML blocks.  A little pedantic, but I don't even like to see *if* statements with HTML.  I build a component with a Show boolean parameter.
3. Use RenderTreeBuilder on small components.

To demonstrate, I have UITag component that builds a Div HTML element.
```c#
public class UITag : UIBase
{
    /// <summary>
    /// Property to set the Tag of the HTML element
    /// </summary>
    [Parameter]
    public virtual string Tag { get; set; } = "div";

    /// <summary>
    /// Override this Protected property to set the Tag in inherited classes.  Default implementation sets it to the Tag Parameter Property set by the user (default is div)
    /// </summary>
    protected override string _Tag => this.Tag;
}
```
For brevity I've only included the BuildRenderTree method of the base UIBase 
```c#
public class UITag : UIBase
{
public abstract class UIBase : ComponentBase
{
    .........
    protected override void BuildRenderTree(RenderTreeBuilder builder)
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
    ...............
}
```
This will be covered in more detail in a later article.

#### BoilerPlate Everything

It more costly now, but the future savings are often enormous.  It's probably top of most programmers list of things to do if you could program a time machine.  That and spend your salary on Amazon, Apple and Microsoft shares rather than in Pizza Express!

You will already have seen my preoccupation with boilerplating in the two sections above.  I love a nice sleek spartan top level UI component.  Look at the WeathForecast List Page Component below.  There's too little code to do a code behind page.  Why make the WeatherList a component rather than the top level page?  You might want to use it as a sub-component in another page or display it in a dialog. 
```c#
@page "/WeatherForecast"
@page "/WeatherForecasts"

@namespace CEC.Blazor.Server.Pages

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

#### Use a Separate Library for all Shared/Multi-Project Code

I know it seems obvious, but it is amazing how often you see common code copied between projects.  It takes disipline to constantly question where you're writing code.  The example project has most of the project code in the CEC.Blazor library.  Only project specific stuff lives in the project.


#### SASS for CSS

I'm a relatively new convert to SASS.  I use Bootstrap a lot, but want to customize it to get a more unique feel.  The sample project associated with this article changes things like the main colour scheme, and the button rounding.  You get one minimized css file.  I use the Web Compiler VS extension.  The SCSS files all live in project directory and compile down to a single CSS file in *wwwroot/css*.  You can see the setup in the sample project.

#### ASYNC All The Way

Blazor components fully implement async functions for the component life cycle.  Entity Framework supports async calls.  There's no reason not to write async code.  For a more detailed look at Async Programming in Blazor read my article [Understanding and Using Async Programming in DotNetCore and Blazor](https://www.codeproject.com/Articles/5276310/Understanding-and-Using-Async-Programming-in-DotNe).

That's wraps up this first article.  The next will look in detail at implmenting the data model.

#### Further Reading and References

The main Microsoft Docs Site is an excellent source of information:
[Introduction to ASP.NET Core Blazor - Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

Key Sections:

1. [ASP.NET Core Blazor lifecycle](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)

2. [Dependency injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)

3. [ASP.NET Core Blazor routing](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing)


[ChrisSainty has a blog](https://chrissainty.com/) where he delves more deeply into specific areas in Blazor.

[As Does Michael Washington](https://blazorhelpwebsite.com/)








