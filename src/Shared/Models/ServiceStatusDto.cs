using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Models;

public record ServiceStatusDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isRunning")] bool IsRunning,
    [property: JsonPropertyName("startupType")] string StartupType,
    [property: JsonPropertyName("lastStartTime")] DateTimeOffset? LastStartTime
);
