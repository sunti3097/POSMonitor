using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record NotificationWindowRequest(
    [property: JsonPropertyName("dayOfWeek")] DayOfWeek DayOfWeek,
    [property: JsonPropertyName("startTime")] string StartTime,
    [property: JsonPropertyName("endTime")] string EndTime
);
