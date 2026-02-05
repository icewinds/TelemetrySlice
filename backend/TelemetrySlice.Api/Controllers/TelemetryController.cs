using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemetrySlice.Api.Data;
using TelemetrySlice.Api.Models;

namespace TelemetrySlice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryDbContext _context;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(TelemetryDbContext context, ILogger<TelemetryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Submit a telemetry event from a device
    /// Handles deduplication based on EventId and supports out-of-order arrivals
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitTelemetry([FromBody] TelemetryEventDto dto)
    {
        try
        {
            // Validate customer ID (in production, this would come from auth token)
            var customerId = dto.CustomerId;
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(new { error = "CustomerId is required" });
            }

            // Check if device exists and belongs to the customer
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.CustomerId == customerId && d.DeviceId == dto.DeviceId);

            if (device == null)
            {
                return NotFound(new { error = $"Device {dto.DeviceId} not found for customer {customerId}" });
            }

            // Check for duplicate event (idempotency)
            var existingEvent = await _context.TelemetryEvents
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (existingEvent != null)
            {
                _logger.LogInformation("Duplicate event {EventId} received, ignoring", dto.EventId);
                return Ok(new { message = "Event already processed", eventId = dto.EventId, isDuplicate = true });
            }

            // Create new telemetry event
            var telemetryEvent = new TelemetryEvent
            {
                CustomerId = dto.CustomerId,
                DeviceId = dto.DeviceId,
                EventId = dto.EventId,
                RecordedAt = dto.RecordedAt,
                ReceivedAt = DateTime.UtcNow,
                Type = dto.Type,
                Value = dto.Value,
                Unit = dto.Unit
            };

            _context.TelemetryEvents.Add(telemetryEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Telemetry event {EventId} processed successfully", dto.EventId);

            return Ok(new { message = "Event processed successfully", eventId = dto.EventId, isDuplicate = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry event");
            return StatusCode(500, new { error = "An error occurred while processing the event" });
        }
    }

    /// <summary>
    /// Get telemetry events for a specific device within the last 24 hours
    /// Supports tenant isolation via customerId parameter
    /// </summary>
    [HttpGet("{customerId}/{deviceId}")]
    public async Task<IActionResult> GetTelemetry(string customerId, string deviceId, [FromQuery] int hours = 24)
    {
        try
        {
            var since = DateTime.UtcNow.AddHours(-hours);

            var telemetryEvents = await _context.TelemetryEvents
                .Where(e => e.CustomerId == customerId 
                    && e.DeviceId == deviceId 
                    && e.RecordedAt >= since)
                .OrderBy(e => e.RecordedAt)
                .Select(e => new TelemetryEventResponseDto
                {
                    EventId = e.EventId,
                    RecordedAt = e.RecordedAt,
                    ReceivedAt = e.ReceivedAt,
                    Type = e.Type,
                    Value = e.Value,
                    Unit = e.Unit
                })
                .ToListAsync();

            return Ok(telemetryEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving telemetry for device {DeviceId}", deviceId);
            return StatusCode(500, new { error = "An error occurred while retrieving telemetry data" });
        }
    }

    /// <summary>
    /// Get insights (min, max, average, latest) for a device's telemetry
    /// </summary>
    [HttpGet("{customerId}/{deviceId}/insights")]
    public async Task<IActionResult> GetInsights(string customerId, string deviceId, [FromQuery] int hours = 24)
    {
        try
        {
            var since = DateTime.UtcNow.AddHours(-hours);

            var telemetryData = await _context.TelemetryEvents
                .Where(e => e.CustomerId == customerId 
                    && e.DeviceId == deviceId 
                    && e.RecordedAt >= since)
                .OrderByDescending(e => e.RecordedAt)
                .ToListAsync();

            if (!telemetryData.Any())
            {
                return Ok(new TelemetryInsightsDto 
                { 
                    Count = 0,
                    Unit = "C"
                });
            }

            var insights = new TelemetryInsightsDto
            {
                Latest = telemetryData.First().Value,
                Min = telemetryData.Min(e => e.Value),
                Average = telemetryData.Average(e => e.Value),
                Max = telemetryData.Max(e => e.Value),
                Count = telemetryData.Count,
                Unit = telemetryData.First().Unit
            };

            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating insights for device {DeviceId}", deviceId);
            return StatusCode(500, new { error = "An error occurred while calculating insights" });
        }
    }
}
