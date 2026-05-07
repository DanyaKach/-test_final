namespace WeatherStation.Api.Dtos;

public record AlertDto(
    int Id,
    int StationId,
    string Type,
    string Message,
    DateTime CreatedAt,
    bool IsAcknowledged);
