using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;
using WeatherStation.Api.Dtos;
using WeatherStation.Api.Models;

namespace WeatherStation.Api.Services;

public class ReadingService : IReadingService
{
    private readonly WeatherStationDbContext _dbContext;

    public ReadingService(WeatherStationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReadingDto> AddReadingAsync(int stationId, CreateReadingDto readingDto)
    {
        var station = await _dbContext.Stations.FindAsync(stationId);
        if (station is null)
        {
            throw new InvalidOperationException("Station not found.");
        }

        if (!station.IsActive)
        {
            throw new InvalidOperationException("Station is not active and cannot receive readings.");
        }

        ValidateReading(readingDto);

        var lastReading = await _dbContext.Readings
            .Where(x => x.StationId == stationId)
            .OrderByDescending(x => x.RecordedAt)
            .FirstOrDefaultAsync();

        if (lastReading is not null && readingDto.RecordedAt <= lastReading.RecordedAt)
        {
            throw new InvalidOperationException("Reading must be recorded in chronological order.");
        }

        var reading = new Reading
        {
            StationId = stationId,
            Temperature = readingDto.Temperature,
            Humidity = readingDto.Humidity,
            WindSpeedKmh = readingDto.WindSpeedKmh,
            Pressure = readingDto.Pressure,
            RecordedAt = readingDto.RecordedAt.ToUniversalTime()
        };

        _dbContext.Readings.Add(reading);
        await CreateAlertsForReadingAsync(reading, stationId);
        await _dbContext.SaveChangesAsync();

        return ToDto(reading);
    }

    public async Task<IEnumerable<ReadingDto>> GetReadingsAsync(int stationId, DateTime? from, DateTime? to)
    {
        var query = _dbContext.Readings.Where(x => x.StationId == stationId);

        if (from.HasValue)
        {
            query = query.Where(x => x.RecordedAt >= from.Value.ToUniversalTime());
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.RecordedAt <= to.Value.ToUniversalTime());
        }

        return await query
            .OrderBy(x => x.RecordedAt)
            .Select(x => ToDto(x))
            .ToListAsync();
    }

    public async Task<ReadingDto?> GetLatestReadingAsync(int stationId)
    {
        var reading = await _dbContext.Readings
            .Where(x => x.StationId == stationId)
            .OrderByDescending(x => x.RecordedAt)
            .FirstOrDefaultAsync();

        return reading is null ? null : ToDto(reading);
    }

    public async Task<AverageResultDto> GetAverageAsync(int stationId, DateTime? from, DateTime? to)
    {
        var query = _dbContext.Readings.Where(x => x.StationId == stationId);

        if (from.HasValue)
        {
            query = query.Where(x => x.RecordedAt >= from.Value.ToUniversalTime());
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.RecordedAt <= to.Value.ToUniversalTime());
        }

        var results = await query.ToListAsync();

        if (!results.Any())
        {
            throw new InvalidOperationException("No readings found for the requested period.");
        }

        return new AverageResultDto(
            results.Average(x => x.Temperature),
            results.Average(x => x.Humidity),
            results.Average(x => x.WindSpeedKmh),
            results.Average(x => x.Pressure));
    }

    private static void ValidateReading(CreateReadingDto readingDto)
    {
        if (readingDto.Temperature < -60 || readingDto.Temperature > 60)
        {
            throw new InvalidOperationException("Temperature must be between -60 and 60 degrees Celsius.");
        }

        if (readingDto.Humidity < 0 || readingDto.Humidity > 100)
        {
            throw new InvalidOperationException("Humidity must be between 0 and 100 percent.");
        }
    }

    private async Task CreateAlertsForReadingAsync(Reading reading, int stationId)
    {
        if (reading.Temperature > 40)
        {
            _dbContext.Alerts.Add(new Alert
            {
                StationId = stationId,
                Type = AlertType.HighTemp,
                Message = $"High temperature detected: {reading.Temperature}°C.",
                CreatedAt = DateTime.UtcNow,
                IsAcknowledged = false
            });
        }

        if (reading.WindSpeedKmh > 100)
        {
            _dbContext.Alerts.Add(new Alert
            {
                StationId = stationId,
                Type = AlertType.HighWind,
                Message = $"High wind speed detected: {reading.WindSpeedKmh} km/h.",
                CreatedAt = DateTime.UtcNow,
                IsAcknowledged = false
            });
        }
    }

    private static ReadingDto ToDto(Reading reading)
        => new(
            reading.Id,
            reading.StationId,
            reading.Temperature,
            reading.Humidity,
            reading.WindSpeedKmh,
            reading.Pressure,
            reading.RecordedAt);
}
