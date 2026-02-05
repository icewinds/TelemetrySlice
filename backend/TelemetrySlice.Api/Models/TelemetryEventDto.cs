namespace TelemetrySlice.Api.Models;

public class TelemetryEventDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class TelemetryEventResponseDto
{
    public string EventId { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class DeviceDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class TelemetryInsightsDto
{
    public double? Latest { get; set; }
    public double? Min { get; set; }
    public double? Average { get; set; }
    public double? Max { get; set; }
    public int Count { get; set; }
    public string Unit { get; set; } = string.Empty;
}
