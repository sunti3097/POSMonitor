using System.Diagnostics;
using System.IO;
using System.Management;
using POSMonitor.Shared.Models;

namespace POSMonitor.Agent.Services;

public class HardwareMetricsProvider : IDisposable
{
    private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");

    public async Task<HardwareSnapshotDto> CaptureAsync(CancellationToken cancellationToken)
    {
        _ = _cpuCounter.NextValue();
        await Task.Delay(500, cancellationToken);
        var cpuPercent = Math.Round(_cpuCounter.NextValue(), 2);

        var (memoryPercent, totalGb, freeGb) = GetMemoryUsage();
        var (diskFree, diskTotal) = GetDiskInfo();
        var diskUsed = diskTotal - diskFree;
        var diskPercent = diskTotal <= 0 ? 0 : Math.Round((diskUsed / diskTotal) * 100, 2);
        var uptimeSeconds = (long)(TimeSpan.FromMilliseconds(Environment.TickCount64).TotalSeconds);

        return new HardwareSnapshotDto(
            cpuPercent,
            memoryPercent,
            diskPercent,
            diskFree,
            diskTotal,
            totalGb,
            freeGb,
            uptimeSeconds);
    }

    private static (double memoryPercent, double totalGb, double freeGb) GetMemoryUsage()
    {
        double totalGb = 0;
        double freeGb = 0;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            var metrics = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (metrics != null)
            {
                var totalKb = Convert.ToDouble(metrics["TotalVisibleMemorySize"]);
                var freeKb = Convert.ToDouble(metrics["FreePhysicalMemory"]);
                totalGb = Math.Round(totalKb / 1024 / 1024, 2);
                freeGb = Math.Round(freeKb / 1024 / 1024, 2);
            }
        }
        catch
        {
            totalGb = freeGb = 0;
        }

        var used = totalGb - freeGb;
        var percent = totalGb <= 0 ? 0 : Math.Round((used / totalGb) * 100, 2);
        return (percent, totalGb, freeGb);
    }

    private static (double freeGb, double totalGb) GetDiskInfo()
    {
        try
        {
            var systemDrive = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .OrderByDescending(d => d.TotalSize)
                .FirstOrDefault();

            if (systemDrive == null)
            {
                return (0, 0);
            }

            var free = Math.Round(systemDrive.TotalFreeSpace / 1024d / 1024 / 1024, 2);
            var total = Math.Round(systemDrive.TotalSize / 1024d / 1024 / 1024, 2);
            return (free, total);
        }
        catch
        {
            return (0, 0);
        }
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
    }
}
