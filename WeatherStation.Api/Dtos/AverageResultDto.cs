namespace WeatherStation.Api.Dtos;

public record AverageResultDto(
    double Temperature,
    double Humidity,
    double WindSpeedKmh,
    double Pressure);
