using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelemetrySlice.Api.Data;

namespace TelemetrySlice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly TelemetryDbContext _context;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(TelemetryDbContext context, ILogger<CustomersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers (for demo purposes - tenant selection in UI)
    /// In production, this would be restricted based on user permissions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        try
        {
            var customers = await _context.Customers
                .Select(c => new
                {
                    c.CustomerId,
                    c.Name
                })
                .ToListAsync();

            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, new { error = "An error occurred while retrieving customers" });
        }
    }
}
