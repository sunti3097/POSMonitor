using POSMonitor.Server.Data.Entities;

namespace POSMonitor.Server.Utilities;

public static class NotificationWindowEvaluator
{
    public static bool IsWithinWindow(Device device, DateTimeOffset now)
    {
        var groups = device.GroupAssignments
            .Select(ga => ga.DeviceGroup)
            .Where(g => g != null)
            .Cast<DeviceGroup>()
            .ToList();

        if (groups.Count == 0)
        {
            return true;
        }

        var windows = groups.SelectMany(g => g.NotificationWindows).ToList();
        if (windows.Count == 0)
        {
            return true;
        }

        var today = now.DayOfWeek;
        var currentTime = now.TimeOfDay;

        return windows.Any(window =>
            window.DayOfWeek == today &&
            currentTime >= window.StartTime &&
            currentTime <= window.EndTime);
    }
}
