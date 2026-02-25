using POSMonitor.Server.Options;

namespace POSMonitor.Server.Services;

public interface INotificationService
{
    Task NotifyOfflineAsync(string deviceId, string hostname, DateTimeOffset lastSeen, CancellationToken cancellationToken = default);
}
