using POSMonitor.Shared.Enums;
using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record CommandResultRequest(
    [property: JsonPropertyName("status")] CommandStatus Status,
    [property: JsonPropertyName("succeeded")] bool Succeeded,
    [property: JsonPropertyName("output")] string Output,
    [property: JsonPropertyName("completedAt")] DateTimeOffset CompletedAt
);
