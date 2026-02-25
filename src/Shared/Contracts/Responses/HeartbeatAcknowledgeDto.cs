using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record HeartbeatAcknowledgeDto(
    [property: JsonPropertyName("accepted")] bool Accepted,
    [property: JsonPropertyName("pendingCommands")] IReadOnlyCollection<PendingCommandDto> PendingCommands
);
