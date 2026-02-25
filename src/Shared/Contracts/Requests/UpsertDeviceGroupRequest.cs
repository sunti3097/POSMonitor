using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record UpsertDeviceGroupRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("notificationWindows")] IReadOnlyCollection<NotificationWindowRequest> NotificationWindows
);
