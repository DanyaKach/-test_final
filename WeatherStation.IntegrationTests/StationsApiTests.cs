using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WeatherStation.Api.Dtos;
using Xunit;

namespace WeatherStation.IntegrationTests;

public class StationsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StationsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddReading_CreatesAlertForThresholds()
    {
        var createStation = new CreateStationDto("Station A", 10, 10, 100, true);
        var stationResponse = await _client.PostAsJsonAsync("/api/stations", createStation);
        stationResponse.EnsureSuccessStatusCode();

        var createdStation = await stationResponse.Content.ReadFromJsonAsync<StationDto>();
        Assert.NotNull(createdStation);

        var reading = new CreateReadingDto(45, 30, 120, 1012, DateTime.UtcNow);
        var readingResponse = await _client.PostAsJsonAsync($"/api/stations/{createdStation!.Id}/readings", reading);
        readingResponse.EnsureSuccessStatusCode();

        var alertsResponse = await _client.GetAsync("/api/alerts");
        alertsResponse.EnsureSuccessStatusCode();

        var alerts = await alertsResponse.Content.ReadFromJsonAsync<List<AlertDto>>();
        Assert.NotNull(alerts);
        Assert.Equal(2, alerts!.Count);
    }

    [Fact]
    public async Task GetAverage_ReturnsCorrectValues()
    {
        var createStation = new CreateStationDto("Station B", 20, 20, 100, true);
        var stationResponse = await _client.PostAsJsonAsync("/api/stations", createStation);
        stationResponse.EnsureSuccessStatusCode();
        var station = await stationResponse.Content.ReadFromJsonAsync<StationDto>();
        Assert.NotNull(station);

        var readings = new[]
        {
            new CreateReadingDto(10, 20, 10, 1000, DateTime.UtcNow.AddMinutes(-10)),
            new CreateReadingDto(20, 40, 20, 1020, DateTime.UtcNow)
        };

        foreach (var reading in readings)
        {
            var response = await _client.PostAsJsonAsync($"/api/stations/{station!.Id}/readings", reading);
            response.EnsureSuccessStatusCode();
        }

        var averageResponse = await _client.GetAsync($"/api/stations/{station.Id}/average");
        averageResponse.EnsureSuccessStatusCode();

        var average = await averageResponse.Content.ReadFromJsonAsync<AverageResultDto>();
        Assert.NotNull(average);
        Assert.Equal(15, average!.Temperature);
        Assert.Equal(30, average.Humidity);
        Assert.Equal(15, average.WindSpeedKmh);
        Assert.Equal(1010, average.Pressure);
    }

    [Fact]
    public async Task GetReadings_WithDateRange_FiltersResults()
    {
        var createStation = new CreateStationDto("Station C", 30, 30, 100, true);
        var stationResponse = await _client.PostAsJsonAsync("/api/stations", createStation);
        stationResponse.EnsureSuccessStatusCode();
        var station = await stationResponse.Content.ReadFromJsonAsync<StationDto>();
        Assert.NotNull(station);

        var first = new CreateReadingDto(10, 10, 10, 1000, DateTime.UtcNow.AddHours(-2));
        var second = new CreateReadingDto(15, 15, 15, 1005, DateTime.UtcNow.AddHours(-1));
        var third = new CreateReadingDto(20, 20, 20, 1010, DateTime.UtcNow);

        await _client.PostAsJsonAsync($"/api/stations/{station!.Id}/readings", first);
        await _client.PostAsJsonAsync($"/api/stations/{station.Id}/readings", second);
        await _client.PostAsJsonAsync($"/api/stations/{station.Id}/readings", third);

        var from = Uri.EscapeDataString(DateTime.UtcNow.AddHours(-1.5).ToString("o"));
        var to = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-30).ToString("o"));

        var response = await _client.GetAsync($"/api/stations/{station.Id}/readings?from={from}&to={to}");
        response.EnsureSuccessStatusCode();

        var readings = await response.Content.ReadFromJsonAsync<List<ReadingDto>>();
        Assert.NotNull(readings);
        Assert.Single(readings!);
        Assert.Equal(15, readings[0].Temperature);
    }
}
