namespace WeatherStation.Api.Models;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
