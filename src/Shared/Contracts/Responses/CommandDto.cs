using POSMonitor.Shared.Enums;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record CommandDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("deviceId")] Guid DeviceId,
    [property: JsonPropertyName("commandType")] CommandType CommandType,
    [property: JsonPropertyName("status")] CommandStatus Status,
    [property: JsonPropertyName("payloadJson")] string PayloadJson,
    [property: JsonPropertyName("resultJson")] string? ResultJson,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTimeOffset? UpdatedAt,
    [property: JsonPropertyName("executedAt")] DateTimeOffset? ExecutedAt
);
