using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Models;

public record HardwareSnapshotDto(
    [property: JsonPropertyName("cpuPercent")] double CpuPercent,
    [property: JsonPropertyName("memoryPercent")] double MemoryPercent,
    [property: JsonPropertyName("diskFreeGb")] double DiskFreeGb,
    [property: JsonPropertyName("diskTotalGb")] double DiskTotalGb,
    [property: JsonPropertyName("uptimeSeconds")] long UptimeSeconds
);
