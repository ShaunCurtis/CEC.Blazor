# Part 6 - Adding new Record Types and Their UI to the Weather Application

This is the sixth article in the series and walks through adding new records to the Weather Application.  The articles so far are:


1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. Adding New Record Types and the UI to Weather Projects

The purpose of the exercise is to import station data from the UK Met Office.  There's an command line importer project included in the solution to fetch and import the data - review the code to see how it works.  The data is in the form of monthly records from British Weather Stations going back to 1928.  We'll add two record types:

* Weather Station
* Weather Report from Stations

And all the infrastructure to provide UI CRUD operations for these two records.

As we're building both Server and WASM deployments, we have 4 projects to which we add code:
1. **CEC.Weather** - the shared project library
2. **CEC.Blazor.Server** - the Server Project
3. **CEC.Blazor.WASM.Client** - the WASM project
4. **CEC.Blazor.WASM.Server** - the API server for the WASM project

The majority of code is library code in CEC.Weather.

## Sample Project and Code

The base code is here in the [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Weather).
The completed code for this article is in [CEC.Weather GitHub Repository](https://github.com/ShaunCurtis/CEC.Weather).

## Overview of the Process

1. Add the Tables, Views and Stored Procedures to the Database
2. Add the Models, Services and Forms to the CEC.Weather Library
3. Add the Views and configure the Services in the Blazor.CEC.Server project.
4. Add the Views and configure the Services in the Blazor.CEC.WASM.Client project.
5. Add the Controllers and configure the Services in the Blazor.CEC.WASM.Server project.

## Database

Add tables for each record type to the database.

```sql
CREATE TABLE [dbo].[WeatherStation](
	[WeatherStationID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Latitude] [decimal](8, 4) NOT NULL,
	[Longitude] [decimal](8, 4) NOT NULL,
	[Elevation] [decimal](8, 2) NOT NULL
)
```
```sql
CREATE TABLE [dbo].[WeatherReport](
	[WeatherReportID] [int] IDENTITY(1,1) NOT NULL,
	[WeatherStationID] [int] NOT NULL,
	[Date] [smalldatetime] NULL,
	[TempMax] [decimal](8, 4) NULL,
	[TempMin] [decimal](8, 4) NULL,
	[FrostDays] [int] NULL,
	[Rainfall] [decimal](8, 4) NULL,
	[SunHours] [decimal](8, 2) NULL
)
```

Add views for each record type.

Note:
1. They both map `ID` and `DisplayName` to map to `IDbRecord`.
2. WeatherReport maps Month and Year to allow SQL Server filtering on those fields.
3. WeatherReport contains a JOIN to provide `WeatherStationName` in the record.  

```sql
CREATE VIEW vw_WeatherStation
AS
SELECT        
    WeatherStationID AS ID, 
    Name, 
    Latitude, 
    Longitude, 
    Elevation, 
    Name AS DisplayName
FROM WeatherStation
```
```sql
CREATE VIEW vw_WeatherReport
AS
SELECT        
    R.WeatherReportID as ID, 
    R.WeatherStationID, 
    R.Date, 
    R.TempMax, 
    R.TempMin, 
    R.FrostDays, 
    R.Rainfall, 
    R.SunHours, 
    S.Name AS WeatherStationName, 
    'Report For ' + CONVERT(VARCHAR(50), Date, 106) AS DisplayName
    MONTH(R.Date) AS Month, 
    YEAR(R.Date) AS Year
FROM  WeatherReport AS R 
LEFT INNER JOIN dbo.WeatherStation AS S ON R.WeatherStationID = S.WeatherStationID
```

Add Create/Update/Delete Stored Procedures for each record type 
```sql
CREATE PROCEDURE sp_Create_WeatherStation
	@ID int output
    ,@Name decimal(4,1)
    ,@Latitude decimal(8,4)
    ,@Longitude decimal(8,4)
    ,@Elevation decimal(8,2)
AS
BEGIN
INSERT INTO dbo.WeatherStation
           ([Name]
           ,[Latitude]
           ,[Longitude]
           ,[Elevation])
     VALUES (@Name
           ,@Latitude
           ,@Longitude
           ,@Elevation)
SELECT @ID  = SCOPE_IDENTITY();
END
```
```sql
CREATE PROCEDURE sp_Update_WeatherStation
	@ID int
    ,@Name decimal(4,1)
    ,@Latitude decimal(8,4)
    ,@Longitude decimal(8,4)
    ,@Elevation decimal(8,2)
AS
BEGIN
UPDATE dbo.WeatherStation
	SET 
           [Name] = @Name
           ,[Latitude] = @Latitude
           ,[Longitude] = @Longitude
           ,[Elevation] = @Elevation
WHERE @ID  = WeatherStationID
END
```
```sql
CREATE PROCEDURE sp_Delete_WeatherStation
	@ID int
AS
BEGIN
DELETE FROM WeatherStation
WHERE @ID  = WeatherStationID
END
```
```sql
CREATE PROCEDURE sp_Create_WeatherReport
	@ID int output
    ,@WeatherStationID int
    ,@Date smalldatetime
    ,@TempMax decimal(8,4)
    ,@TempMin decimal(8,4)
    ,@FrostDays int
    ,@Rainfall decimal(8,4)
    ,@SunHours decimal(8,2)
AS
BEGIN
INSERT INTO WeatherReport
           ([WeatherStationID]
           ,[Date]
           ,[TempMax]
           ,[TempMin]
           ,[FrostDays]
           ,[Rainfall]
           ,[SunHours])
     VALUES
           (@WeatherStationID
           ,@Date
           ,@TempMax
           ,@TempMin
           ,@FrostDays
           ,@Rainfall
           ,@SunHours)
SELECT @ID  = SCOPE_IDENTITY();
END
```
```sql
CREATE PROCEDURE sp_Update_WeatherReport
	@ID int output
    ,@WeatherStationID int
    ,@Date smalldatetime
    ,@TempMax decimal(8,4)
    ,@TempMin decimal(8,4)
    ,@FrostDays int
    ,@Rainfall decimal(8,4)
    ,@SunHours decimal(8,2)
AS
BEGIN
UPDATE WeatherReport
   SET [WeatherStationID] = @WeatherStationID
      ,[Date] = @Date
      ,[TempMax] = @TempMax
      ,[TempMin] = @TempMin
      ,[FrostDays] = @FrostDays
      ,[Rainfall] = @Rainfall
      ,[SunHours] = @SunHours
WHERE @ID  = WeatherReportID
END
```
```sql
CREATE PROCEDURE sp_Delete_WeatherReport
	@ID int
AS
BEGIN
DELETE FROM WeatherReport
WHERE @ID  = WeatherReportID
END
```

All the SQL, including two weather station datasets, is available as a set of files in the SQL folder of the GitHub Repository.

## CEC.Weather Library

We need to:
1. Add the model classes for each record type.
2. Add some utility classes specific to the project.  In this instance we:
    * Add some extensions to `decimal` to display our fields correctly (Latitude and Longitude).
    * Add custom validators for the editors for each record type.
3. Update the WeatherForecastDBContext to handle the new record types.
4. Build specific Controller and Data Services to handle each record type.
5. Build specific List/Edit/View Forms for each record type.
6. Update the NavMenu component.

### Add Model Classes for the Records

1. We implement IDbRecord.
2. We add the SPParameter custom attribute to all the properties that map to the Stored Procedures.
3. We decorate Properties that are not mapped to the Database View with `[Not Mapped]`.

```c#
// CEC.Weather/Model/DbWeatherStation.cs
public class DbWeatherStation :
    IDbRecord<DbWeatherStation>
{
    [NotMapped]
    public int WeatherStationID { get => this.ID; }
    
    [SPParameter(IsID = true, DataType = SqlDbType.Int)]
    public int ID { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.VarChar)]
    public string Name { get; set; } = "No Name";

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,4)")]
    public decimal Latitude { get; set; } = 1000;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,4)")]
    public decimal Longitude { get; set; } = 1000;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,2)")]
    public decimal Elevation { get; set; } = 1000;

    public string DisplayName { get; set; }

    [NotMapped]
    public string LatLong => $"{this.Latitude.AsLatitude()} {this.Longitude.AsLongitude()}";

    public void SetNew() => this.ID = 0;

    public DbWeatherStation ShadowCopy()
    {
        return new DbWeatherStation() {
            Name = this.Name,
            ID = this.ID,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            Elevation = this.Elevation,
            DisplayName = this.DisplayName
        };
    }
}
```

```c#
// CEC.Weather/Model/DbWeatherReport.cs
public class DbWeatherReport :IDbRecord<DbWeatherReport>
{
    [NotMapped]
    public int WeatherReportID { get => this.ID; }

    [SPParameter(IsID = true, DataType = SqlDbType.Int)]
    public int ID { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.Int)]
    public int WeatherStationID { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.SmallDateTime)]
    public DateTime Date { get; set; } = DateTime.Now.Date;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,4)")]
    public decimal TempMax { get; set; } = 1000;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,4)")]
    public decimal TempMin { get; set; } = 1000;

    [SPParameter(DataType = SqlDbType.Int)]
    public int FrostDays { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,4)")]
    public decimal Rainfall { get; set; } = -1;

    [SPParameter(DataType = SqlDbType.Decimal)]
    [Column(TypeName ="decimal(8,2)")]
    public decimal SunHours { get; set; } = -1;

    public string DisplayName { get; set; }

    public string WeatherStationName { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    [NotMapped]
    public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);

    [NotMapped]
    public string MonthYearName => $"{this.MonthName}-{this.Year}";

    public void SetNew() => this.ID = 0;

    public DbWeatherReport ShadowCopy()
    {
        return new DbWeatherReport() {
            ID = this.ID,
            Date = this.Date,
            TempMax = this.TempMax,
            TempMin = this.TempMin,
            FrostDays = this.FrostDays,
            Rainfall = this.Rainfall,
            SunHours = this.SunHours,
            DisplayName = this.DisplayName,
            WeatherStationID = this.WeatherStationID,
            WeatherStationName = this.WeatherStationName
        };
    }
}
```

### Add Some Utility Classes

I'm a great believer in making life easier.  Extension methods are great for this. Longitudes and Latitudes are handled as decimals, but we need to present them a little differently in the UI. We use decimal extension methods to do this.

```c#
// CEC.Weather/Extensions/DecimalExtensions.cs
public static class DecimalExtensions
{
    public static string AsLatitude(this decimal value)  => value > 0 ? $"{value}N" : $"{Math.Abs(value)}S";

    public static string AsLongitude(this decimal value) => value > 0 ? $"{value}E" : $"{Math.Abs(value)}W";
}
```
The application uses Blazored Fluent Validation for the Editors.  It's more flexible that the built in validation.

```c#
// CEC.Weather/Data/Validators/WeatherStationValidator.cs
using FluentValidation;

namespace CEC.Weather.Data.Validators
{
    public class WeatherStationValidator : AbstractValidator<DbWeatherStation>
    {
        public WeatherStationValidator()
        {
            RuleFor(p => p.Longitude).LessThan(-180).WithMessage("Longitude must be -180 or greater");
            RuleFor(p => p.Longitude).GreaterThan(180).WithMessage("Longitude must be 180 or less");
            RuleFor(p => p.Latitude).LessThan(-90).WithMessage("Latitude must be -90 or greater");
            RuleFor(p => p.Latitude).GreaterThan(90).WithMessage("Latitude must be 90 or less");
            RuleFor(p => p.Name).MinimumLength(1).WithMessage("Your need a Station Name!");
        }
    }
}
```

```c#
// CEC.Weather/Data/Validators/WeatherReportValidator.cs
using FluentValidation;

namespace CEC.Weather.Data.Validators
{
    public class WeatherReportValidator : AbstractValidator<DbWeatherReport>
    {
        public WeatherReportValidator()
        {
            RuleFor(p => p.Date).NotEmpty().WithMessage("You must select a date");
            RuleFor(p => p.TempMax).LessThan(60).WithMessage("The temperature must be less than 60C");
            RuleFor(p => p.TempMax).GreaterThan(-40).WithMessage("The temperature must be greater than -40C");
            RuleFor(p => p.TempMin).LessThan(60).WithMessage("The temperature must be less than 60C");
            RuleFor(p => p.TempMin).GreaterThan(-40).WithMessage("The temperature must be greater than -40C");
            RuleFor(p => p.FrostDays).LessThan(32).WithMessage("There's a maximun of 31 days in any month");
            RuleFor(p => p.FrostDays).GreaterThan(0).WithMessage("valid entries are 0-31");
            RuleFor(p => p.Rainfall).GreaterThan(0).WithMessage("valid entries are 0-31");
            RuleFor(p => p.SunHours).LessThan(24).WithMessage("Valid entries 0-24");
            RuleFor(p => p.SunHours).GreaterThan(0).WithMessage("Valid entries 0-24");
        }
    }
}
```

### Update WeatherForecastDbContext

Add two new `DbSet` properties to the class and two `modelBuilder` calls to `OnModelCreating`.
```c#
// CEC.Weather/Data/WeatherForecastDbContext.cs
......

public DbSet<DbWeatherStation> WeatherStation { get; set; }

public DbSet<DbWeatherReport> WeatherReport { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
......
    modelBuilder
        .Entity<DbWeatherStation>(eb =>
        {
            eb.HasNoKey();
            eb.ToView("vw_WeatherStation");
        });
    modelBuilder
        .Entity<DbWeatherReport>(eb =>
        {
            eb.HasNoKey();
            eb.ToView("vw_WeatherReport");
        });
}
```

### Add Data and Controller Services

We only show the Weather Station Services code here - the Weather Report Services are identical.
 
Add the `IWeatherStationDataService` and `IWeatherReportDataService` interfaces.

```c#
// CEC.Weather/Services/Interfaces/IWeatherStationDataService.cs
using CEC.Blazor.Services;
using CEC.Weather.Data;

namespace CEC.Weather.Services
{
    public interface IWeatherStationDataService : 
        IDataService<DbWeatherStation, WeatherForecastDbContext>
    {}
}
```

Add the Server Data Services.

```c#
// CEC.Weather/Services/DataServices/WeatherStationServerDataService.cs
using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;

namespace CEC.Weather.Services
{
    public class WeatherStationServerDataService :
        BaseServerDataService<DbWeatherStation, WeatherForecastDbContext>,
        IWeatherStationDataService
    {
        public WeatherStationServerDataService(IConfiguration configuration, IDbContextFactory<WeatherForecastDbContext> dbcontext) : base(configuration, dbcontext)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
        }
    }
}
```

Add the WASM Data Services

```c#
// CEC.Weather/Services/DataServices/WeatherStationWASMDataService.cs
using CEC.Weather.Data;
using CEC.Blazor.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CEC.Blazor.Data;

namespace CEC.Weather.Services
{
    public class WeatherStationWASMDataService :
        BaseWASMDataService<DbWeatherStation, WeatherForecastDbContext>,
        IWeatherStationDataService
    {
        public WeatherStationWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration, httpClient)
        {
            this.RecordConfiguration = new RecordConfigurationData() { RecordName = "WeatherStation", RecordDescription = "Weather Station", RecordListName = "WeatherStation", RecordListDecription = "Weather Stations" };
        }
    }
}
```

Add the Controller Services

```c#
// CEC.Weather/Services/ControllerServices/WeatherStationControllerService.cs
using CEC.Weather.Data;
using CEC.Blazor.Services;
using CEC.Blazor.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CEC.Weather.Services
{
    public class WeatherStationControllerService : BaseControllerService<DbWeatherStation, WeatherForecastDbContext>, IControllerService<DbWeatherStation, WeatherForecastDbContext>
    {
        /// <summary>
        /// List of Outlooks for Select Controls
        /// </summary>
        public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

        public WeatherStationControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IWeatherStationDataService DataService) : base(appconfiguration, navmanager)
        {
            this.Service = DataService;
            this.DefaultSortColumn = "ID";
        }
    }
}
```

## Forms

The forms rely heavily on the boilerplate code in their respective base classes.  The code pages are relatively simple, while the razor markup pages contain the record specific UI information.

### WeatherStation Viewer Form

The code behind page is trivial - everything is handled by the boilerplate code in `RecordComponentBase`.

```c#
// CEC.Weather/Components/Forms/WeatherStationViewerForm.razor.cs

using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationViewerForm : RecordComponentBase<DbWeatherStation, WeatherForecastDbContext>
    {
        [Inject]
        private WeatherStationControllerService ControllerService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            this.Service = this.ControllerService;
            // Set the delay on the record load as this is a demo project
            this.DemoLoadDelay = 0;
            await base.OnInitializedAsync();
        }
    }
}
```
The razor page builds out the UI controls for displaying the record fields.  Note that we have to use `@using` statements in the markup as this is a library file with no `_Imports.Razor`.

```c#
// CEC.Weather/Components/Forms/WeatherStationViewerForm.razor

@using CEC.Blazor.Components
@using CEC.Blazor.Components.BaseForms
@using CEC.Blazor.Components.UIControls
@using CEC.Weather.Data
@using CEC.FormControls.Components.FormControls

@namespace CEC.Weather.Components
@inherits RecordComponentBase<DbWeatherReport, WeatherForecastDbContext>

<UICard>
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        <UIErrorHandler IsError="this.IsError" IsLoading="this.IsDataLoading" ErrorMessage="@this.RecordErrorMessage">
            <UIContainer>
                <UIRow>
                    <UILabelColumn Columns="2">
                        Month/Year
                    </UILabelColumn>
                    <UIColumn Columns="2">
                        <FormControlPlainText Value="@this.Service.Record.MonthYearName"></FormControlPlainText>
                    </UIColumn>
                    <UILabelColumn Columns="2">
                        Station
                    </UILabelColumn>
                    <UIColumn Columns="4">
                        <FormControlPlainText Value="@this.Service.Record.WeatherStationName"></FormControlPlainText>
                    </UIColumn>
                    <UILabelColumn Columns="1">
                        ID
                    </UILabelColumn>
                    <UIColumn Columns="1">
                        <FormControlPlainText Value="@this.Service.Record.ID.ToString()"></FormControlPlainText>
                    </UIColumn>
                </UIRow>
                <UIRow>
                    <UILabelColumn Columns="2">
                        Max Temperature &deg; C:
                    </UILabelColumn>
                    <UIColumn Columns="4">
                        <FormControlPlainText Value="@this.Service.Record.TempMax.ToString()"></FormControlPlainText>
                    </UIColumn>
                    <UILabelColumn Columns="2">
                        Min Temperature &deg; C:
                    </UILabelColumn>
                    <UIColumn Columns="4">
                        <FormControlPlainText Value="@this.Service.Record.TempMin.ToString()"></FormControlPlainText>
                    </UIColumn>
                </UIRow>
                <UIRow>
                    <UILabelColumn Columns="2">
                        Frost Days
                    </UILabelColumn>
                    <UIColumn Columns="2">
                        <FormControlPlainText Value="@this.Service.Record.FrostDays.ToString()"></FormControlPlainText>
                    </UIColumn>
                    <UILabelColumn Columns="2">
                        Rainfall (mm)
                    </UILabelColumn>
                    <UIColumn Columns="2">
                        <FormControlPlainText Value="@this.Service.Record.Rainfall.ToString()"></FormControlPlainText>
                    </UIColumn>
                    <UILabelColumn Columns="2">
                        Sunshine (hrs)
                    </UILabelColumn>
                    <UIColumn Columns="2">
                        <FormControlPlainText Value="@this.Service.Record.SunHours.ToString()"></FormControlPlainText>
                    </UIColumn>
                </UIRow>
            </UIContainer>
        </UIErrorHandler>
        <UIContainer>
            <UIRow>
                <UIButtonColumn Columns="12">
                    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToList))">
                        Exit To List
                    </UIButton>
                    <UIButton Show="!this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToLast))">
                        Exit
                    </UIButton>
                    <UIButton Show="this.IsModal" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.ModalExit())">
                        Exit
                    </UIButton>
                </UIButtonColumn>
            </UIRow>
        </UIContainer>
    </Body>
</UICard>
```
### WeatherStation Editor Form

```c#
// CEC.Weather/Components/Forms/WeatherStationEditorForm.razor.cs

using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationEditorForm : EditRecordComponentBase<DbWeatherStation, WeatherForecastDbContext>
    {
        [Inject]
        public WeatherStationControllerService ControllerService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            // Assign the correct controller service
            this.Service = this.ControllerService;
            // Set the delay on the record load as this is a demo project
            this.DemoLoadDelay = 0;
            await base.OnInitializedAsync();
        }
    }
}
```
```c#
// CEC.Weather/Components/Forms/WeatherStationEditorForm.razor
@using CEC.Blazor.Components
@using CEC.Blazor.Components.BaseForms
@using CEC.Blazor.Components.UIControls
@using CEC.Weather.Data
@using CEC.FormControls.Components.FormControls
@using Microsoft.AspNetCore.Components.Forms
@using Blazored.FluentValidation

@namespace CEC.Weather.Components
@inherits EditRecordComponentBase<DbWeatherStation, WeatherForecastDbContext>

<UICard IsCollapsible="false">
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        <CascadingValue Value="@this.RecordFieldChanged" Name="OnRecordChange" TValue="Action<bool>">
            <UIErrorHandler IsError="@this.IsError" IsLoading="this.IsDataLoading" ErrorMessage="@this.RecordErrorMessage">
                <UIContainer>
                    <EditForm EditContext="this.EditContext">
                        <FluentValidationValidator DisableAssemblyScanning="@true" />
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Record ID:
                            </UILabelColumn>
                            <UIColumn Columns="4">
                                <FormControlPlainText Value="@this.Service.Record.ID.ToString()"></FormControlPlainText>
                            </UIColumn>
                        </UIFormRow>
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Name:
                            </UILabelColumn>
                            <UIColumn Columns="4">
                                <FormControlText class="form-control" @bind-Value="this.Service.Record.Name" RecordValue="this.Service.ShadowRecord.Name"></FormControlText>
                            </UIColumn>
                            <UIColumn Columns="4">
                                <ValidationMessage For=@(() => this.Service.Record.Name) />
                            </UIColumn>
                        </UIFormRow>
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Latitude
                            </UILabelColumn>
                            <UIColumn Columns="2">
                                <FormControlNumber class="form-control" @bind-Value="this.Service.Record.Latitude" RecordValue="this.Service.ShadowRecord.Latitude"></FormControlNumber>
                            </UIColumn>
                            <UIColumn Columns="6">
                                <ValidationMessage For=@(() => this.Service.Record.Latitude) />
                            </UIColumn>
                        </UIFormRow>
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Longitude
                            </UILabelColumn>
                            <UIColumn Columns="2">
                                <FormControlNumber class="form-control" @bind-Value="this.Service.Record.Longitude" RecordValue="this.Service.ShadowRecord.Longitude"></FormControlNumber>
                            </UIColumn>
                            <UIColumn Columns="6">
                                <ValidationMessage For=@(() => this.Service.Record.Longitude) />
                            </UIColumn>
                        </UIFormRow>
                        <UIFormRow>
                            <UILabelColumn Columns="4">
                                Elevation
                            </UILabelColumn>
                            <UIColumn Columns="2">
                                <FormControlNumber class="form-control" @bind-Value="this.Service.Record.Elevation" RecordValue="this.Service.ShadowRecord.Elevation"></FormControlNumber>
                            </UIColumn>
                            <UIColumn Columns="6">
                                <ValidationMessage For=@(() => this.Service.Record.Elevation) />
                            </UIColumn>
                        </UIFormRow>
                    </EditForm>
                </UIContainer>
            </UIErrorHandler>
            <UIContainer>
                <UIRow>
                    <UIColumn Columns="7">
                        <UIAlert Alert="this.AlertMessage" SizeCode="Bootstrap.SizeCode.sm"></UIAlert>
                    </UIColumn>
                    <UIButtonColumn Columns="5">
                        <UIButton Show="this.NavigationCancelled && this.IsLoaded" ClickEvent="this.Cancel" ColourCode="Bootstrap.ColourCode.cancel">Cancel</UIButton>
                        <UIButton Show="this.NavigationCancelled && this.IsLoaded" ClickEvent="this.SaveAndExit" ColourCode="Bootstrap.ColourCode.save">Save &amp; Exit</UIButton>
                        <UIButton Show="(!this.IsClean) && this.IsLoaded" ClickEvent="this.Save" ColourCode="Bootstrap.ColourCode.save">Save</UIButton>
                        <UIButton Show="this.ShowExitConfirmation && this.IsLoaded" ClickEvent="this.ConfirmExit" ColourCode="Bootstrap.ColourCode.danger_exit">Exit Without Saving</UIButton>
                        <UIButton Show="(!this.NavigationCancelled) && !this.ShowExitConfirmation" ClickEvent="(e => this.NavigateTo(PageExitType.ExitToList))" ColourCode="Bootstrap.ColourCode.nav">Exit To List</UIButton>
                        <UIButton Show="(!this.NavigationCancelled) && !this.ShowExitConfirmation" ClickEvent="this.Exit" ColourCode="Bootstrap.ColourCode.nav">Exit</UIButton>
                    </UIButtonColumn>
                </UIRow>
            </UIContainer>
        </CascadingValue>
    </Body>
</UICard>
```

### WeatherStation List Form

```c#
// CEC.Weather/Components/Forms/WeatherStation/WeatherStationListForm.razor.cs
@using CEC.Blazor.Components
@using CEC.Blazor.Components.BaseForms
@using CEC.Blazor.Components.UIControls
@using CEC.Weather.Data
@using CEC.Weather.Extensions
@using CEC.Blazor.Extensions

@namespace CEC.Weather.Components

@inherits ListComponentBase<DbWeatherStation, WeatherForecastDbContext>

<UIWrapper UIOptions="@this.UIOptions" RecordConfiguration="@this.Service.RecordConfiguration" OnView="@OnView" OnEdit="@OnEdit">
    <UICardGrid TRecord="DbWeatherStation" IsCollapsible="true" Paging="this.Paging" IsLoading="this.Loading">
        <Title>
            @this.ListTitle
        </Title>
        <TableHeader>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="1" FieldName="ID">ID</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="2" FieldName="Name">Name</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="3" FieldName="Latitude">Latitiude</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="4" FieldName="Longitude">Longitude</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="5" FieldName="Elevation">Elevation</UIGridTableHeaderColumn>
            <UIGridTableHeaderColumn TRecord="DbWeatherStation" Column="6"></UIGridTableHeaderColumn>
        </TableHeader>
        <RowTemplate>
            <CascadingValue Name="RecordID" Value="@context.ID">
                <UIGridTableColumn TRecord="DbWeatherStation" Column="1">@context.ID</UIGridTableColumn>
                <UIGridTableColumn TRecord="DbWeatherStation" Column="2">@context.Name</UIGridTableColumn>
                <UIGridTableColumn TRecord="DbWeatherStation" Column="3">@context.Latitude.AsLatitude()</UIGridTableColumn>
                <UIGridTableColumn TRecord="DbWeatherStation" Column="4">@context.Longitude.AsLongitude()</UIGridTableColumn>
                <UIGridTableColumn TRecord="DbWeatherStation" Column="5">@context.Elevation.DecimalPlaces(1)</UIGridTableColumn>
                <UIGridTableEditColumn TRecord="DbWeatherStation"></UIGridTableEditColumn>
            </CascadingValue>
        </RowTemplate>
        <Navigation>
            <UIListButtonRow>
                <Paging>
                    <PagingControl TRecord="DbWeatherStation" Paging="this.Paging"></PagingControl>
                </Paging>
            </UIListButtonRow>
        </Navigation>
    </UICardGrid>
</UIWrapper>
<BootstrapModal @ref="this._BootstrapModal"></BootstrapModal>
```

```c#
// CEC.Weather/Components/Forms/WeatherStation/WeatherStationListForm.razor.cs
using Microsoft.AspNetCore.Components;
using CEC.Blazor.Components.BaseForms;
using CEC.Weather.Data;
using CEC.Weather.Services;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class WeatherStationListForm : ListComponentBase<DbWeatherStation, WeatherForecastDbContext>
    {
        /// The Injected Controller service for this record
        [Inject]
        protected WeatherStationControllerService ControllerService { get; set; }


        protected async override Task OnInitializedAsync()
        {
            this.UIOptions.MaxColumn = 2;
            this.Service = this.ControllerService;
            await base.OnInitializedAsync();
        }

        /// Method called when the user clicks on a row in the viewer.
        protected void OnView(int id) => this.OnViewAsync<WeatherStationViewerForm>(id);

        /// Method called when the user clicks on a row Edit button.
        protected void OnEdit(int id) => this.OnEditAsync<WeatherStationEditorForm>(id);
    }
}
```

### Weather Report Forms

You can get these from the GitHub Repository.  They are the same as the Station forms except in the editor where we have a select and a lookuplist for the Weather Stations. The section from the editor form looks like this. 
```c#
// CEC.Weather/Components/Forms/WeatherReport/WeatherReportEditorForm.razor
<UIFormRow>
    <UILabelColumn Columns="4">
        Station:
    </UILabelColumn>
    <UIColumn Columns="4">
        <InputControlSelect OptionList="this.StationLookupList" @bind-Value="this.Service.Record.WeatherStationID" RecordValue="@this.Service.ShadowRecord.WeatherStationID"></InputControlSelect>
    </UIColumn>
</UIFormRow>
```
The `StationLookupList` property is loaded in `OnParametersSetAsync` by making a call to the generic `GetLookUpListAsync\<IRecord\>()` method in the Controller Service.  We specify the actual record type - in this case `DbWeatherStation` - and the method calls back into the relevant Data Service which does it's magic (`GetRecordLookupListAsync` in DBContextExtensions in `CEC.Blazor/Extensions`) and returns a `SortedDictionary` list containing the record `ID` and `DisplayName` properties.  

```c#
// CEC.Weather/Components/Forms/WeatherReport/WeatherReportEditorForm.razor.cs

    public partial class WeatherReportEditorForm : EditRecordComponentBase<DbWeatherReport, WeatherForecastDbContext>
    {
        .......
        // Property to hold the Station Lookup List
        public SortedDictionary<int, string> StationLookupList { get; set; }

        .......
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            // Method to get the Station Lookup List. Called here so whenever we do a UI refresh we get the list, we never know when it might be updated
            StationLookupList = await this.Service.GetLookUpListAsync<DbWeatherStation>();
        }
    }
```

Filter loading is part of `OnParametersSetAsync` process in `ListComponentBase`

```c#
// CEC.Blazor/Components/BaseForms/ListComponentBase.cs

protected async override Task OnParametersSetAsync()
{
    await base.OnParametersSetAsync();
    // Load the page - as we've reset everything this will be the first page with the default filter
    if (this.IsService)
    {
        // Load the filters for the recordset
        this.LoadFilter();
        // Load the paged recordset
        await this.Service.LoadPagingAsync();
    }
    this.Loading = false;
}

/// Method called to load the filter
protected virtual void LoadFilter()
{
    if (IsService) this.Service.FilterList.OnlyLoadIfFilters = this.OnlyLoadIfFilter;
}
```

`WeatherReportListForm` overrides `LoadFilter` to set up the record specific filters.

```c#
// CEC.Weather/Components/Forms/WeatherReport/WeatherReportListForm.razor.cs
.....
[Parameter]
public int WeatherStationID { get; set; }
.......
/// inherited - loads the filter
protected override void LoadFilter()
{
    // Before the call to base so the filter is set before the get the list
    if (this.IsService &&  this.WeatherStationID > 0)
    {
        this.Service.FilterList.Filters.Clear();
        this.Service.FilterList.Filters.Add("WeatherStationID", this.WeatherStationID);
    }
    base.LoadFilter();
}
......
```
### Nav Menu

Add the menu link in `NavMenu`.

```c#
// CEC.Weather/Components/Controls/NavMenu.cs
    .....
    <li class="nav-item px-3">
        <NavLink class="nav-link" href="weatherforecastmodal">
            <span class="oi oi-cloud-upload" aria-hidden="true"></span> Modal Weather
        </NavLink>
    </li>
    <li class="nav-item px-3">
        <NavLink class="nav-link" href="weatherstation">
            <span class="oi oi-cloudy" aria-hidden="true"></span> Weather Stations
        </NavLink>
    </li>
    <li class="nav-item px-3">
        <NavLink class="nav-link" href="weatherreport">
            <span class="oi oi-cloudy" aria-hidden="true"></span> Weather Reports
        </NavLink>
    </li>
    <li class="nav-item px-3">
        <NavLink class="nav-link" href="https://github.com/ShaunCurtis/CEC.Blazor">
            <span class="oi oi-fork" aria-hidden="true"></span> Github Repo
        </NavLink>
    </li>
    ......
```

### Filter Control

Add a new control called `MonthYearIDListFilter`. This is used in the `WestherReport` list View to filter the records.

```c#
// CEC.Weather/Components/Controls/MonthYearIDListFilter.razor.cs
using CEC.Blazor.Data;
using CEC.Weather.Data;
using CEC.Weather.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CEC.Weather.Components
{
    public partial class MonthYearIDListFilter : ComponentBase
    {
        // Inject the Controller Service
        [Inject]
        private WeatherReportControllerService Service { get; set; }

        // Boolean to control the ID Control Display
        [Parameter]
        public bool ShowID { get; set; } = true;

        // Month Lookup List
        private SortedDictionary<int, string> MonthLookupList { get; set; }

        // Year Lookup List
        private SortedDictionary<int, string> YearLookupList { get; set; }

        // Weather Station Lookup List
        private SortedDictionary<int, string> IdLookupList { get; set; }

        // Dummy Edit Context for selects
        private EditContext EditContext => new EditContext(this.Service.Record);

        // privates to hold current select values
        private int OldMonth = 0;
        private int OldYear = 0;
        private long OldID = 0;

        // Month value - adds or removes the value from the filter list and kicks off Filter changed if changed
        private int Month
        {
            get => this.Service.FilterList.TryGetFilter("Month", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Month", value);
                else this.Service.FilterList.ClearFilter("Month");
                if (this.Month != this.OldMonth)
                {
                    this.OldMonth = this.Month;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        // Year value - adds or removes the value from the filter list and kicks off Filter changed if changed
        private int Year
        {
            get => this.Service.FilterList.TryGetFilter("Year", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("Year", value);
                else this.Service.FilterList.ClearFilter("Year");
                if (this.Year != this.OldYear)
                {
                    this.OldYear = this.Year;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        // Weather Station value - adds or removes the value from the filter list and kicks off Filter changed if changed
        private int ID
        {
            get => this.Service.FilterList.TryGetFilter("WeatherStationID", out object value) ? (int)value : 0;
            set
            {
                if (value > 0) this.Service.FilterList.SetFilter("WeatherStationID", value);
                else this.Service.FilterList.ClearFilter("WeatherStationID");
                if (this.ID != this.OldID)
                {
                    this.OldID = this.ID;
                    this.Service.TriggerFilterChangedEvent(this);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            this.OldYear = this.Year;
            this.OldMonth = this.Month;
            await GetLookupsAsync();
        }

        // Method to get he LokkupLists
        protected async Task GetLookupsAsync()
        {
            this.IdLookupList = await this.Service.GetLookUpListAsync<DbWeatherStation>("-- ALL STATIONS --");
            // Get the months in the year
            this.MonthLookupList = new SortedDictionary<int, string> { { 0, "-- ALL MONTHS --" } };
            for (int i = 1; i < 13; i++) this.MonthLookupList.Add(i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
            // Gets a distinct list of Years in the Weather Reports
            {
                var list = await this.Service.GetDistinctListAsync(new DbDistinctRequest() { FieldName = "Year", QuerySetName = "WeatherReport", DistinctSetName = "DistinctList" });
                this.YearLookupList = new SortedDictionary<int, string> { { 0, "-- ALL YEARS --" } };
                list.ForEach(item => this.YearLookupList.Add(int.Parse(item), item));
            }

        }
    }
}
```
```c#
// CEC.Weather/Components/Controls/MonthYearIDListFilter.razor
@using CEC.Blazor.Components.FormControls
@using Microsoft.AspNetCore.Components.Forms

@namespace CEC.Weather.Components
@inherits ComponentBase

<EditForm EditContext="this.EditContext">

    <table class="table">
        <tr>
            @if (this.ShowID)
            {
                <!--Weather Station-->
                <td>
                    <label class="" for="ID">Weather Station:</label>
                    <div class="">
                        <InputControlSelect OptionList="this.IdLookupList" @bind-Value="this.ID"></InputControlSelect>
                    </div>
                </td>
            }
            <td>
                <!--Month-->
                <label class="">Month:</label>
                <div class="">
                    <InputControlSelect OptionList="this.MonthLookupList" @bind-Value="this.Month"></InputControlSelect>
                </div>
            </td>
            <td>
                <!--Year-->
                <label class="">Year:</label>
                <div class="">
                    <InputControlSelect OptionList="this.YearLookupList" @bind-Value="this.Year"></InputControlSelect>
                </div>
            </td>
        </tr>
    </table>
</EditForm>
```
 The filter displays a set of dropdowns.  When you change a value, the value is added, updated or deleted from the filter list and the service FilterUpdated event is kicked off.  This triggers a set of events which kicks off a ListForm UI Update.  We'll look at this in more detail in the next article in this series in a section of the article - Component Updating with Events.  

## CEC.Blazor.Server

All the shared code is now complete and need to move down to the actual projects.

To set up the Server we need to:

1. Configure the correct services - specific to the Server.
2. Build the Views for each record type - these are the same views as used in the WASM Client.


### Startup.cs

We need to update Startup with the new services, by updating `AddApplicationServices` in `ServiceCollectionExtensions.cs`.

Note `xxxxxxServerDataService` is added as a `IxxxxxxDataService`.

```c#
// CEC.Blazor.Server/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Singleton service for the Server Side version of each Data Service 
    services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
    services.AddSingleton<IWeatherStationDataService, WeatherStationServerDataService>();
    services.AddSingleton<IWeatherReportDataService, WeatherReportServerDataService>();
    // Scoped service for each Controller Service
    services.AddScoped<WeatherForecastControllerService>();
    services.AddScoped<WeatherStationControllerService>();
    services.AddScoped<WeatherReportControllerService>();
    // Transient service for the Fluent Validator for each record
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    services.AddTransient<IValidator<DbWeatherStation>, WeatherStationValidator>();
    services.AddTransient<IValidator<DbWeatherReport>, WeatherReportValidator>();
    // Factory for building the DBContext 
    var dbContext = configuration.GetValue<string>("Configuration:DBContext");
    services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
    return services;
}
```

### Weather Station Routes/Views

These are almost trivial.  All the code and markup is in the forms.  We just declare the route and add the form to the View.

```c#
// CEC.Blazor.Server/Routes/WeatherStation/WeatherStationEditorView.razor
@page "/WeatherStation/New"
@page "/WeatherStation/Edit"

@inherits ApplicationComponentBase
@namespace CEC.Blazor.Server.Routes

<WeatherStationEditorForm></WeatherStationEditorForm>
```

```c#
// CEC.Blazor.Server/Routes/WeatherStation/WeatherStationListView.razor
@page "/WeatherStation"
@namespace CEC.Blazor.Server.Routes
@inherits ApplicationComponentBase

<WeatherStationListForm UIOptions="this.UIOptions" ></WeatherStationListForm>

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
The view for WeatherStation is a little more complicated.  We add the View Form for WeatherStation and the List Form for WeatherReports, and pass the WeatherReport list form the WeatherStation ID. 
```c#
@page "/WeatherStation/View"
@namespace CEC.Blazor.Server.Routes
@inherits ApplicationComponentBase

<WeatherStationViewerForm></WeatherStationViewerForm>
<UIBootstrapBase Css="mt-2">
    <WeatherReportListForm WeatherStationID="this._ID" OnlyLoadIfFilter="true"></WeatherReportListForm>
</UIBootstrapBase>
```

```c#
// CEC.Blazor.Server/Routes/WeatherReport/WeatherReportEditorView.razor
@page "/WeatherReport/New"
@page "/WeatherReport/Edit"

@inherits ApplicationComponentBase

@namespace CEC.Blazor.Server.Routes

<WeatherReportEditorForm></WeatherReportEditorForm>

```
`WeatherReportListView` uses the `MonthYearIdListFilter` to control the Weather Report List.  Note `OnlyLoadIfFilter` is set to true to prevent the full recordset being displayed when no filter is set.
```c#
// CEC.Blazor.Server/Routes/WeatherReport/WeatherReportListView.razor
@page "/WeatherReport"

@namespace CEC.Blazor.Server.Routes
@inherits ApplicationComponentBase

<MonthYearIDListFilter></MonthYearIDListFilter>
<WeatherReportListForm OnlyLoadIfFilter="true" UIOptions="this.UIOptions"></WeatherReportListForm>

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
```c#
// CEC.Blazor.Server/Routes/WeatherReport/WeatherReportViewerView.razor
@page "/WeatherReport/View"

@namespace CEC.Blazor.Server.Routes
@inherits ApplicationComponentBase

<WeatherReportViewerForm></WeatherReportViewerForm>
```
## CEC.Blazor.WASM.Client

To set up the client we need to:

1. Configure the correct services - specific to the Client.
2. Build the Views for each record type - same as Server.

### program.cs

We need to update program with the new services.  We do this by updating `AddApplicationServices` in `ServiceCollectionExtensions.cs`.

`xxxxxxWASMDataService` is added as the `IxxxxxxDataService`.

```c#
// CEC.Blazor.WASM/Client/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Scoped service for the WASM Client version of Data Services 
    services.AddScoped<IWeatherForecastDataService, WeatherForecastWASMDataService>();
    services.AddScoped<IWeatherStationDataService, WeatherStationWASMDataService>();
    services.AddScoped<IWeatherReportDataService, WeatherReportWASMDataService>();
    // Scoped service for the Controller Services
    services.AddScoped<WeatherForecastControllerService>();
    services.AddScoped<WeatherStationControllerService>();
    services.AddScoped<WeatherReportControllerService>();
    // Transient service for the Fluent Validator for the records
    services.AddTransient<IValidator<DbWeatherForecast>, WeatherForecastValidator>();
    services.AddTransient<IValidator<DbWeatherStation>, WeatherStationValidator>();
    services.AddTransient<IValidator<DbWeatherReport>, WeatherReportValidator>();
    return services;
}
```
### Weather Station Routes/Views

These are exactly the same as Server. So I won't repeat them here.

That's it.  The Client is configured.

## CEC.Blazor.WASM.Server

The WASM Server is the API provider.  We need to:

1. Configure the correct services.
2. Build controllers for each record type.

### Startup.cs

We need to update Startup with the new services.  We do this by updating `AddApplicationServices` in `ServiceCollectionExtensions.cs`.

`xxxxxxServerDataService` is added as the `IxxxxxxDataService`.

```c#
// CEC.Blazor.WASM.Server/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Singleton service for the Server Side version of each Data Service 
    services.AddSingleton<IWeatherForecastDataService, WeatherForecastServerDataService>();
    services.AddSingleton<IWeatherStationDataService, WeatherStationServerDataService>();
    services.AddSingleton<IWeatherReportDataService, WeatherReportServerDataService>();
    // Factory for building the DBContext 
    var dbContext = configuration.GetValue<string>("Configuration:DBContext");
    services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
    return services;
}
```

### Weather Station Controllers

The controllers act as gateways to the data controllers for each service.  They are self explanatory.  We use `HttpgGet` where we are just making a data request, and `HttpPost` where we need to post information into the API.  The controller for each record type has the same patterns - building new ones is a copy and replace exercise.

```c#
// CEC.Blazor.WASM.Server/Controllers/WeatherStationController.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CEC.Weather.Services;
using CEC.Weather.Data;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.WASM.Server.Controllers
{
    [ApiController]
    public class WeatherStationController : ControllerBase
    {
        protected IWeatherStationDataService DataService { get; set; }

        private readonly ILogger<WeatherStationController> logger;

        public WeatherStationController(ILogger<WeatherStationController> logger, IWeatherStationDataService dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [MVC.Route("weatherstation/list")]
        [HttpGet]
        public async Task<List<DbWeatherStation>> GetList() => await DataService.GetRecordListAsync();

        [MVC.Route("weatherStation/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherStation>> GetFilteredRecordListAsync([FromBody] FilterList filterList) => await DataService.GetFilteredRecordListAsync(filterList);

        [MVC.Route("weatherstation/base")]
        public async Task<List<DbBaseRecord>> GetBaseAsync() => await DataService.GetBaseRecordListAsync<DbWeatherStation>();

        [MVC.Route("weatherstation/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync();

        [MVC.Route("weatherstation/get")]
        [HttpGet]
        public async Task<DbWeatherStation> GetRec(int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherstation/read")]
        [HttpPost]
        public async Task<DbWeatherStation> Read([FromBody]int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherstation/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherStation record) => await DataService.UpdateRecordAsync(record);

        [MVC.Route("weatherstation/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherStation record) => await DataService.CreateRecordAsync(record);

        [MVC.Route("weatherstation/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherStation record) => await DataService.DeleteRecordAsync(record);
    }
}
```
```c#
// CEC.Blazor.WASM.Server/Controllers/WeatherReportController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CEC.Weather.Services;
using CEC.Weather.Data;
using CEC.Blazor.Data;
using CEC.Blazor.Components;

namespace CEC.Blazor.WASM.Server.Controllers
{
    [ApiController]
    public class WeatherReportController : ControllerBase
    {
        protected IWeatherReportDataService DataService { get; set; }

        private readonly ILogger<WeatherReportController> logger;

        public WeatherReportController(ILogger<WeatherReportController> logger, IWeatherReportDataService dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [MVC.Route("weatherreport/list")]
        [HttpGet]
        public async Task<List<DbWeatherReport>> GetList() => await DataService.GetRecordListAsync();

        [MVC.Route("weatherreport/filteredlist")]
        [HttpPost]
        public async Task<List<DbWeatherReport>> GetFilteredRecordListAsync([FromBody] FilterList filterList) => await DataService.GetFilteredRecordListAsync(filterList);

        [MVC.Route("weatherreport/distinctlist")]
        [HttpPost]
        public async Task<List<string>> GetDistinctListAsync([FromBody] DbDistinctRequest req) => await DataService.GetDistinctListAsync(req);

        [MVC.Route("weatherreport/base")]
        public async Task<List<DbBaseRecord>> GetBaseAsync() => await DataService.GetBaseRecordListAsync<DbWeatherReport>();

        [MVC.Route("weatherreport/count")]
        [HttpGet]
        public async Task<int> Count() => await DataService.GetRecordListCountAsync();

        [MVC.Route("weatherreport/get")]
        [HttpGet]
        public async Task<DbWeatherReport> GetRec(int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherreport/read")]
        [HttpPost]
        public async Task<DbWeatherReport> Read([FromBody]int id) => await DataService.GetRecordAsync(id);

        [MVC.Route("weatherreport/update")]
        [HttpPost]
        public async Task<DbTaskResult> Update([FromBody]DbWeatherReport record) => await DataService.UpdateRecordAsync(record);

        [MVC.Route("weatherreport/create")]
        [HttpPost]
        public async Task<DbTaskResult> Create([FromBody]DbWeatherReport record) => await DataService.CreateRecordAsync(record);

        [MVC.Route("weatherreport/delete")]
        [HttpPost]
        public async Task<DbTaskResult> Delete([FromBody] DbWeatherReport record) => await DataService.DeleteRecordAsync(record);
    }
}
```

### Wrap Up
This article demonstrates how to add more record types to the Weather application and build out either the Blazor WASM or Server project to handle the new types.

In the final article we'll look at some key concepts and code within the application and at deployment.


## History

* 2-Oct-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
