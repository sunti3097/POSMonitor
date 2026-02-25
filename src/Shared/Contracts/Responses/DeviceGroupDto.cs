using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record DeviceGroupDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("notificationWindows")] IReadOnlyCollection<NotificationWindowDto> NotificationWindows
);
