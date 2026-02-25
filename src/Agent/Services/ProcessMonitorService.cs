using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using POSMonitor.Agent.Options;
using POSMonitor.Shared.Models;

namespace POSMonitor.Agent.Services;

public class ProcessMonitorService
{
    private readonly MonitoringTargetsOptions _options;
    private readonly ILogger<ProcessMonitorService> _logger;

    public ProcessMonitorService(IOptions<MonitoringTargetsOptions> options, ILogger<ProcessMonitorService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<TrackedProcessDto>> CollectAsync(CancellationToken cancellationToken)
    {
        var processes = new List<TrackedProcessDto>();

        foreach (var target in _options.Processes)
        {
            var processName = Path.GetFileNameWithoutExtension(target.Name);
            if (string.IsNullOrWhiteSpace(processName))
            {
                processName = Path.GetFileNameWithoutExtension(target.Path);
            }

            var runningProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            var isRunning = runningProcess != null && !runningProcess.HasExited;
            DateTimeOffset? lastStart = null;
            double? memoryMb = null;

            if (isRunning)
            {
                try
                {
                    lastStart = runningProcess!.StartTime;
                    memoryMb = Math.Round(runningProcess.WorkingSet64 / 1024d / 1024, 2);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Unable to read metrics for process {Process}", target.Name);
                }
            }
            else if (target.AutoRestart && File.Exists(target.Path))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target.Path,
                        WorkingDirectory = Path.GetDirectoryName(target.Path),
                        UseShellExecute = true
                    });
                    isRunning = true;
                    _logger.LogInformation("Auto-started process {Process}", target.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start process {Process}", target.Name);
                }
            }

            processes.Add(new TrackedProcessDto(
                target.Name,
                target.Path,
                isRunning,
                null,
                memoryMb,
                lastStart));
        }

        return Task.FromResult((IReadOnlyCollection<TrackedProcessDto>)processes);
    }
}
