# Building a Blazor Project
# Part 2 - Data

In the first article I provided an overview of the way I build and structure my projects.  This article provides a detailed look at how I implement a multi-tiered data model in the project.

### The Entitiy Framework Tier

I use Entity Framework [EF] for database access. But, being old school (the application gets nowhere near my tables) I implement CUD [CRUD without the Read] through stored procedures, Read access data through views.  My data tier therefore has two layers - the EF Database Context and a Data Service.  In Blazor both of these are implemented as singleton services - all user requests go through the same objects.

The Entity Framework database account I use for web access has database access limited to the Views and Stored Procedures.

The demo application had no database so the EF layer isn't implemented.  If it was it would look like the code block below.  All EF code is impklemented in the project rather the the library.  The context and DBSets are all project specific.

There is a DBSet for each record that needed retrieval.  Each DBSet is linked to a view in *OnModelCreating()*.  The WeatherForecast application is very simple as we only have one record type.

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

### The Data Service Tier

I use generics through all the data layers - *TRecord* is the generic term I use for *T*.  I also define an Interface *IDbRecord* to TRecord to ensure all records implement certain properties and methods.

The data tier Interfaces live in the library, while the Base Service class - which uses Entity Framework - is project specific.

#### IDbRecord
  
```c#
public interface IDbRecord<T>
{
    public int ID { get; }

    public string DisplayName { get; }

    /// <summary>
    /// Creates a deep copy of the object
    /// </summary>
    /// <returns></returns>
    public T ShadowCopy(); 
}
```
These ensure:
* We can build a Id/Value pair for Select dropdowns if needed.
* We have a name to use in the title area of any control when displaying the record.
* We can make a copy of the record when needed during editing.

#### IDataService

IDataService defines the key functionality we need in DataServices

```c#
    public interface IDataService<TRecord> where TRecord : new()
    {
        /// <summary>
        /// Record Configuration Property
        /// </summary>
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// <summary>
        /// Method to get the Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetRecordListAsync() => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a Filtered Record List
        /// </summary>
        /// <returns></returns>
        public Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => Task.FromResult(new List<TRecord>());

        /// <summary>
        /// Method to get a Record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TRecord> GetRecordAsync(int id) => Task.FromResult(new TRecord());

        /// <summary>
        /// Method to update a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> UpdateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// method to add a record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public Task<DbTaskResult> AddRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        /// <summary>
        /// Method to delete a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DbTaskResult> DeleteRecordAsync(int id) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });
    }
```
Most of the properties and methods are self explanatory - core CRUD operations.  RecordConfiguration is a simple class that contains UI specific data such as the generic Record name and last name and the route name to get to the various UI component pages.   We'll see more about this in the UI article.

#### BaseDataService

IDataService defines the key functionality we need in DataServices

```c#
public abstract class BaseDataService<TRecord>: IDataService<TRecord> where TRecord : new()
{
    /// <summary>
    /// Access to the Application Configuration data
    /// </summary>
    public IConfiguration AppConfiguration { get; set; }

    /// <summary>
    /// Record Configuration data used by UI for display and navigation for records of type T
    /// </summary>
    public RecordConfigurationData RecordConfiguration { get; set; } = new RecordConfigurationData();

    public BaseDataService(IConfiguration configuration)
    {
        this.AppConfiguration = configuration;
    }

    // This would normally contain all the base boilerplate code for accessing the database context and doing CRUD operations
    // I'm old school and a little paranoid with data so link datasets to read only views for listing and viewing operations
    //  and use Stored Procedures and ExecuteSQLRawAsync for all CUD operations.

}
```

The code below show the code in a BaseService class implementing EF and using Stored Procedures for CUD. 

GetContext gets a DBcontext to execute the Stored Procedures on.  You need a Context per quiery, rather than a shared context because operations are async: there's no way of ensuring the shared context isn't in use when an async method wahts to use it.

```c#
protected WeatherForecastDbContext GetContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<BugTrackerDbContext>();

    optionsBuilder.UseSqlServer(AppConfiguration.GetConnectionString("BugTrackerConnection"));
    return new BugTrackerDbContext(optionsBuilder.Options);
}

```

*GetParameterizedSqlCommand* builds a SQL query from a set of parameters: We use *ExecuteSqlRawAsync* which doesn't take parameters.  This doesn't cover all options in *SQLParameter* so feel free to enhance it.  Maybe EF will change and give us parameterized Async Sql Executer one day.

```c#

protected string GetParameterizedSqlCommand(string storedprocname, List<SqlParameter> parameters)
{
    var paramstring = new StringBuilder();
    var quotedtypes = new List<SqlDbType>() { SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.NChar, SqlDbType.NText, SqlDbType.Text, SqlDbType.VarChar };
    var datetypes = new List<SqlDbType>() { SqlDbType.Date, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.SmallDateTime };

    foreach (var par in parameters)
    {
        if (paramstring.Length > 0) paramstring.Append(", ");
        if (quotedtypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
        else if (datetypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
        else paramstring.Append(string.Concat(par.ParameterName, "=", par.Value));

    }
    return $"exec {storedprocname} {paramstring.ToString()}";
}

```

There are two methods for executing Stored Procedures.  The first doesn't have a return value

```c#
protected async Task<DbTaskResult> RunDatabaseStoredProcedureAsync(string storedprocname, List<SqlParameter> parameters)
{
    var result = new DbTaskResult();
    var newsqlcommand = this.GetParameterizedSqlCommand(storedprocname, parameters);
    var rows = await this.GetContext().Database.ExecuteSqlRawAsync(newsqlcommand);
    if (rows == -1) result = new DbTaskResult() {Message = $"{this.Configuration.RecordDescription} saved", IsOK = true, Type= MessageType.Success, NewID = id };
    else result = new DbTaskResult() {Message = $"Error saving {this.Configuration.RecordDescription}", IsOK = false, Type= MessageType.Error, NewID = 0 };
    return result;
}
```


```c#

protected async Task<DbTaskResult> RunDatabaseIDStoredProcedureAsync(string sqlcommand)
{
    int id = 0;
    var result = new DbTaskResult();

    var rows = GetContext().IDs.FromSqlRaw(sqlcommand).AsAsyncEnumerable();
    await foreach (var row in rows) id = Convert.ToInt32(row.Id);
    if (id > 0) result = new DbTaskResult() {Message = $"{this.Configuration.RecordDescription} saved", IsOK = true, Type= MessageType.Success, NewID = id };
    else result = new DbTaskResult() {Message = $"Error saving {this.Configuration.RecordDescription}", IsOK = false, Type= MessageType.Error, NewID = 0 };
    return await Task.FromResult(result);
}
```


```c#

protected string GetParameterizedSqlCommand(string storedprocname, List<SqlParameter> parameters)
{
    var paramstring = new StringBuilder();
    var quotedtypes = new List<SqlDbType>() { SqlDbType.NVarChar, SqlDbType.Char, SqlDbType.NChar, SqlDbType.NText, SqlDbType.Text, SqlDbType.VarChar };
    var datetypes = new List<SqlDbType>() { SqlDbType.Date, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.SmallDateTime };

    foreach (var par in parameters)
    {
        if (paramstring.Length > 0) paramstring.Append(", ");
        if (quotedtypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
        else if (datetypes.Contains(par.SqlDbType)) paramstring.Append(string.Concat(par.ParameterName, "='", par.Value, "'"));
        else paramstring.Append(string.Concat(par.ParameterName, "=", par.Value));

    }
    return $"exec {storedprocname} {paramstring.ToString()}";
}

```


My DataService functionality is defined through an IDataService Interface which implements generics.  The TRecord geneneric object itself implements a IDBRecord Interface which defines common data object functionality.  We'll look at these later in more detail.  The IDataService core functionality is implemented in a boilerplate abstract BaseDataService class.  All higher level DataService inherit from this class.

```c#
```
