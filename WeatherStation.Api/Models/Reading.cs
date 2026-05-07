namespace WeatherStation.Api.Models;

public class Reading
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }

    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double WindSpeedKmh { get; set; }
    public double Pressure { get; set; }
    public DateTime RecordedAt { get; set; }
}
