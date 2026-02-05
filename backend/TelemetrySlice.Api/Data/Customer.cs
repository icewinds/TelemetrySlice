namespace TelemetrySlice.Api.Data;

public class Customer
{
    public string CustomerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
