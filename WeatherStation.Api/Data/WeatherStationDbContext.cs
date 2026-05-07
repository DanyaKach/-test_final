using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Models;

namespace WeatherStation.Api.Data;

public class WeatherStationDbContext : DbContext
{
    public WeatherStationDbContext(DbContextOptions<WeatherStationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Latitude).IsRequired();
            entity.Property(x => x.Longitude).IsRequired();
            entity.Property(x => x.Altitude).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();

            entity.HasMany(x => x.Readings)
                .WithOne(x => x.Station)
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Alerts)
                .WithOne(x => x.Station)
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reading>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Temperature).IsRequired();
            entity.Property(x => x.Humidity).IsRequired();
            entity.Property(x => x.WindSpeedKmh).IsRequired();
            entity.Property(x => x.Pressure).IsRequired();
            entity.Property(x => x.RecordedAt).IsRequired();

            entity.HasIndex(x => new { x.StationId, x.RecordedAt });
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type)
                .HasConversion<string>()
                .IsRequired();
            entity.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.IsAcknowledged).IsRequired();
        });
    }
}
