# Building a Database Application in Blazor 
# Part 1 - Project Structure and Framework

This set of articles looks at how to build and structure a real Database Application in Blazor.  There are 6 articles:

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

They document my current framework for developing Blazor Applications.

They are not:
1. An attempt to define best practice.
2. The finished product.

Use as much or as little as you like, and please offer suggestions.

I do make a few suggestions, principally to steer those new to Blazor away from dark holes.  As an example, I recommend parking the term `Page`.  A routed component isn't a page.  Label it a page, even subconsciously, and it gains web page attributes that simply don't apply.    I use the term **Views** after the component that displays them - `RouteView`.

This first section describes my somewhat radical development approach and walks runs through two projects on the GitHub repository - a Blazor Server and WASM/Backend API project - explaining the structure.

## Repository and Database

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server version of the project running here](https://cec-blazor-server.azurewebsites.net/).

[You can see the WASM version of the project running here](https://cec-blazor-wasm.azurewebsites.net/).

## Solution Structure

I use Visual Studio, so the Github repository consists of a solution with five projects.  These are:

1. CEC.Blazor - the core library containing everything that can be boilerplated and reused across any project.
2. CEC.Weather - this is the library shared by both the Server and WASM projests.  Almost all the project code lives here.  Examples are the EF DB Context, Model classes, model specific CRUD components, Bootstrap SCSS, Views, Forms, ...
3. CEC.Blazor.Server - the Server project.
4. CEC.Blazor.WASM.Server - the WASM backend server project.  The API Controllers with supporting code.
5. CEC.Blazor.WASM.Client - the WASM Client project.

## Design Philosophy

The data side of the project is structured fairly conventionally, loosely based on the three tier - data, logical and presentation layer - model.  The exposed data classes all run as Services and are available for dependency injection.  The detail is covered in the second article.

The UI is more radical:

1. A custom `Component` class is used as the base UI component - `ControlBase` is only used by the out-of-the-box data controls.
2. Routing is discarded.  There's a new `ViewManager` that manages the UI.

## UI Structure

### Pages

Pages are the web pages that act as the host for the the application.  There's normally one per application.

### ViewManager

The `ViewManager` is the sub-root component in the RenderTree, loaded by App.  It's purpose is to manage and load Views.  The supporting `ViewData` class is used to store View configuration data.  The main  method used is `LoadViewAsync`.  There are various versions, but all load the View defined in `ViewData`.  `ViewManager` exposes itself to all other components through a cascading value.

### Views

Views are the components loaded into the page by `ViewManager`.  They must implement `IView` and can define a Layout.

```c#
    public interface IView : IComponent
    {
        /// provides a unique reference for the instance of the view
        public Guid GUID => Guid.NewGuid();

        /// The cascaded ViewManager Instance
        [CascadingParameter] public ViewManager ViewManager { get; set; }
    }
```

### Layouts

Layouts are the Blazor out-of-the-box Layouts.  The ViewManager renders the Layout with the `View` as the child content.

### Forms

Forms are logical collections of controls that are either displayed in a view or a modal dialog.  Lists, view forms, edit forms are all classic forms. Forms contain controls not HTML.

### Controls

Controls are components that display something: they emit HTML code.  For example, an edit box, a dropdown, button, ... A Form is a collection of controls.

## CEC.Blazor.WASM.Client Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.WASM.Client-2.png?raw=true)

The project is almost empty.  The controls and services are all in the libraries.

### index.html

`index.html` is almost standard issue with:

1. Added stylesheet references.  Note that you use the virtual directory `_content/Assembly_Name` to access content exposed in the `wwwroot` folders of dependency assembles.  Scripts are accessed in the same way.
2. The base content of `app` is an HTML block that displays a spinner while the application is initializing. 

![App Start Screen](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/WASM-Start-Screen.png?raw=true)

```html
<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>CEC.Blazor.WASM</title>
    <base href="/" />
    <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,300,300i,400,400i,600,600i,700,700i,800,800i,900,900i" rel="stylesheet">
    <link rel="stylesheet" href="_content/CEC.Blazor/cec.blazor.min.css" />
    <link  rel="stylesheet" href="_content/CEC.Weather/css/site.min.css" />
</head>

<body>
    <app>
        <div class="mt-4" style="margin-right:auto; margin-left:auto; width:100%;" >
            <div class="loader"></div>
            <div style="width:100%; text-align:center;"><h4>Web Application Loading</h4></div>
        </div>
    </app>

    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
    <script src="_content/CEC.Blazor/site.js"></script>
</body>

</html>
```
### CSS

All CSS is shared, so lives in `CEC.Weather`.  I use Bootstrap, customized a little with SASS.  I have the WEB COMPILER extension installed in Visual Studio to compile SASS files on the fly.

### Program.cs

`CEC.Blazor` and local application services are loaded.  Note the Blazor root component is defined here.  You don't have to use `App`.

```c#
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    // Full class name used here to make it a little more obvious.  Not required if the assemble is referenced.
    builder.RootComponents.Add<CEC.Weather.Components.App>("app");

    // Added here as we don't have access to builder in AddApplicationServices
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    // the Services for the CEC.Blazor Library
    builder.Services.AddCECBlazor();
    // the local application Services defined in ServiceCollectionExtensions.cs
    builder.Services.AddApplicationServices();

    await builder.Build().RunAsync();
}
```
Services for each project/library are specified in `IServiceCollection` Extensions.

#### ServiceCollectionExtensions.cs

The site specific services loaded are the controller service `WeatherForecastControllerService` and the data service as an `IWeatherForecastDataService` interface loading  `WeatherForecastWASMDataService`.  The final transient service is the Fluent Validator for the Edit form.

```c#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Scoped service for the WASM Client version of WeatherForecast Data Service 
        services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
        // Scoped service for the WeatherForecast Controller Service
        services.AddScoped<WeatherForecastControllerService>();
        // Transient service for the Fluent Validator for the WeatherForecast record
        services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
        return services;
    }
}
```

### CEC.Blazor.WASM.Server Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.WASM.Server-2.png?raw=true)

The only files in the server project, other than error handling for anyone trying to navigate to the site, are the WeatherForecast Controller and the startup/program files.

#### WeatherForecastController.cs

This is a standard API type controller.  It uses the registered `IWeatherForecastDataService` as it's data layer making async calls through the `IWeatherForecastDataService` interface.

```c#
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        protected IWeatherForecastDataService DataService { get; set; }

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastDataService weatherForecastDataService)
        {
            this.DataService = weatherForecastDataService;
            this.logger = logger;
        }

        [MVC.Route("weatherforecast/list")]
        [HttpGet]
        public async Task<List<DbWeatherForecast>> GetList() => await DataService.GetRecordListAsync();

        [MVC.Route("weatherforecast/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync();

        [MVC.Route("weatherforecast/get")]
        [HttpGet]
        public async Task<DbWeatherForecast> GetRec(int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherforecast/read")]
        [HttpPost]
        public async Task<DbWeatherForecast> Read([FromBody]int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherforecast/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherForecast record) => await DataService.UpdateRecordAsync(record);

        [MVC.Route("weatherforecast/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherForecast record) => await DataService.CreateRecordAsync(record);

        [MVC.Route("weatherforecast/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherForecast record) => await DataService.DeleteRecordAsync(record);
    }
```

#### ServiceCollectionExtensions.cs

The site specific service is a singleton `IWeatherForecastDataService` interface loading either `WeatherForecastServerDataService` or `WeatherForecastDummyDataService`. `WeatherForecastDummyDataService` is for demo purposes and doesn't need a backend SQL database. It does what it says, creates an in-application dataset.  

```c#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // You have a choice of data sources.
        // services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
        services.AddSingleton<IWeatherForecastDataService, WeatherForecastDummyDataService>();
        // Factory for building the DBContext 
        var dbContext = configuration.GetValue<string>("Configuration:DBContext");
        services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
        return services;
    }
}
```

### CEC.Blazor.Server Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.Server-2.png?raw=true)

This project is almost identical to `CEC.Blazor.WASM.Client`.

#### Pages

We have one real page - the standard issue `_Host.cshtml`.  As we're running on the server this is an asp.netcore page.
1. Added stylesheet references.  Note that you use the virtual directory `_content/Assembly_Name` to access content exposed in the `wwwroot` folders of dependency assembles.  Scripts are accessed in the same way.
2. The base content of `app` uses a TagHelper to load the root component - in this case `CEC.Weather.Components.App`.  Again, you're not tied to App, just specify a different component class. 

```c#
@page "/"
@namespace CEC.Blazor.Server.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = null;
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>CEC.Blazor.Server</title>
    <base href="~/" />
    <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,300,300i,400,400i,600,600i,700,700i,800,800i,900,900i" rel="stylesheet">
    <link rel="stylesheet" href="_content/CEC.Blazor/cec.blazor.min.css" />
    <link rel="stylesheet" href="_content/CEC.Weather/css/site.min.css" />
</head>
<body>
    <app>
        <component type="typeof(CEC.Weather.Components.App)" render-mode="Server" />
    </app>

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    <script src="_framework/blazor.server.js"></script>
    <script src="_content/CEC.Blazor/site.js"></script>
</body>
</html>
```

#### Startup.cs

The local services and `CEC.Blazor` library services are added.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddCECBlazor();
    services.AddApplicationServices();
}
```

#### ServiceCollectionExtensions.cs

The site specific services are a singleton `IWeatherForecastDataService` interface loading either `WeatherForecastServerDataService` or `WeatherForecastDummyDataService`, a scoped `WeatherForecastControllerService` and a transient Fluent Validator service for the Editor.

```c#
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    //services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
    services.AddSingleton<IWeatherForecastDataService, WeatherForecastDummyDataService>();
    services.AddScoped<WeatherForecastControllerService>();
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    return services;
}
```

### Wrap Up
That wraps up this section.  It's a bit of an overview, with a lot more detail to come later.  Hopefully it demonstrates the level of abstraction you can achieve with Blazor projects.  The next section looks at Services and implementing the data layers.

Some key points to note:
1. Almost all the code is common across Blazor Server and Blazor WASM projects.  With care you can write an application that can be deployed either way.
2. Be very careful about the terminology.  We don't have "Pages" in the application.

## History

* 15-Sep-2020: Initial version
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.

