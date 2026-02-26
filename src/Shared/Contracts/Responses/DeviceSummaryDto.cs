using POSMonitor.Shared.Enums;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record DeviceSummaryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("hostname")] string Hostname,
    [property: JsonPropertyName("ipAddress")] string IpAddress,
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("status")] DeviceStatus Status,
    [property: JsonPropertyName("networkStatus")] NetworkStatus NetworkStatus,
    [property: JsonPropertyName("lastHeartbeatAt")] DateTimeOffset? LastHeartbeatAt,
    [property: JsonPropertyName("groupId")] Guid? GroupId,
    [property: JsonPropertyName("groupName")] string? GroupName,
    [property: JsonPropertyName("companyCode")] string? CompanyCode,
    [property: JsonPropertyName("storeCode")] string? StoreCode
);
