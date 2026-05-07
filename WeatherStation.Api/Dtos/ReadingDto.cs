namespace WeatherStation.Api.Dtos;

public record ReadingDto(
    int Id,
    int StationId,
    double Temperature,
    double Humidity,
    double WindSpeedKmh,
    double Pressure,
    DateTime RecordedAt);
