using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Data Record for a Weather Foreecast
    /// Data validation is handled by the Fluent Validator
    /// Custom Attributes are for building the EF strored procedures
    /// </summary>
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
        [Column(TypeName = "decimal(8,4)")]
        public decimal TempMax { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal TempMin { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Int)]
        public int FrostDays { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,4)")]
        public decimal Rainfall { get; set; } = -1;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,2)")]
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
}
