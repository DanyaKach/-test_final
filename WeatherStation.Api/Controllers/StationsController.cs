using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;
using WeatherStation.Api.Dtos;
using WeatherStation.Api.Services;
using WeatherStation.Api.Models;

namespace WeatherStation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly WeatherStationDbContext _dbContext;
    private readonly IReadingService _readingService;

    public StationsController(WeatherStationDbContext dbContext, IReadingService readingService)
    {
        _dbContext = dbContext;
        _readingService = readingService;
    }

    [HttpGet]
    public async Task<IEnumerable<StationDto>> GetStations()
    {
        return await _dbContext.Stations
            .Select(x => new StationDto(x.Id, x.Name, x.Latitude, x.Longitude, x.Altitude, x.IsActive))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<StationDto>> CreateStation([FromBody] CreateStationDto dto)
    {
        var station = new Station
        {
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Altitude = dto.Altitude,
            IsActive = dto.IsActive
        };

        _dbContext.Stations.Add(station);
        await _dbContext.SaveChangesAsync();

        var result = new StationDto(station.Id, station.Name, station.Latitude, station.Longitude, station.Altitude, station.IsActive);
        return CreatedAtAction(nameof(GetStations), new { id = station.Id }, result);
    }

    [HttpPost("{id}/readings")]
    public async Task<ActionResult<ReadingDto>> AddReading(int id, [FromBody] CreateReadingDto dto)
    {
        try
        {
            var reading = await _readingService.AddReadingAsync(id, dto);
            return CreatedAtAction(nameof(GetLatestReading), new { id }, reading);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/readings")]
    public async Task<ActionResult<IEnumerable<ReadingDto>>> GetReadings(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stationExists = await _dbContext.Stations.AnyAsync(x => x.Id == id);
        if (!stationExists)
        {
            return NotFound();
        }

        return Ok(await _readingService.GetReadingsAsync(id, from, to));
    }

    [HttpGet("{id}/latest")]
    public async Task<ActionResult<ReadingDto>> GetLatestReading(int id)
    {
        var stationExists = await _dbContext.Stations.AnyAsync(x => x.Id == id);
        if (!stationExists)
        {
            return NotFound();
        }

        var reading = await _readingService.GetLatestReadingAsync(id);
        return reading is null ? NotFound() : Ok(reading);
    }

    [HttpGet("{id}/average")]
    public async Task<ActionResult<AverageResultDto>> GetAverage(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stationExists = await _dbContext.Stations.AnyAsync(x => x.Id == id);
        if (!stationExists)
        {
            return NotFound();
        }

        try
        {
            var average = await _readingService.GetAverageAsync(id, from, to);
            return Ok(average);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
