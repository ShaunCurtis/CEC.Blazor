using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CEC.Weather.Data
{
    public class WeatherForecastDbContext : DbContext
    {
        /// <summary>
        /// Tracking lifetime of contexts.
        /// </summary>
        private readonly Guid _id;

        public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) 
            : base(options) {
            _id = Guid.NewGuid();
            Debug.WriteLine($"{_id} context created.");
        }

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

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        public override void Dispose()
        {
            Debug.WriteLine($"{_id} context disposed.");
            base.Dispose();
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/></returns>
        public override ValueTask DisposeAsync()
        {
            Debug.WriteLine($"{_id} context disposed async.");
            return base.DisposeAsync();
        }
    }
}
