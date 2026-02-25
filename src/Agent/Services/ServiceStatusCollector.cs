using Microsoft.Extensions.Options;
using POSMonitor.Agent.Options;
using POSMonitor.Shared.Models;
using System.ServiceProcess;

namespace POSMonitor.Agent.Services;

public class ServiceStatusCollector
{
    private readonly MonitoringTargetsOptions _options;
    private readonly ILogger<ServiceStatusCollector> _logger;

    public ServiceStatusCollector(IOptions<MonitoringTargetsOptions> options, ILogger<ServiceStatusCollector> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<ServiceStatusDto>> CollectAsync(CancellationToken cancellationToken)
    {
        var statuses = new List<ServiceStatusDto>();
        foreach (var target in _options.Services)
        {
            try
            {
                using var controller = new ServiceController(target.Name);
                var isRunning = controller.Status == ServiceControllerStatus.Running;
                statuses.Add(new ServiceStatusDto(
                    target.DisplayName ?? target.Name,
                    isRunning,
                    controller.ServiceType.ToString(),
                    null));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to query service {Service}", target.Name);
                statuses.Add(new ServiceStatusDto(target.Name, false, "Unknown", null));
            }
        }

        return Task.FromResult((IReadOnlyCollection<ServiceStatusDto>)statuses);
    }
}
