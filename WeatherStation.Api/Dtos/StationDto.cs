namespace WeatherStation.Api.Dtos;

public record StationDto(
    int Id,
    string Name,
    double Latitude,
    double Longitude,
    double Altitude,
    bool IsActive);
