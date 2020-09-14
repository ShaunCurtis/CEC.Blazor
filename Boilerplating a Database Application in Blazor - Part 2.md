# Boilerplating a Database Application in Blazor 
## Part 2 - Services - Building the CRUD Data Layers

This article is the second in a series on Building Blazor Projects: it describes techniques and methodologies for boilerplating the data and business logic layers.

There's a [GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) for the libraries and sample projects.

### Services

Blazor is built around DI [Dependency Injection] and IOC [Inversion of Control].  If your not familiar with these concepts, you should do a little backgound reading before diving into Blazor.

Blazor Singleton and Transient services are relatively straight forward.  Scoped a little more complicated.

1. A scoped service object exists for the lifetime of a client application session - note client and not server.  Any resets of the application, such as F5 or navigation away from the application, resets the application.  A duplicated tab in a browser creates a new application.
2. A scoped service can be "scoped" in code.  This is most commonly done in a UI conponent.  The *OwningComponentBase* component class has functionality to restrict the life of a scoped service to the lifetime of the component. This will be discussed in further detail n the next article. 

Services is the Blazor IOC [Inversion of Control] container.  In Server mode it's configured in *startup.cs*:

```c#
// startup.cs for CEC.Blazor.Server
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    // the Services for the CEC.Blazor .
    services.AddCECBlazor();
    // the Services for the CEC.Routing Library
    services.AddCECRouting();
    // the local application Services defined in ServiceCollectionExtensions.cs
    services.AddApplicationServices(Configurtion);
}

// ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Singleton service for the Server Side version of WeatherForecast Data Service 
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

 and *program.cs* in WASM:

```c#
// program.cs for CEC.Blazor.WASM.Client
public static async Task Main(string[] args)
{
    .....
    // Added here as we don't have access to buildler in AddApplicationServices
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    // the Services for the CEC.Blazor Library
    builder.Services.AddCECBlazor();
    // the Services for the CEC.Routing Library
    builder.Services.AddCECRouting();
    // the local application Services defined in ServiceCollectionExtensions.cs
    builder.Services.AddApplicationServices();
    .....
}

// ServiceCollectionExtensions.cs
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
1. There's an *IServiceCollection* extension method for each project/library to encapsulate the specific services needed by each project.
2. Only the data layer service is different.  The Server version, used by both the Blazor Server and the WASM API Server, contains the code that interfaces with the database and Entitiy Framework, and is a Singleton.  You only need one.  The Client version needs access to *HttpClient* which is a scoped service and is therefore itself scoped.
3. There's a Factory coded process to build the specific DBContext for the application.  With this, the core data service code can be abstracted to the base library.

### The Entity Framework Tier

The solution uses a combination of Entity Framework [EF] and normal database access. Being old school (the application gets nowhere near my tables) I implement CUD [CRUD without the Read] through stored procedures, and R [Read access] through views.  My data tier has two layers - the EF Database Context and a Data Service.

The database account ued by Entity Framework database has access limited to select on Views and execute on Stored Procedures.

The demo application can be run with or without a full database connection - there's a "Dummy database" server Data Service.

All EF code is implemented in *CEC.Weather* the shared project specific library.

#### WeatherForecastDBContext

The DBContext has a DB DataSet per record type.  Each DataSet is linked to a view in *OnModelCreating()*.  The WeatherForecast application, being very simple, has only have one record type.

The class looks like this:
```c#
// CEC.Weather/Data
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
                eb.ToView("vw_WeatherForecasts");
            });
    }
}
```

Core Data Service functionality is defined in the *IDataService* interface.

```c#
// CEC.Blazor/Services/Interfaces
 public interface IDataService<TRecord, TContext> 
        where TRecord : IDbRecord<TRecord>, new() 
        where TContext : DbContext
     {
        /// Used by the WASM client, otherwise set to null
        public HttpClient HttpClient { get; set; }

        /// Access to the DBContext using the IDbContextFactory interfce 
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

*BaseDataService* provides a base class implementing the Interface

```c#
// CEC.Blazor/Services/Interfaces
public abstract class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : IDbRecord<TRecord>, new()
{
    public HttpClient HttpClient { get; set; } = null;

    public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

    public IConfiguration AppConfiguration { get; set; }

    public virtual RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

    public BaseDataService(IConfiguration configuration) => this.AppConfiguration = configuration;
    }
```
**** BaseServerDataService

See the project code for the full class.

The *BaseServerDataService* class implements boilerplate functionality for building out and executing the CUD Stored Procedures.
1. Async Methods for Create, Update and Delete.
2. Methods to extract the Stored Procedure parameters fro the model data class
3. Methods to build and execute the Stored Procedures.

Read and list methods are model specific, so implemented in the CEC.Weather library. 

Model classes use two custom attributes for Stored Procedure markup:
1. *DbAccess* - class level to define the Stored Procedure names.
2. *SPParameter* - Property specific.  Label all properties used in the Stored Procedures.

A short section of the DbWeatherForecast model class is shown below. 

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
Stored Procedures are run by calling the *ExecStoredProcAsync()* extension method on the *DBContext*.  The method is shown below.  It uses the EF DBContext to get a normal ADO Database Command Object, and then executes the Stored Procedure with a parameter set built using the custom attributes from the Model class.

```c#
// CEC.Blazor/Extensions/DBContextExtensions
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















The Stored Procedure base method is in the DBContext and uses *ExecuteSqlRawAsync* to run all the stored procedures.  Note that Entity Framework for DotNet Core doesn't support many of the features in the DotNet version, so it's back to basics.

There's a single method for executing all stored procedures.

```c#
internal async Task<DbTaskResult> RunStoredProcedureAsync(string storedprocname, List<SqlParameter> parameters, RecordConfigurationData recordConfiguration)
{
    var rows = await this.Database.ExecuteSqlRawAsync(this.GetParameterizedNames(storedprocname, parameters), parameters);
    if (rows == 1)
    {
        var idparam = parameters.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
        var ret = new DbTaskResult()
        {
            Message = $"{recordConfiguration.RecordDescription} saved",
            IsOK = true,
            Type = MessageType.Success
        };
        if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
        return ret;
    }
    else return new DbTaskResult()
    {
        Message = $"Error saving {recordConfiguration.RecordDescription}",
        IsOK = false,
        Type = MessageType.Error
    };
}
```
*GetParameterizedNames* builds the SQL query with the parameters.  Note the use of output parameters to get the new ID.

```c#
protected string GetParameterizedNames(string storedprocname, List<SqlParameter> parameters)
{
    var paramstring = new StringBuilder();

    foreach (var par in parameters)
    {
        if (paramstring.Length > 0) paramstring.Append(", ");
        if (par.Direction == ParameterDirection.Output) paramstring.Append($"{par.ParameterName} output");
        else paramstring.Append(par.ParameterName);
    }
    return $"exec {storedprocname} {paramstring}";
}
```

### The Data Service Tier

Generics are used throughout the data layers - *TRecord* is used as the generic term for *T*.  It should always implement *IDbRecord* and *new* to ensure all records implement certain properties and methods.

The data tier Interfaces reside in the library, while the Base Service class - which uses Entity Framework - is project specific.

#### IDbRecord
  
```c#
public interface IDbRecord<T>
{
    public int ID { get; }

    public string DisplayName { get; }

    public T ShadowCopy(); 
}
```
IDbRecord ensure:
* We can build a Id/Value pair for Select dropdowns if needed.
* We have a name to use in the title area of any control when displaying the record.
* We can make a copy of the record when needed during editing.

#### IDataService

IDataService defines the key functionality we need in DataServices

```c#
public interface IDataService<TRecord> where TRecord : new()
{
    public RecordConfigurationData RecordConfiguration { get; set; }

    public Task<List<TRecord>> GetRecordListAsync() => Task.FromResult(new List<TRecord>());

    public Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => Task.FromResult(new List<TRecord>());

    public Task<TRecord> GetRecordAsync(int id) => Task.FromResult(new TRecord());

    public Task<int> GetRecordListCountAsync() => Task.FromResult(0);

    public Task<DbTaskResult> UpdateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public Task<DbTaskResult> AddRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public Task<DbTaskResult> DeleteRecordAsync(int id) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public List<SqlParameter> GetSQLParameters(TRecord item, bool withid = false) => new List<SqlParameter>();
}
```
Most of the properties and methods are self explanatory - core CRUD operations.

RecordConfiguration is a simple class that contains UI specific data such as the generic Record name and last name and the route name to get to the various UI component pages.   We'll see more about this in the UI article.

#### BaseDataService

IDataService defines the key functionality we need in DataServices

```c#
public abstract class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : new()
{
    public IConfiguration AppConfiguration { get; set; }

    public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

    public BaseDataService(IConfiguration configuration)
    {
        this.AppConfiguration = configuration;
    }
}
```

The code below show the code in a BaseService class implementing EF and using Stored Procedures for CUD. 

```c#
protected WeatherForecastDbContext GetContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<WeatherForecastDbContext>();

    optionsBuilder.UseSqlServer(AppConfiguration.GetConnectionString("WeatherForecastConnection"));
    return new WeatherForecastDbContext(optionsBuilder.Options);
}
```

GetContext gets a DBcontext to execute the Stored Procedures on.  You need a new context per query, rather than a shared context, as operations are async: you don't know when an async method wants to use it or release it.

#### WeatherForecastDataService

This is the specific data service for WeatherForecast records.

The class new method configures the RecordConfiguration and passes the necessary injected objects to the base class.
```c#
public WeatherForecastDataService(IConfiguration configuration) : base(configuration)
{
    this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherForecast", RecordDescription = "Weather Forecast", RecordListName = "WeatherForecasts", RecordListDecription = "Weather Forecasts" };
}
```

The main interface declarations are all async methods using the dbcontext to access the records.  *DbTaskResult* is a custom class to pass information back up to the UI.

```c#
public async Task<DbWeatherForecast> GetRecordAsync(int id) => await this.GetContext().WeatherForecasts.FirstOrDefaultAsync(item => item.WeatherForecastID == id);

public async Task<List<DbWeatherForecast>> GetRecordListAsync() => await this.GetContext().WeatherForecasts.ToListAsync();

public async Task<DbTaskResult> UpdateRecordAsync(DbWeatherForecast record) => await this.GetContext().RunStoredProcedureAsync("sp_Update_WeatherForecast", this.GetSQLParameters(record, true), this.RecordConfiguration);

public async Task<DbTaskResult> AddRecordAsync(DbWeatherForecast record) => await this.GetContext().RunStoredProcedureAsync("sp_Add_WeatherForecast", this.GetSQLParameters(record, false), this.RecordConfiguration);

public async Task<DbTaskResult> DeleteRecordAsync(int id)
{
    var parameters = new List<SqlParameter>() {
    new SqlParameter() {
        ParameterName =  "@WeatherForecastID",
        SqlDbType = SqlDbType.Int,
        Direction = ParameterDirection.Input,
        Value = id }
    };
    return await this.GetContext().RunStoredProcedureAsync("sp_Delete_WeatherForecast", parameters, this.RecordConfiguration);
}

private List<SqlParameter> GetSQLParameters(DbWeatherForecast item, bool isinsert = false)
{
    var parameters = new List<SqlParameter>() {
    new SqlParameter("@Date", SqlDbType.SmallDateTime) { Direction = ParameterDirection.Input, Value = item.Date.ToString("dd-MMM-yyyy") },
    new SqlParameter("@TemperatureC", SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = item.TemperatureC },
    new SqlParameter("@Frost", SqlDbType.Bit) { Direction = ParameterDirection.Input, Value = item.Frost },
    new SqlParameter("@SummaryValue", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = item.SummaryValue },
    new SqlParameter("@OutlookValue", SqlDbType.Int) { Direction = ParameterDirection.Input, Value = item.OutlookValue },
    new SqlParameter("@Description", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.Description },
    new SqlParameter("@PostCode", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.PostCode },
    new SqlParameter("@Detail", SqlDbType.NVarChar) { Direction = ParameterDirection.Input, Value = item.Detail },
    };
    if (isinsert) parameters.Insert(0, new SqlParameter("@WeatherForecastID", SqlDbType.BigInt) { Direction = ParameterDirection.Input, Value = item.ID });
    else parameters.Insert(0, new SqlParameter("@WeatherForecastID", SqlDbType.BigInt) { Direction = ParameterDirection.Output });
    return parameters;
}
```

### The Business Logic/Controller Service Tier

Again *TRecord* generic is used throughout the controller layerdata layers.  Controllers are normally scoped as Scoped Services and then further restricted using OwningComponentBase in the UI when needed.

The controller tier interface and base class are library classes.  Two interfaces *IControllerService* and *IControllerPagingService* defined the required functionality.  Both are implemented in the BaseControllerService class.  The code for the interface and base class are too long to show here.  We'll cover most of the functionality when we look at how the UI layer interfaces with the controller layer.

The main functionality implemented is:

1. Properties to hold the current record and recordset and their status.
2. Properties and methods - defined in *IControllerPagingService* - for UI paging operations on large datasets.
4. Properties and methods to sort the the dataset.
3. Properties and methods to track the edit status of the record (Dirty/Clean).
4. Methods to implement CRUD operations through the IDataService Interface.
5. Events triggered on record and record set changes.  Used by the UI to control page refreshes.
7. Methods to reset the Controller during routing to a new page that uses the same scoped instance of the controller.

Almost all code needed for the above functionality is boiler plated in the base class.  Implementing specific record based controllers is a simple task with minimal coding.

#### WeatherForecastControllerService

This is where the bolier plate code in the base class pays dividends.  The specific WeatherForecast record implementation of the Controller Service:
 
1. Implements the class constructor that gets the required DI services, sets up the base class and sets the default sort column for db dataset paging and sorting.
3. Gets the Dictionary object for the Outlook Enum Select box in the UI.

```c#
public class WeatherForecastControllerService : BaseControllerService<DbWeatherForecast>, IControllerService<DbWeatherForecast>
{
    public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

    public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, WeatherForecastDataService weatherForecastDataService) : base(appconfiguration, navmanager)
    {
        this.Service = weatherForecastDataService;
        this.DefaultSortColumn = "WeatherForecastID";
    }
}
```

It's pretty simple and straightforward.

### Wrap Up
That wraps up this section.  It demonstrates a framework for building the data access section of a project and how to boilerplate most of the code through interfaces and bas classes.  The next section looks at the Presentation Layer / UI framework.

Some key points to note:
1. Aysnc code is used wherever possible.  The data access functions are all async.
2. The use of generics to make much of the boilerplating possible.
3. Using Interfaces for Dependancy Injection and UI boilerplating.
