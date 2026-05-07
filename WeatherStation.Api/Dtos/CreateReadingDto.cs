namespace WeatherStation.Api.Dtos;

public record CreateReadingDto(
    double Temperature,
    double Humidity,
    double WindSpeedKmh,
    double Pressure,
    DateTime RecordedAt);
