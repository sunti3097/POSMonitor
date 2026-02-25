using POSMonitor.Shared.Enums;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record PendingCommandDto(
    [property: JsonPropertyName("commandId")] Guid CommandId,
    [property: JsonPropertyName("commandType")] CommandType CommandType,
    [property: JsonPropertyName("payloadJson")] string PayloadJson,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt
);
