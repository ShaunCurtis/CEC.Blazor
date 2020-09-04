using Microsoft.EntityFrameworkCore;

namespace CEC.Weather.Data
{
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
}
