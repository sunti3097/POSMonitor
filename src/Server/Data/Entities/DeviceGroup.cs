namespace POSMonitor.Server.Data.Entities;

public class DeviceGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<DeviceGroupNotificationWindow> NotificationWindows { get; set; } = new List<DeviceGroupNotificationWindow>();
    public ICollection<DeviceGroupAssignment> DeviceAssignments { get; set; } = new List<DeviceGroupAssignment>();
}
