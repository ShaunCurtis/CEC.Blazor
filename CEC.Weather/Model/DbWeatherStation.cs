using CEC.Blazor.Data;
using CEC.Blazor.Extensions;
using CEC.Weather.Extensions;
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
    public class DbWeatherStation 
        :IDbRecord<DbWeatherStation>
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
        [Column(TypeName = "decimal(8,4)")]
        public decimal Longitude { get; set; } = 1000;

        [SPParameter(DataType = SqlDbType.Decimal)]
        [Column(TypeName = "decimal(8,2)")]
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
}
