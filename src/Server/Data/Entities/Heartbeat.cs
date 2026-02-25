using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Data.Entities;

public class Heartbeat
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public NetworkStatus NetworkStatus { get; set; }
    public string HardwareSnapshotJson { get; set; } = string.Empty;
    public string ServicesJson { get; set; } = string.Empty;
    public string ProcessesJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
