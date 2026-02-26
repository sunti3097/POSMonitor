using POSMonitor.Shared.Enums;
using POSMonitor.Shared.Models;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record HeartbeatReport(
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("hostname")] string Hostname,
    [property: JsonPropertyName("ipAddress")] string IpAddress,
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("networkStatus")] NetworkStatus NetworkStatus,
    [property: JsonPropertyName("hardware")] HardwareSnapshotDto Hardware,
    [property: JsonPropertyName("services")] IReadOnlyCollection<ServiceStatusDto> Services,
    [property: JsonPropertyName("trackedProcesses")] IReadOnlyCollection<TrackedProcessDto> TrackedProcesses,
    [property: JsonPropertyName("reportedAt")] DateTimeOffset ReportedAt
);
