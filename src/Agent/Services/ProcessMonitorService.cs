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
            var processName = Path.GetFileNameWithoutExtension(target.ProcessName);
            if (string.IsNullOrWhiteSpace(processName))
            {
                processName = Path.GetFileNameWithoutExtension(target.ExecutablePath);
            }

            var runningProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            bool isRunning = false;
            DateTimeOffset? lastStart = null;
            double? memoryMb = null;

            if (runningProcess != null)
            {
                try
                {
                    isRunning = !runningProcess.HasExited;
                }
                catch
                {
                    isRunning = true;
                }
            }

            if (isRunning)
            {
                try
                {
                    lastStart = runningProcess!.StartTime;
                    memoryMb = Math.Round(runningProcess.WorkingSet64 / 1024d / 1024, 2);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Unable to read metrics for process {Process}", target.ProcessName);
                }
            }
            else if (target.AutoRestart && File.Exists(target.ExecutablePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target.ExecutablePath,
                        WorkingDirectory = Path.GetDirectoryName(target.ExecutablePath),
                        UseShellExecute = true
                    });
                    isRunning = true;
                    _logger.LogInformation("Auto-started process {Process}", target.ProcessName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start process {Process}", target.ProcessName);
                }
            }

            processes.Add(new TrackedProcessDto(
                target.DisplayName ?? target.ProcessName,
                target.ExecutablePath,
                isRunning,
                null,
                memoryMb,
                lastStart));
        }

        return Task.FromResult((IReadOnlyCollection<TrackedProcessDto>)processes);
    }
}
