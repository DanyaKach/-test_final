using AutoFixture;
using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Models;

namespace WeatherStation.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(WeatherStationDbContext dbContext, int stationCount = 35)
    {
        if (await dbContext.Stations.AnyAsync())
        {
            return;
        }

        var fixture = new Fixture();
        var random = Random.Shared;
        var stations = new List<Station>(stationCount);

        for (var i = 1; i <= stationCount; i++)
        {
            var station = fixture.Build<Station>()
                .Without(x => x.Readings)
                .Without(x => x.Alerts)
                .With(x => x.Name, () => $"Station {i:00}")
                .With(x => x.Latitude, () => random.NextDouble() * 180 - 90)
                .With(x => x.Longitude, () => random.NextDouble() * 360 - 180)
                .With(x => x.Altitude, () => Math.Round(random.NextDouble() * 2500, 1))
                .With(x => x.IsActive, () => random.Next(0, 10) > 0)
                .Create();

            stations.Add(station);
        }

        await dbContext.Stations.AddRangeAsync(stations);
        await dbContext.SaveChangesAsync();

        var readings = new List<Reading>();
        var alerts = new List<Alert>();

        foreach (var station in stations)
        {
            if (!station.IsActive)
            {
                continue;
            }

            var readingCount = random.Next(250, 350);
            var recordedAt = DateTime.UtcNow.AddDays(-30);

            for (var j = 0; j < readingCount; j++)
            {
                recordedAt = recordedAt.AddMinutes(random.Next(15, 120));
                var temperature = GetTemperature(random);
                var windSpeed = GetWindSpeed(random);
                var humidity = GetHumidity(random);
                var pressure = GetPressure(random);

                var reading = fixture.Build<Reading>()
                    .Without(x => x.Station)
                    .With(x => x.StationId, station.Id)
                    .With(x => x.Temperature, temperature)
                    .With(x => x.Humidity, humidity)
                    .With(x => x.WindSpeedKmh, windSpeed)
                    .With(x => x.Pressure, pressure)
                    .With(x => x.RecordedAt, recordedAt)
                    .Create();

                readings.Add(reading);

                if (temperature > 40)
                {
                    alerts.Add(CreateAlert(station.Id, AlertType.HighTemp, $"High temperature detected: {temperature:F1}°C."));
                }

                if (windSpeed > 100)
                {
                    alerts.Add(CreateAlert(station.Id, AlertType.HighWind, $"High wind speed detected: {windSpeed:F1} km/h."));
                }
            }
        }

        await dbContext.Readings.AddRangeAsync(readings);
        await dbContext.Alerts.AddRangeAsync(alerts);
        await dbContext.SaveChangesAsync();
    }

    private static Alert CreateAlert(int stationId, AlertType type, string message)
        => new()
        {
            StationId = stationId,
            Type = type,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsAcknowledged = false
        };

    private static double GetTemperature(Random random)
    {
        var chance = random.NextDouble();
        if (chance < 0.04)
        {
            return random.NextDouble() * 20 + 40; // high temp alerts
        }

        return random.NextDouble() * 45 - 15; // typical range -15..30
    }

    private static double GetWindSpeed(Random random)
    {
        var chance = random.NextDouble();
        if (chance < 0.08)
        {
            return random.NextDouble() * 50 + 100; // high wind alerts
        }

        return random.NextDouble() * 80; // typical range 0..80
    }

    private static double GetHumidity(Random random)
        => random.NextDouble() * 100;

    private static double GetPressure(Random random)
        => 980 + random.NextDouble() * 60;
}
