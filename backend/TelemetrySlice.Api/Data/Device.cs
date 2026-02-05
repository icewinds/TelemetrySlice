namespace TelemetrySlice.Api.Data;

public class Device
{
    public string CustomerId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public Customer? Customer { get; set; }
    public ICollection<TelemetryEvent> TelemetryEvents { get; set; } = new List<TelemetryEvent>();
}
