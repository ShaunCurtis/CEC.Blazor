# Boilerplating a Database Application in Blazor 
## Part 1 - Project Structure and Framework

This article looks at how to structure a Database Application in Blazor.  There's a sample solution that implements the strategies discussed in both Server and WASM projects.  This is not:
1. A definition of Best Practice - I'm not in that league!
2. A finished product.

It is a framework and methodology I've developed writing Blazor Applications.  Use as much or as little of it as you like, and please offer suggestions to improve it.  I do make a few recommendations, principly to try and break old routines.  For example, don't call a routed component a page.  It's not, call it a page and you subconciously attribute other web page features to it that simply don't apply. 

### Solution Structure

I use Visual Studio, so there's a solution split into a set of logical projects.  These are:

1. CEC.Blazor - the core library containing everything that's boilerplated and reusable across Server and WASM projects.
2. CEC.Weather - this is the library shared by both the WeatherForecast Server and WASM projests.  Everything that can be shared, but is project specific, lives in here.  Examples are the EF DB Context, Model classes, model specific CRUD components, Bootstrap SCSS, ...
3. CEC.Blazor.Server - the WeatherForecast Server project. The Routed Components with supporting code
4. CEC.Blazor.WASM.Server - the WASM backend server project.  The API Controllers with supporting code.
5. CEC.Blazor.WASM.Client - the WASM Client project.  The Routed Components with supporting code.

The key to note here is that the actual projects are relatively small.  Most of the code is generic boilerplate code in the libraries.  The rest of this article walks through the Server and WASM sites.

### CEC.Blazor.WASM.Client

![Project Fules](images/CEC.Blazor.WASM.client.png)

#### Pages 

The Pages directory is history. There's only one web page on the site - the index.html page in *wwwroot*.  Routed Components aren't pages, so don't refer to them as such!

Routes comes in.  All routed Components live in Routes.A typical Routed Component looks like this:

```html
@page "/WeatherForecasts"
@layout MainLayout
@namespace CEC.Blazor.WASM.Client.RoutedComponents
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
*WeatherList* is a shared component - used in both the Server and WASM projects.  It's WeatherForecast specific, so lives in **CEC.Weather**.

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

The site specific services are:

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
5. *Index.html* is a lightly modified to 
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

6. 










### Wrap Up
That wraps up this section.  It demonstrates a framework for building the data access section of a project and how to boilerplate most of the code through interfaces and bas classes.  The next section looks at the Presentation Layer / UI framework.

Some key points to note:
1. Aysnc code is used wherever possible.  The data access functions are all async.
2. The use of generics to make much of the boilerplating possible.
3. Using Interfaces for Dependancy Injection and UI boilerplating.
