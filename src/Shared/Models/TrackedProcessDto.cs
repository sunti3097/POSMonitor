using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Models;

public record TrackedProcessDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("isRunning")] bool IsRunning,
    [property: JsonPropertyName("cpuPercent")] double? CpuPercent,
    [property: JsonPropertyName("memoryMb")] double? MemoryMb,
    [property: JsonPropertyName("lastStartTime")] DateTimeOffset? LastStartTime
);
