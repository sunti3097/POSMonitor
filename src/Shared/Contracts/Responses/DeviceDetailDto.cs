using POSMonitor.Shared.Enums;
using POSMonitor.Shared.Models;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record DeviceDetailDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("hostname")] string Hostname,
    [property: JsonPropertyName("ipAddress")] string IpAddress,
    [property: JsonPropertyName("status")] DeviceStatus Status,
    [property: JsonPropertyName("networkStatus")] NetworkStatus NetworkStatus,
    [property: JsonPropertyName("lastHeartbeatAt")] DateTimeOffset? LastHeartbeatAt,
    [property: JsonPropertyName("hardware")] HardwareSnapshotDto? Hardware,
    [property: JsonPropertyName("services")] IReadOnlyCollection<ServiceStatusDto> Services,
    [property: JsonPropertyName("processes")] IReadOnlyCollection<TrackedProcessDto> Processes,
    [property: JsonPropertyName("groups")] IReadOnlyCollection<DeviceGroupDto> Groups
);
