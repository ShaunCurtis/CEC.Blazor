# Boilerplating a Database Application in Blazor 
## Part 1 - Project Structure and Framework

This set of articles looks at how to build and structure a real Database Application in Blazor.  There are 4 articles:

1. Project Structure and Framework
2. Services
3. Routed View Components
4. UI Components

They are not:
1. A definition of Best Practice - I'm not in that league!
2. A finished product.

It really a state of playis a framework and methodology I've developed writing Blazor Applications.  Use as much or as little of it as you like, and please offer suggestions to improve it.  I do make a few recommendations, principly to try and break old routines.  For example, don't call a routed component a page.  It's not, call it a page and you subconciously attribute other web page features to it that simply don't apply.    I call these **Routed Views** after the component that displays them - *RouteView*. 

There's a [GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

This first section walks runs through two projects on the GitHub repository - a Blazor Server and WASM/Backend API project.


### Solution Structure

I use Visual Studio, so the Github repository consists of a solution and a set of logical projects.  These are:

1. CEC.Blazor - the core library containing everything that's boilerplated and reusable across Server and WASM projects.
2. CEC.Weather - this is the library shared by both the WeatherForecast Server and WASM projests.  Everything that can be shared, but is project specific, lives in here.  Examples are the EF DB Context, Model classes, model specific CRUD components, Bootstrap SCSS, ...
3. CEC.Blazor.Server - the WeatherForecast Server project. The Routed Components with supporting code
4. CEC.Blazor.WASM.Server - the WASM backend server project.  The API Controllers with supporting code.
5. CEC.Blazor.WASM.Client - the WASM Client project.  The Routed Components with supporting code.

A key point to note here is that the actual projects are relatively small.  Most of the code is generic boilerplate code in the libraries.  The rest of this article walks through the Server and WASM sites.

### CEC.Blazor.WASM.Client Project

![Project Files](images/CEC.Blazor.WASM.client.png)

#### Routes (Pages) 

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

CSS is shared so is in *CEC.Weather*.  I use Bootstrap, but like to customize it a little so use SASS.

#### Program.cs

Services for each project/library are specified in IServiceCollection Extensions.  We load the CEC.Blazor, CEC.Routing and the local application services.
```c#
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddApplicationServices();
    builder.Services.AddCECBlazor();
    builder.Services.AddCECRouting();

    await builder.Build().RunAsync();
}
```
#### ServiceCollectionExtensions.cs

The site specific services loaded are the controller service *WeatherForecastControllerService* and the data service as an *IWeatherForecastDataService* interface loading  *WeatherForecastWASMDataService**.  The final transient service is the Fluent Validator for the Edit form.

```c#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<BrowserService>();
        services.AddScoped<WeatherForecastControllerService>();
        services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
        services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
        return services;
    }
}
```
#### Index.html

*Index.html* is lightly modified to 
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
Points to note:

1. The stylesheets are referenced by *_content/projectname*.  This is where linked projects/installed library *wwwroot* directories are anchored.
2. The scripts are referenced in the same way.
3. The *app* tag contains a startup HTML block that displays while the project is loading.

![App Start Screen](images/WASM-Start-Screen.png)

### CEC.Blazor.WASM.Client Project

![Project Files](images/CEC.Blazor.WASM.Server.png)

The only files the server project contains, other than error handling for anyone trying to navigate to the site, are the WeatherForecast Controller and the startup/program files.

#### WeatherForecastController.cs

This is a standard API type controller.  It uses the registered *IWeatherForecastDataService* as it's data layer making async calls through the interface.

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

![Project Files](images/CEC.Blazor.Server.png)

This project is very similar to *CEC.Blazor.WASM.Client*.  Before I highlight the differences, look at what is the same.

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
That wraps up this section.  It demonstrates a framework for building the data access section of a project and how to boilerplate most of the code through interfaces and bas classes.  The next section looks at the Presentation Layer / UI framework.

Some key points to note:
1. Aysnc code is used wherever possible.  The data access functions are all async.
2. The use of generics to make much of the boilerplating possible.
3. Using Interfaces for Dependancy Injection and UI boilerplating.
