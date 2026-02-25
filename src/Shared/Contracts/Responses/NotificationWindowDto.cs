using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Responses;

public record NotificationWindowDto(
    [property: JsonPropertyName("dayOfWeek")] DayOfWeek DayOfWeek,
    [property: JsonPropertyName("startTime")] string StartTime,
    [property: JsonPropertyName("endTime")] string EndTime
);
