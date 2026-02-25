namespace POSMonitor.Server.Data.Entities;

public class DeviceGroupNotificationWindow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceGroupId { get; set; }
    public DeviceGroup? DeviceGroup { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
