namespace WeatherStation.Api.Dtos;

public record CreateStationDto(
    string Name,
    double Latitude,
    double Longitude,
    double Altitude,
    bool IsActive = true);
