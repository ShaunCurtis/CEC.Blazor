using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Foreecast
    /// Data validation is handled by the Fluent Validator
    /// Custom Attributes are for building the EF strored procedures
    /// </summary>
    [DbAccess(CreateSP = "sp_Create_WeatherForecast", UpdateSP ="sp_Update_WeatherForecast", DeleteSP ="sp_Delete_WeatherForecast") ]
    public class DbWeatherForecast :IDbRecord<DbWeatherForecast>
    {

        [NotMapped]
        public int WeatherForecastID { get => this.ID; }

        [SPParameter(IsID = true, DataType = SqlDbType.Int)]
        public int ID { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.SmallDateTime)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [SPParameter(DataType = SqlDbType.Decimal)]
        public decimal TemperatureC { get; set; } = 20;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string PostCode { get; set; } = string.Empty;

        [SPParameter(DataType = SqlDbType.Bit)]
        public bool Frost { get; set; }

        [SPParameter(DataType = SqlDbType.Int)]
        public int SummaryValue
        {
            get => (int)this.Summary;
            set => this.Summary = (WeatherSummary)value;
        }

        [SPParameter(DataType = SqlDbType.Int)]
        public int OutlookValue
        {
            get => (int)this.Outlook;
            set => this.Outlook = (WeatherOutlook)value;
        }

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Description { get; set; } = string.Empty;

        [SPParameter(DataType = SqlDbType.VarChar)]
        public string Detail { get; set; } = string.Empty;

        public string DisplayName { get; set; }

        [NotMapped]
        public decimal TemperatureF => decimal.Round(32 + (TemperatureC / 0.5556M), 2);

        [NotMapped]
        public WeatherSummary Summary { get; set; } = WeatherSummary.Unknown;

        [NotMapped]
        public WeatherOutlook Outlook { get; set; } = WeatherOutlook.Sunny;

        public void SetNew() => this.ID = 0;

        public DbWeatherForecast ShadowCopy()
        {
            return new DbWeatherForecast() {
                Date = this.Date,
                TemperatureC = this.TemperatureC,
                Frost = this.Frost,
                Outlook = this.Outlook,
                Description = this.Description,
                Summary = this.Summary,
                Detail = this.Detail
            };
        }
    }
}
