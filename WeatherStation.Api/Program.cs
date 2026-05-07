using Microsoft.EntityFrameworkCore;
using WeatherStation.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeatherStationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" })).WithName("HealthCheck");

app.Run();
