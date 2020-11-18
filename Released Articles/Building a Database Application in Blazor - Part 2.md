# Part 2 - Services - Building the CRUD Data Layers

This article is the second in a series on Building Blazor Projects: it describes techniques and methodologies for abstracting the data and business logic layers into boilerplate code in a library.

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

## Repository and Database

[CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor)

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server version of the project running here](https://cec-blazor-server.azurewebsites.net/).

[You can see the WASM version of the project running here](https://cec-blazor-wasm.azurewebsites.net/).

## Services

Blazor is built on DI [Dependency Injection] and IOC [Inversion of Control].  If your not familiar with these concepts, do a little [backgound reading](https://www.codeproject.com/Articles/5274732/Dependency-Injection-and-IoC-Containers-in-Csharp) before diving into Blazor.  You'll save yourself time in the long run!

Blazor Singleton and Transient services are relatively straight forward.  You can read more about them in the [Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection).  Scoped are a little more complicated.

1. A scoped service object exists for the lifetime of a client application session - note client and not server.  Any application resets, such as F5 or navigation away from the application, resets all scoped services.  A duplicated tab in a browser creates a new application, and a new set of scoped services.
2. A scoped service can be scoped to an object in code.  This is most common in a UI conponent.  The `OwningComponentBase` component class has functionality to restrict the life of a scoped service to the lifetime of the component. This is covered in more detail in another article. 

Services is the Blazor IOC [Inversion of Control] container.

In Server mode services are configured in `startup.cs`:

```c#
// CEC.Blazor.Server/startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    // the Services for the CEC.Blazor .
    services.AddCECBlazor();
    // the local application Services defined in ServiceCollectionExtensions.cs
    services.AddApplicationServices(Configurtion);
}
```

```c#
// CEC.Blazor.Server/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Singleton service for the Server Side version of WeatherForecast Data Service 
    //services.AddSingleton<IWeatherForecastDataService, WeatherForecastDummyDataService>();
    services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
    // Scoped service for the WeatherForecast Controller Service
    services.AddScoped<WeatherForecastControllerService>();
    // Transient service for the Fluent Validator for the WeatherForecast record
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    // Factory that builds the specific DBContext 
    var dbContext = configuration.GetValue<string>("Configuration:DBContext");
    services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
    return services;
}
```

 and `program.cs` in WASM mode:

```c#
// CEC.Blazor.WASM.Client/program.cs
public static async Task Main(string[] args)
{
    .....
    // Added here as we don't have access to buildler in AddApplicationServices
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    // the Services for the CEC.Blazor Library
    builder.Services.AddCECBlazor();
    // the local application Services defined in ServiceCollectionExtensions.cs
    builder.Services.AddApplicationServices();
    .....
}
```

```c#
// CEC.Blazor.WASM.Client/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Scoped service for the WASM Client version of WeatherForecast Data Service 
    services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
    // Scoped service for the WeatherForecast Controller Service
    services.AddScoped<WeatherForecastControllerService>();
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    // Transient service for the Fluent Validator for the WeatherForecast record
    return services;
}
```
Points:
1. There's an `IServiceCollection` extension method for each project/library to encapsulate the specific services needed for the project.
2. Only the data layer service is different.  The Server version, used by both the Blazor Server and the WASM API Server, interfaces with the database and Entitiy Framework.  It's scoped as a Singleton - as we are running async, DbContexts are created and closed per query.  The Client version uses `HttpClient` (which is a scoped service) to make calls to the API and is therefore itself scoped.  There's also a dummy data service to emulate the database.
3. A code factory is used to build the specific DBContext, and provide the necessary level of abstraction for boilerplating the core data service code in the base library.

### Generics

The boilerplate library code relies heavily on Generics.  The two generic entities used are:
1. `TRecord` - this represents a model record class.  It must implement `IDbRecord`, a vanilla `new()` and be a class.
2. `TContext` - this is the database context and must inherit from the `DbContext` class.

Class declarations look like this:

```c#
// CEC.Blazor/Services/BaseDataClass.cs
public abstract class BaseDataService<TRecord, TContext>: 
    IDataService<TRecord, TContext>
    where TRecord : class, IDbRecord<TRecord>, new()
    where TContext : DbContext
{......}
```

### The Entity Framework Tier

The solution uses a combination of Entity Framework [EF] and normal database access. Being old school (the application gets nowhere near the data tables). I implement CUD [CRUD without the Read] through stored procedures, and R [Read access] and List through views.  The data tier has two layers - the EF Database Context and a Data Service.

The database account ued by Entity Framework database has access limited to select on Views and execute on Stored Procedures.

The demo application can be run with or without a full database connection - there's a "Dummy database" server Data Service.

All EF code is implemented in `CEC.Weather`, the shared project specific library.

#### WeatherForecastDBContext

The `DbContext` has a `DbSet` per record type.  Each `DbSet` is linked to a view in `OnModelCreating()`.  The WeatherForecast application has one record type.

The class looks like this:
```c#
// CEC.Weather/Data/WeatherForecastDbContext.cs
public class WeatherForecastDbContext : DbContext
{
    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : base(options) { }

    public DbSet<DbWeatherForecast> WeatherForecasts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DbWeatherForecast>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_WeatherForecast");
            });
    }
}
```
### The Data Service Tier

#### IDbRecord

`IDbRecord` defines the common interface for all database records.  
```c#
// CEC.Blazor/Data/Interfaces/IDbRecord.cs
public interface IDbRecord<T>
{
    public int ID { get; }

    public string DisplayName { get; }

    public T ShadowCopy(); 
}
```
IDbRecord ensures:
* An Id/Value pair for Select dropdowns.
* A default name to use in the title area of any control when displaying the record.
* A deep copy of the record when needed during editing.

#### IDataService

Core Data Service functionality is defined in the `IDataService` interface.

```c#
// CEC.Blazor/Services/Interfaces/IDataService.cs
 public interface IDataService<TRecord, TContext> 
        where TRecord : class, IDbRecord<TRecord>, new() 
        where TContext : DbContext
     {
        /// Used by the WASM client, otherwise set to null
        public HttpClient HttpClient { get; set; }

        /// Access to the DBContext using the IDbContextFactory interface 
       public IDbContextFactory<TContext> DBContext { get; set; }

        /// Access to the application configuration in Server
        public IConfiguration AppConfiguration { get; set; }

        /// Record Configuration object that contains routing and naming information about the specific record type
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// Method to get the full Record List
        public Task<List<TRecord>> GetRecordListAsync() => Task.FromResult(new List<TRecord>());

        /// Method to get a filtered Record List using a IFilterLit object
        public Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => Task.FromResult(new List<TRecord>());

        /// Method to get a single Record
        public Task<TRecord> GetRecordAsync(int id) => Task.FromResult(new TRecord());

        /// Method to get the current record count
        public Task<int> GetRecordListCountAsync() => Task.FromResult(0);

        /// Method to update a record
        public Task<DbTaskResult> UpdateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// method to create and add a record
        public Task<DbTaskResult> CreateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// Method to delete a record
        public Task<DbTaskResult> DeleteRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// Method to build the a list of SqlParameters for a CUD Stored Procedure.  Uses custom atrribute data.
        public List<SqlParameter> GetSQLParameters(TRecord item, bool withid = false) => new List<SqlParameter>();
    }
```

#### BaseDataService

`BaseDataService` implements the Interface

```c#
// CEC.Blazor/Services/Interfaces
public abstract class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : IDbRecord<TRecord>, new()
{
    // The HttpClient used by the WASM dataservice implementation - set to null by default - set in the WASM implementation
    public HttpClient HttpClient { get; set; } = null;

    // The DBContext access through the IDbContextFactory interface - set to null by default - set in the Server implementation
    public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

    // Access to the Application Configuration
    public IConfiguration AppConfiguration { get; set; }
    
    // Record Configuration - set in each specific model implementation
    public virtual RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

    // Base new
    public BaseDataService(IConfiguration configuration) => this.AppConfiguration = configuration;
    }
```
#### BaseServerDataService

See the [project code](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/BaseServerDataService.cs) for the full class - it's rather long.

The service implements boilerplate code:
1. Implement the `IDataService` interface CRUD methods.
1. Async Methods to build out the Create, Update and Delete Stored Procedures.
2. Async Methods to get lists and individual records using EF DbSets. 

The code relies on either:
* using naming conventions
  * Model class names Db`RecordName` - e.g. `DbWeatherForecast`.
  * DbContext DbSet properties named `RecordName` - e.g. `WeatherForecast`.  
* using custom attributes.
  * `DbAccess` - class level attribute to define the Stored Procedure names.
  * `SPParameter` - Property specific attribute to mark all properties used in the Stored Procedures.

A short section of the DbWeatherForecast model class is shown below decorated with the custom attributes. 

```c#
[DbAccess(CreateSP = "sp_Create_WeatherForecast", UpdateSP ="sp_Update_WeatherForecast", DeleteSP ="sp_Delete_WeatherForecast") ]
public class DbWeatherForecast :IDbRecord<DbWeatherForecast>
{
    [SPParameter(IsID = true, DataType = SqlDbType.Int)]
    public int WeatherForecastID { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.SmallDateTime)]
    public DateTime Date { get; set; } = DateTime.Now.Date;
    ......
}
```
Data operations on EF are implemented as extension methods on `DBContext`.

Stored Procedures are run by calling `ExecStoredProcAsync()`.  The method is shown below.  It uses the EF DBContext to get a normal ADO Database Command Object, and then executes the Stored Procedure with a parameter set built using the custom attributes from the Model class. 

```c#
// CEC.Blazor/Extensions/DBContextExtensions.cs
public static async Task<bool> ExecStoredProcAsync(this DbContext context, string storedProcName, List<SqlParameter> parameters)
{
    var result = false;

    var cmd = context.Database.GetDbConnection().CreateCommand();
    cmd.CommandText = storedProcName;
    cmd.CommandType = CommandType.StoredProcedure;
    parameters.ForEach(item => cmd.Parameters.Add(item));
    using (cmd)
    {
        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch {}
        finally
        {
            cmd.Connection.Close();
            result = true;
        }
    }
    return result;
}
```
Using Create as an example.

```c#
// CEC.Blazor/Services/DBServerDataService.cs
public async Task<DbTaskResult> CreateRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Create);
```
See the comments for information
```c#
// CEC.Blazor/Services/DBServerDataService.cs
protected async Task<DbTaskResult> RunStoredProcedure(TRecord record, SPType spType)
{
    // Builds a default error DbTaskResult
    var ret = new DbTaskResult()
    {
        Message = $"Error saving {this.RecordConfiguration.RecordDescription}",
        IsOK = false,
        Type = MessageType.Error
    };

    // Gets the correct Stored Procedure name.
    var spname = spType switch
    {
        SPType.Create => this.RecordInfo.CreateSP,
        SPType.Update => this.RecordInfo.UpdateSP,
        SPType.Delete => this.RecordInfo.DeleteSP,
        _ => string.Empty
    };
    
    // Gets the Parameters List
    var parms = this.GetSQLParameters(record, spType);

    // Executes the Stored Procedure with the parameters.
    // Builds a new Success DbTaskResult.  In this case (Create) it retrieves the new ID.
    if (await this.DBContext.CreateDbContext().ExecStoredProcAsync(spname, parms))
    {
        var idparam = parms.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
        ret = new DbTaskResult()
        {
            Message = $"{this.RecordConfiguration.RecordDescription} saved",
            IsOK = true,
            Type = MessageType.Success
        };
        if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
    }
    return ret;
}
```
You can dig into the detail of `GetSqlParameters` in the [GitHub Code File](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/BaseServerDataService.cs).

The Read and List methods get the DbSet name through reflection, and use EF methodology and the `IDbRecord` interface to get the data.

```c#
// CEC.Blazor/Extensions/DBContextExtensions

public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
{
    var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
    var set = par.GetValue(context);
    var sets = (DbSet<TRecord>)set;
    return await sets.ToListAsync();
}

public async static Task<int> GetRecordListCountAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
{
    var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
    var set = par.GetValue(context);
    var sets = (DbSet<TRecord>)set;
    return await sets.CountAsync();
}

public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>
{
    var par = context.GetType().GetProperty(dbSetName ?? IDbRecord<TRecord>.RecordName);
    var set = par.GetValue(context);
    var sets = (DbSet<TRecord>)set;
    return await sets.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).ID == id);
}
```

#### BaseWASMDataService

See the [project code](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/BaseWASMDataService.cs) for the full class.

The client version of the class is relatively simple, using the `HttpClient` to make API calls to the server.  Again we rely on naming conventions for boilerplating to work.

Using Create as an example.

```c#
// CEC.Blazor/Services/DBWASMDataService.cs
public async Task<DbTaskResult> CreateRecordAsync(TRecord record)
{
    var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{RecordConfiguration.RecordName}/create", record);
    var result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
    return result;
}
```

We'll look at the Server Side Controller shortly.

### Project Specific Implementation

For abstraction purposes we define a common data service interface.  This implements no new functionality, just specifies the generics.
```c#
// CEC.Weather/Services/Interfaces/IWeatherForecastDataService.cs
public interface IWeatherForecastDataService : 
    IDataService<DbWeatherForecast, WeatherForecastDbContext>
{
    // Only code here is to build dummy data set
}
```

The WASM service inherits from `BaseWASMDataService` and implements `IWeatherForecastDataService`.  It defines the generics and configures the `RecordConfiguration`.

```c#
// CEC.Weather/Services/WeatherForecastWASMDataService.cs
public class WeatherForecastWASMDataService :
    BaseWASMDataService<DbWeatherForecast, WeatherForecastDbContext>,
    IWeatherForecastDataService
{
    public WeatherForecastWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
    {
        this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
    }
}
```

The Server service inherits from `BaseServerDataService` and implements `IWeatherForecastDataService`.  It defines the generics and configures the `RecordConfiguration`.

```c#
// CEC.Weather/Services/WeatherForecastServerDataService.cs
public class WeatherForecastServerDataService :
    BaseServerDataService<DbWeatherForecast, WeatherForecastDbContext>,
    IWeatherForecastDataService
{
    public WeatherForecastServerDataService(IConfiguration configuration, IDbContextFactory<WeatherForecastDbContext> dbcontext) : base(configuration, dbcontext)
    {
        this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
    }
}
```

### The Business Logic/Controller Service Tier

Controllers are normally configured as Scoped Services.

The controller tier interface and base class are generic and reside in the CEC.Blazor library.  Two interfaces `IControllerService` and `IControllerPagingService` define the required functionality.  Both are implemented in the BaseControllerService class.  

The code for the [IControllerService](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/Interfaces/IControllerService.cs), [IControllerPagingService](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/Interfaces/IControllerPagingService.cs) and [BaseControllerService](https://github.com/ShaunCurtis/CEC.Blazor/blob/master/CEC.Blazor/Services/BaseControllerService.cs) are too long to show here.  We'll cover most of the functionality when we look at how the UI layer interfaces with the controller layer.

The main functionality implemented is:

1. Properties to hold the current record and recordset and their status.
2. Properties and methods - defined in `IControllerPagingService` - for UI paging operations on large datasets.
4. Properties and methods to sort the the dataset.
3. Properties and methods to track the edit status of the record (Dirty/Clean).
4. Methods to implement CRUD operations through the IDataService Interface.
5. Events triggered on record and record set changes.  Used by the UI to control page refreshes.
7. Methods to reset the Controller during routing to a new page that uses the same scoped instance of the controller.

All code needed for the above functionality is boilerplated in the base class.  Implementing specific record based controllers is a simple task with minimal coding.

#### WeatherForecastControllerService

The class:
 
1. Implements the class constructor that gets the required DI services, sets up the base class and sets the default sort column for db dataset paging and sorting.
2. Gets the Dictionary object for the Outlook Enum Select box in the UI.

Note that the data service used is the `IWeatherForecastDataService` configured in Services.  For WASM this is `WeatherForecastWASMDataService` and for Server or the API EASM Server this is `WeatherForecastServerDataService`.
```c#
// CEC.Weather/Controllers/ControllerServices/WeatherForecastControllerService.cs
public class WeatherForecastControllerService : BaseControllerService<DbWeatherForecast, WeatherForecastDbContext>, IControllerService<DbWeatherForecast, WeatherForecastDbContext>
{
    /// List of Outlooks for Select Controls
    public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

    public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IWeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
    {
        this.Service = weatherForecastDataService;
        this.DefaultSortColumn = "WeatherForecastID";
    }
}
```

#### WeatherForecastController

While it's not a service, the `WeatherForecastController` is the final bit of the data layers to cover.  It uses `IWeatherForecastDataService` to access it's data service and makes the same calls as the ControllerService into the DataService to access and return the requested data sets.  I've not found a way yet to abstract this, so we need to implement one per record.
 
```c#
// CEC.Blazor.WASM.Server/Controllers/WeatherForecastController.cs
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

### Wrap Up
This article demonstrates how to abstract the data and controller tier code into a library.

Some key points to note:
1. Aysnc code is used wherever possible.  The data access functions are all async.
2. Generics make much of the boilerplating possible.  They create complexity, but are worth the effort.
3. Interfaces are crucial for Dependancy Injection and UI boilerplating.

The next section looks at the [Presentation Layer / UI Framework](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C).

## History

* 15-Sep-2020: Initial version.
* 2-Oct-2020: Minor formatting updates and typo fixes.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
