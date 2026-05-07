using WeatherStation.Api.Dtos;

namespace WeatherStation.Api.Services;

public interface IReadingService
{
    Task<ReadingDto> AddReadingAsync(int stationId, CreateReadingDto readingDto);
    Task<IEnumerable<ReadingDto>> GetReadingsAsync(int stationId, DateTime? from, DateTime? to);
    Task<ReadingDto?> GetLatestReadingAsync(int stationId);
    Task<AverageResultDto> GetAverageAsync(int stationId, DateTime? from, DateTime? to);
}
