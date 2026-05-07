using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;
using WeatherStation.Api.Dtos;
using WeatherStation.Api.Models;
using WeatherStation.Api.Services;
using Xunit;

namespace WeatherStation.DbTests;

public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _postgresContainer;

    public DatabaseTests()
    {
        _postgresContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "weatherstation",
                Username = "postgres",
                Password = "postgres"
            })
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task StationReadingRelationship_IsPersistedCorrectly()
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseNpgsql(_postgresContainer.ConnectionString)
            .Options;

        await using var context = new WeatherStationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var station = new Station { Name = "DbStation", Latitude = 50, Longitude = 30, Altitude = 200, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        var reading = new Reading
        {
            StationId = station.Id,
            Temperature = 10,
            Humidity = 20,
            WindSpeedKmh = 10,
            Pressure = 1010,
            RecordedAt = DateTime.UtcNow
        };

        context.Readings.Add(reading);
        await context.SaveChangesAsync();

        var loaded = await context.Stations.Include(s => s.Readings).FirstOrDefaultAsync(s => s.Id == station.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Readings);
        Assert.Equal(reading.Id, loaded.Readings.First().Id);
    }

    [Fact]
    public async Task TimeRangeQuery_ReturnsOnlyMatchingReadings()
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseNpgsql(_postgresContainer.ConnectionString)
            .Options;

        await using var context = new WeatherStationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var station = new Station { Name = "DbStation2", Latitude = 50, Longitude = 30, Altitude = 200, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        context.Readings.AddRange(
            new Reading { StationId = station.Id, Temperature = 10, Humidity = 20, WindSpeedKmh = 15, Pressure = 1000, RecordedAt = DateTime.UtcNow.AddHours(-3) },
            new Reading { StationId = station.Id, Temperature = 15, Humidity = 30, WindSpeedKmh = 20, Pressure = 1005, RecordedAt = DateTime.UtcNow.AddHours(-1) }
        );
        await context.SaveChangesAsync();

        var from = DateTime.UtcNow.AddHours(-2);
        var readingsInRange = await context.Readings
            .Where(x => x.StationId == station.Id && x.RecordedAt >= from)
            .ToListAsync();

        Assert.Single(readingsInRange);
        Assert.Equal(15, readingsInRange[0].Temperature);
    }

    [Fact]
    public async Task AddingReading_CreatesAlertRecords()
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseNpgsql(_postgresContainer.ConnectionString)
            .Options;

        await using var context = new WeatherStationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var station = new Station { Name = "AlertStation", Latitude = 50, Longitude = 30, Altitude = 200, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        var service = new ReadingService(context);
        var readingDto = new CreateReadingDto(45, 50, 150, 1015, DateTime.UtcNow);
        await service.AddReadingAsync(station.Id, readingDto);

        var alertCount = await context.Alerts.CountAsync();
        Assert.Equal(2, alertCount);
    }
}
