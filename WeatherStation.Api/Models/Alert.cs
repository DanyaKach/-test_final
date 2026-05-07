namespace WeatherStation.Api.Models;

public class Alert
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }

    public AlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsAcknowledged { get; set; }
}
