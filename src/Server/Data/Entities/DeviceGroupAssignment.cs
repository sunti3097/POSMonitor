namespace POSMonitor.Server.Data.Entities;

public class DeviceGroupAssignment
{
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }

    public Guid DeviceGroupId { get; set; }
    public DeviceGroup? DeviceGroup { get; set; }

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}
