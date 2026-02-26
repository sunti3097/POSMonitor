using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Data.Entities;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;
    public NetworkStatus NetworkStatus { get; set; } = NetworkStatus.Unknown;
    public DateTimeOffset? LastHeartbeatAt { get; set; }
    public string LastHardwareSnapshotJson { get; set; } = string.Empty;
    public string LastServicesJson { get; set; } = string.Empty;
    public string LastProcessesJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CompanyCode { get; set; }
    public string? StoreCode { get; set; }

    public ICollection<Heartbeat> Heartbeats { get; set; } = new List<Heartbeat>();
    public ICollection<Command> Commands { get; set; } = new List<Command>();
    public ICollection<DeviceGroupAssignment> GroupAssignments { get; set; } = new List<DeviceGroupAssignment>();
}
