using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;
using WeatherStation.Api.Dtos;

namespace WeatherStation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly WeatherStationDbContext _dbContext;

    public AlertsController(WeatherStationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IEnumerable<AlertDto>> GetActiveAlerts()
    {
        return await _dbContext.Alerts
            .Where(x => !x.IsAcknowledged)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AlertDto(
                x.Id,
                x.StationId,
                x.Type.ToString(),
                x.Message,
                x.CreatedAt,
                x.IsAcknowledged))
            .ToListAsync();
    }

    [HttpPatch("{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        var alert = await _dbContext.Alerts.FindAsync(id);
        if (alert is null)
        {
            return NotFound();
        }

        alert.IsAcknowledged = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
