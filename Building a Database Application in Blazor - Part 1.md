# Building a Database Application in Blazor 
## Part 1 - Project Structure and Framework

This set of articles looks at how to build and structure a real Database Application in Blazor.  There are 5 articles:

1. Project Structure and Framework.
2. Services - Building the CRUD Data Layers.
3. View Components - Building the CRUD Presentation Layer.
4. UI Components - Building HTML/CSS Controls.
5. A walk through detailing how to add weather stations and weather station data to the application.

They document my current framework for developing Blazor Applications.

They are not:
1. An attempt to define best practice.
2. The finished product.

Use as much or as little as you like, and please offer suggestions.

I do make a few suggestions, principly to steer those new to Blazor away from dark holes.  As an example, I recommend parking the term Page.  A routed component isn't a page.  Label it a page, even subconsciously, and it will gain web page attributes that simply don't apply.    I use the term **Views** after the component that displays them - *RouteView*.

This first section walks runs through two projects on the GitHub repository - a Blazor Server and WASM/Backend API project - explaining the structure.

### Repository and Database

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

### Solution Structure

I use Visual Studio, so the Github repository consists of a solution with five projects.  These are:

1. CEC.Blazor - the core library containing everything that can be boilerplated and reused across any project.
2. CEC.Weather - this is the library shared by both the Server and WASM projests.  Everything that can be shared, but is project specific.  Examples are the EF DB Context, Model classes, model specific CRUD components, Bootstrap SCSS, ...
3. CEC.Blazor.Server - the Server project. The Routed Components with supporting code
4. CEC.Blazor.WASM.Server - the WASM backend server project.  The API Controllers with supporting code.
5. CEC.Blazor.WASM.Client - the WASM Client project.  The Routed Components with supporting code.

Most code is generic boilerplate stuff in the libraries. The actual projects are relatively small.

### CEC.Blazor.WASM.Client Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.WASM.Client.png?raw=true)

#### Routes (Views) 

The Pages directory is history. There's only one web page on the site - the index.html page in *wwwroot*.  Routed Views aren't pages, so don't refer to them as such!

The Routes directory contains all the Routed Views.  A typical routed view looks like this:

```html
@page "/WeatherForecasts"
@layout MainLayout
@namespace CEC.Blazor.WASM.Client.Routes
@inherits ApplicationComponentBase

<WeatherList UIOptions="this.UIOptions"></WeatherList>

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
*WeatherList* is a component used in both the Server and WASM projects.  It's WeatherForecast specific, so lives in the **CEC.Weather** project.

#### CSS

CSS is shared so is in *CEC.Weather*.  I use Bootstrap, but like to customize it a little so use SASS.  I have a SASS extension installed in Visual Studio to compile SASS files on the fly.

#### Program.cs

We load the CEC.Blazor, CEC.Routing and the local application services.
```c#
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("app");

    // Added here as we don't have access to builder in AddApplicationServices
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    // the Services for the CEC.Blazor Library
    builder.Services.AddCECBlazor();
    // the Services for the CEC.Routing Library
    builder.Services.AddCECRouting();
    // the local application Services defined in ServiceCollectionExtensions.cs
    builder.Services.AddApplicationServices();

    await builder.Build().RunAsync();
}
```
Services for each project/library are specified in *IServiceCollection* Extensions.

#### ServiceCollectionExtensions.cs

The site specific services loaded are the controller service *WeatherForecastControllerService* and the data service as an *IWeatherForecastDataService* interface loading  *WeatherForecastWASMDataService**.  The final transient service is the Fluent Validator for the Edit form.

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
#### Index.html

*Index.html* is lightly modified. 
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
    <script src="_content/CEC.Routing/cec.routing.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
</body>

</html>
```

1. The stylesheets are referenced by *_content/projectname*.  This is where linked projects/installed library *wwwroot* directories are anchored.
2. The scripts are referenced in the same way.
3. The *app* tag contains a startup HTML block that displays while the project is loading.

![App Start Screen](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/WASM-Start-Screen.png?raw=true)

### CEC.Blazor.WASM.Server Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.WASM.Server.png?raw=true)

The only files the server project contains, other than error handling for anyone trying to navigate to the site, are the WeatherForecast Controller and the startup/program files.

#### WeatherForecastController.cs

This is a standard API type controller.  It uses the registered *IWeatherForecastDataService* as it's data layer making async calls through the interface.

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

The site specific service is a singleton *IWeatherForecastDataService* interface loading *WeatherForecastServerDataService*.

```c#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
        return services;
    }
}
```

### CEC.Blazor.Server Project

![Project Files](https://github.com/ShaunCurtis/CEC-Publish/blob/master/Images/CEC.Blazor.Server.png?raw=true)

This project is very similar to *CEC.Blazor.WASM.Client*.

All the *Route* files are the same.  We can't "Share" them in *CEC.Weather* as they must exist in the project for the Router to recognise them as a route, and load them.  To "share" the files we would need to write a different router, that read the routes from a different source.

#### Pages

We have one real page *_Host.cshtml*. This is a bulk standard Blazor Server page.  CSS is linked through to the *CEC.Weather* shared project. 

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
    <link href="vendor/fontawesome-free/css/all.min.css" rel="stylesheet" type="text/css">
    <link href="https://fonts.googleapis.com/css?family=Nunito:200,200i,300,300i,400,400i,600,600i,700,700i,800,800i,900,900i" rel="stylesheet">
    <link rel="stylesheet" href="_content/CEC.Blazor/cec.blazor.min.css" />
    <link rel="stylesheet" href="_content/CEC.Weather/css/site.min.css" />
</head>
<body>
    <app>
        <component type="typeof(App)" render-mode="Server" />
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
    <script src="_content/CEC.Routing/cec.routing.js"></script>
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

#### Startup.cs

We're using the *CEC.Routing* and *CEC.Blazor* libraries, so load their associated services.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddCECBlazor();
    services.AddCECRouting();
    services.AddApplicationServices();
}
```

#### ServiceCollectionExtensions.cs

The site specific services are a singleton *IWeatherForecastDataService* interface loading *WeatherForecastServerDataService*, a scoped *WeatherForecastControllerService* and a transient Fluent Validator service for the Editor.

```c#
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
    services.AddScoped<WeatherForecastControllerService>();
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    return services;
}
```

### Wrap Up
That wraps up this section.  It's a bit of an overview, with the details to come later.  Hopefully it demonstrates the level of abstraction you can achieve with Blazor projects.  The next section looks in detail at Services and implementing the data layers.

Some key points to note:
1. The differences in code between a Blazor Server and a Blazor WASM project are very minor.
2. Be very careful about the terminology.  The use of the term "Pages" is a good example.
