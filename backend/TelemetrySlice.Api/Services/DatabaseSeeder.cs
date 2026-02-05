using TelemetrySlice.Api.Data;

namespace TelemetrySlice.Api.Services;

public class DatabaseSeeder
{
    private readonly TelemetryDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(TelemetryDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if data already exists
            if (_context.Customers.Any())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Seed Customers
            var customers = new[]
            {
                new Customer
                {
                    CustomerId = "acme-123",
                    Name = "Acme Corporation",
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    CustomerId = "beta-456",
                    Name = "Beta Industries",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();

            // Seed Devices
            var devices = new[]
            {
                new Device
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    Label = "Boiler #3",
                    Location = "Plant A",
                    CreatedAt = DateTime.UtcNow
                },
                new Device
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    Label = "Chiller #1",
                    Location = "Plant A",
                    CreatedAt = DateTime.UtcNow
                },
                new Device
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    Label = "Pump #9",
                    Location = "Site B",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await _context.Devices.AddRangeAsync(devices);
            await _context.SaveChangesAsync();

            // Seed Telemetry Events (using dates relative to now for more realistic data)
            var now = DateTime.UtcNow;
            var telemetryEvents = new[]
            {
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a0",
                    RecordedAt = now.AddHours(-23).AddMinutes(-30),
                    ReceivedAt = now.AddHours(-23).AddMinutes(-29),
                    Type = "temperature",
                    Value = 21.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a1",
                    RecordedAt = now.AddHours(-23),
                    ReceivedAt = now.AddHours(-22).AddMinutes(-59),
                    Type = "temperature",
                    Value = 21.5,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a2",
                    RecordedAt = now.AddHours(-22).AddMinutes(-30),
                    ReceivedAt = now.AddHours(-22).AddMinutes(-29),
                    Type = "temperature",
                    Value = 22.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a3",
                    RecordedAt = now.AddHours(-20),
                    ReceivedAt = now.AddHours(-19).AddMinutes(-59),
                    Type = "temperature",
                    Value = 22.5,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a4",
                    RecordedAt = now.AddHours(-18),
                    ReceivedAt = now.AddHours(-17).AddMinutes(-59),
                    Type = "temperature",
                    Value = 23.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a5",
                    RecordedAt = now.AddHours(-15),
                    ReceivedAt = now.AddHours(-14).AddMinutes(-59),
                    Type = "temperature",
                    Value = 22.8,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a6",
                    RecordedAt = now.AddHours(-12),
                    ReceivedAt = now.AddHours(-11).AddMinutes(-59),
                    Type = "temperature",
                    Value = 22.2,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a7",
                    RecordedAt = now.AddHours(-8),
                    ReceivedAt = now.AddHours(-7).AddMinutes(-59),
                    Type = "temperature",
                    Value = 21.8,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a8",
                    RecordedAt = now.AddHours(-4),
                    ReceivedAt = now.AddHours(-3).AddMinutes(-59),
                    Type = "temperature",
                    Value = 21.3,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-001",
                    EventId = "evt-a9",
                    RecordedAt = now.AddHours(-1),
                    ReceivedAt = now.AddMinutes(-59),
                    Type = "temperature",
                    Value = 21.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    EventId = "evt-b1",
                    RecordedAt = now.AddHours(-20),
                    ReceivedAt = now.AddHours(-19).AddMinutes(-59),
                    Type = "temperature",
                    Value = 6.8,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    EventId = "evt-b2",
                    RecordedAt = now.AddHours(-16),
                    ReceivedAt = now.AddHours(-15).AddMinutes(-59),
                    Type = "temperature",
                    Value = 7.2,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    EventId = "evt-b3",
                    RecordedAt = now.AddHours(-12),
                    ReceivedAt = now.AddHours(-11).AddMinutes(-59),
                    Type = "temperature",
                    Value = 6.5,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    EventId = "evt-b4",
                    RecordedAt = now.AddHours(-8),
                    ReceivedAt = now.AddHours(-7).AddMinutes(-59),
                    Type = "temperature",
                    Value = 7.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "acme-123",
                    DeviceId = "dev-002",
                    EventId = "evt-b5",
                    RecordedAt = now.AddHours(-4),
                    ReceivedAt = now.AddHours(-3).AddMinutes(-59),
                    Type = "temperature",
                    Value = 6.9,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    EventId = "evt-c1",
                    RecordedAt = now.AddHours(-19),
                    ReceivedAt = now.AddHours(-18).AddMinutes(-59),
                    Type = "temperature",
                    Value = 55.2,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    EventId = "evt-c2",
                    RecordedAt = now.AddHours(-15),
                    ReceivedAt = now.AddHours(-14).AddMinutes(-59),
                    Type = "temperature",
                    Value = 54.8,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    EventId = "evt-c3",
                    RecordedAt = now.AddHours(-11),
                    ReceivedAt = now.AddHours(-10).AddMinutes(-59),
                    Type = "temperature",
                    Value = 56.0,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    EventId = "evt-c4",
                    RecordedAt = now.AddHours(-7),
                    ReceivedAt = now.AddHours(-6).AddMinutes(-59),
                    Type = "temperature",
                    Value = 55.5,
                    Unit = "C"
                },
                new TelemetryEvent
                {
                    CustomerId = "beta-456",
                    DeviceId = "dev-100",
                    EventId = "evt-c5",
                    RecordedAt = now.AddHours(-3),
                    ReceivedAt = now.AddHours(-2).AddMinutes(-59),
                    Type = "temperature",
                    Value = 55.0,
                    Unit = "C"
                }
            };

            await _context.TelemetryEvents.AddRangeAsync(telemetryEvents);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
