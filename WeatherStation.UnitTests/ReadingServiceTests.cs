using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;
using WeatherStation.Api.Dtos;
using WeatherStation.Api.Models;
using WeatherStation.Api.Services;
using Xunit;

namespace WeatherStation.UnitTests;

public class ReadingServiceTests
{
    [Fact]
    public async Task AddReadingAsync_CreatesHighTempAndHighWindAlerts()
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new WeatherStationDbContext(options);
        var station = new Station { Name = "Station 1", Latitude = 10, Longitude = 10, Altitude = 100, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        var service = new ReadingService(context);
        var readingDto = new CreateReadingDto(45, 50, 110, 1013, DateTime.UtcNow);

        var result = await service.AddReadingAsync(station.Id, readingDto);

        Assert.Equal(station.Id, result.StationId);
        Assert.Equal(45, result.Temperature);
        Assert.Equal(110, result.WindSpeedKmh);
        Assert.Equal(1, await context.Readings.CountAsync());
        Assert.Equal(2, await context.Alerts.CountAsync());
    }

    [Theory]
    [InlineData(-61, 50)]
    [InlineData(61, 50)]
    [InlineData(10, -1)]
    [InlineData(10, 101)]
    public async Task AddReadingAsync_InvalidTemperatureOrHumidity_Throws(double temperature, double humidity)
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new WeatherStationDbContext(options);
        var station = new Station { Name = "Station 1", Latitude = 10, Longitude = 10, Altitude = 100, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        var service = new ReadingService(context);
        var readingDto = new CreateReadingDto(temperature, humidity, 10, 1013, DateTime.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddReadingAsync(station.Id, readingDto));
    }

    [Fact]
    public async Task GetAverageAsync_ReturnsCorrectAverages()
    {
        var options = new DbContextOptionsBuilder<WeatherStationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new WeatherStationDbContext(options);
        var station = new Station { Name = "Station 1", Latitude = 10, Longitude = 10, Altitude = 100, IsActive = true };
        context.Stations.Add(station);
        await context.SaveChangesAsync();

        context.Readings.AddRange(
            new Reading { StationId = station.Id, Temperature = 10, Humidity = 30, WindSpeedKmh = 10, Pressure = 1000, RecordedAt = DateTime.UtcNow.AddMinutes(-10) },
            new Reading { StationId = station.Id, Temperature = 20, Humidity = 50, WindSpeedKmh = 20, Pressure = 1020, RecordedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ReadingService(context);
        var result = await service.GetAverageAsync(station.Id, null, null);

        Assert.Equal(15, result.Temperature);
        Assert.Equal(40, result.Humidity);
        Assert.Equal(15, result.WindSpeedKmh);
        Assert.Equal(1010, result.Pressure);
    }
}
