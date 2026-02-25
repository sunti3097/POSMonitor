using System.Text.Json.Serialization;

namespace POSMonitor.Shared.Contracts.Requests;

public record AssignDevicesRequest(
    [property: JsonPropertyName("deviceIds")] IReadOnlyCollection<string> DeviceIds
);
