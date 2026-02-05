namespace TelemetrySlice.Api.Data;

public class TelemetryEvent
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    
    public Device? Device { get; set; }
}
