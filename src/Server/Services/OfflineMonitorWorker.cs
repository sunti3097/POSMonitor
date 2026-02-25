using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;
using POSMonitor.Server.Options;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Services;

public class OfflineMonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OfflineMonitorWorker> _logger;
    private readonly MonitoringOptions _options;

    public OfflineMonitorWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<MonitoringOptions> options,
        ILogger<OfflineMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OfflineMonitorWorker started with polling interval {Interval}s", _options.PollingIntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOfflineDevicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking offline devices");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task CheckOfflineDevicesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<POSMonitorDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var threshold = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(_options.OfflineThresholdMinutes);

        var offlineCandidates = await dbContext.Devices
            .Where(d => !d.LastHeartbeatAt.HasValue || d.LastHeartbeatAt < threshold)
            .ToListAsync(cancellationToken);

        foreach (var device in offlineCandidates)
        {
            if (device.Status != DeviceStatus.Offline)
            {
                device.Status = DeviceStatus.Offline;
                await notificationService.NotifyOfflineAsync(device.DeviceId, device.Hostname, device.LastHeartbeatAt ?? DateTimeOffset.MinValue, cancellationToken);
            }
        }

        var retentionThreshold = DateTimeOffset.UtcNow - TimeSpan.FromDays(_options.HeartbeatRetentionDays);
        var oldHeartbeats = await dbContext.Heartbeats
            .Where(h => h.ReportedAt < retentionThreshold)
            .ToListAsync(cancellationToken);

        if (oldHeartbeats.Count > 0)
        {
            dbContext.Heartbeats.RemoveRange(oldHeartbeats);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
