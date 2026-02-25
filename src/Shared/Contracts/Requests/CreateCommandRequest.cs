using POSMonitor.Shared.Enums;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record CreateCommandRequest(
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("commandType")] CommandType CommandType,
    [property: JsonPropertyName("payloadJson")] string PayloadJson,
    [property: JsonPropertyName("requestedBy")] string RequestedBy
);
