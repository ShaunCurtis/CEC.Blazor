# Boilerplating a Database Application in Blazor 
## Part 1 - CRUD Data Access and Logic Operations

This article provides a detailed look methodolgies to build CRUD Application projects based on multi-tiered data models.

This part describes methodologies for boilerplating the data and business logic layers.  A second article demonstrates methodologies for boilerplating the presentation layer.

The article uses a demonstration solution, with code split between a library where (almost) all the reusable classes reside, and the project where project specific code resides.  It relies heavily on inheritance.  The developement process and code refactoring should always seek to migrate code downwards through the inheritance tree, and from specific (project) code to generic (library) code.  While this may seem like stating the obvious - yes "we" always do it - it's amazing how often what we preach is not what we implement.  I'm sure people will point out code in the demo project where I'm as guilty as everyone else. 


### The Entity Framework Tier

I use Entity Framework [EF] for database access. But, being old school (the application gets nowhere near my tables) I implement CUD [CRUD without the Read] through stored procedures, and R [Read access] through views.  My data tier has two layers - the EF Database Context and a Data Service.  In Blazor both of these are implemented as singleton services - all user requests go through the same objects.

The Entity Framework database account has access limited to Select on the Views and Execute on the Stored Procedures.

The demo application can be run with or without a database. All EF code is implemented in the project rather the the library - it's project specific.

#### WeatherForecastDBContext

The class constructor looks like this:
```c#
protected WeatherForecastDbContext GetContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<WeatherForecastDbContext>();

    optionsBuilder.UseSqlServer(AppConfiguration.GetConnectionString("WeatherForecastConnection"));
    return new WeatherForecastDbContext(optionsBuilder.Options);
}

```

The DBContext has a DB DataSet for each record.  Each DB DataSet is linked to a view in *OnModelCreating()*.  The WeatherForecast application is very simple as it only have one record type.

```c#
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
