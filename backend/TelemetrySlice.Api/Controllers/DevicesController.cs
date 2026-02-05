using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemetrySlice.Api.Data;
using TelemetrySlice.Api.Models;

namespace TelemetrySlice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly TelemetryDbContext _context;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(TelemetryDbContext context, ILogger<DevicesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all devices for a specific customer
    /// Demonstrates tenant isolation
    /// </summary>
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetDevices(string customerId)
    {
        try
        {
            // Verify customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            if (!customerExists)
            {
                return NotFound(new { error = $"Customer {customerId} not found" });
            }

            var devices = await _context.Devices
                .Where(d => d.CustomerId == customerId)
                .Select(d => new DeviceDto
                {
                    CustomerId = d.CustomerId,
                    DeviceId = d.DeviceId,
                    Label = d.Label,
                    Location = d.Location
                })
                .ToListAsync();

            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving devices for customer {CustomerId}", customerId);
            return StatusCode(500, new { error = "An error occurred while retrieving devices" });
        }
    }

    /// <summary>
    /// Get a specific device
    /// </summary>
    [HttpGet("{customerId}/{deviceId}")]
    public async Task<IActionResult> GetDevice(string customerId, string deviceId)
    {
        try
        {
            var device = await _context.Devices
                .Where(d => d.CustomerId == customerId && d.DeviceId == deviceId)
                .Select(d => new DeviceDto
                {
                    CustomerId = d.CustomerId,
                    DeviceId = d.DeviceId,
                    Label = d.Label,
                    Location = d.Location
                })
                .FirstOrDefaultAsync();

            if (device == null)
            {
                return NotFound(new { error = $"Device {deviceId} not found for customer {customerId}" });
            }

            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving device {DeviceId}", deviceId);
            return StatusCode(500, new { error = "An error occurred while retrieving the device" });
        }
    }
}
