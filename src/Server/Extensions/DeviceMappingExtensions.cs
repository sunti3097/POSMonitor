using POSMonitor.Server.Data.Entities;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Enums;
using POSMonitor.Shared.Models;
using System.Text.Json;

namespace POSMonitor.Server.Extensions;

public static class DeviceMappingExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static DeviceSummaryDto ToSummaryDto(this Device device)
    {
        var primaryGroup = device.GroupAssignments
            .Select(ga => ga.DeviceGroup)
            .FirstOrDefault(g => g != null);

        return new DeviceSummaryDto(
            device.Id,
            device.DeviceId,
            device.Hostname,
            device.IpAddress,
            device.Status,
            device.NetworkStatus,
            device.LastHeartbeatAt,
            primaryGroup?.Id,
            primaryGroup?.Name,
            device.CompanyCode,
            device.StoreCode);
    }

    public static DeviceDetailDto ToDetailDto(this Device device)
    {
        return new DeviceDetailDto(
            device.Id,
            device.DeviceId,
            device.Hostname,
            device.IpAddress,
            device.Status,
            device.NetworkStatus,
            device.LastHeartbeatAt,
            device.DeserializeHardware(),
            device.DeserializeServices(),
            device.DeserializeProcesses(),
            device.GroupAssignments
                .Select(ga => ga.DeviceGroup)
                .Where(g => g != null)
                .Cast<DeviceGroup>()
                .Select(g => g.ToDto())
                .ToList(),
            device.CompanyCode,
            device.StoreCode);
    }

    public static HardwareSnapshotDto? DeserializeHardware(this Device device)
        => string.IsNullOrWhiteSpace(device.LastHardwareSnapshotJson)
            ? null
            : JsonSerializer.Deserialize<HardwareSnapshotDto>(device.LastHardwareSnapshotJson, SerializerOptions);

    public static IReadOnlyCollection<ServiceStatusDto> DeserializeServices(this Device device)
        => string.IsNullOrWhiteSpace(device.LastServicesJson)
            ? Array.Empty<ServiceStatusDto>()
            : JsonSerializer.Deserialize<IReadOnlyCollection<ServiceStatusDto>>(device.LastServicesJson, SerializerOptions)
              ?? Array.Empty<ServiceStatusDto>();

    public static IReadOnlyCollection<TrackedProcessDto> DeserializeProcesses(this Device device)
        => string.IsNullOrWhiteSpace(device.LastProcessesJson)
            ? Array.Empty<TrackedProcessDto>()
            : JsonSerializer.Deserialize<IReadOnlyCollection<TrackedProcessDto>>(device.LastProcessesJson, SerializerOptions)
              ?? Array.Empty<TrackedProcessDto>();

    public static string SerializePayload<T>(T value) => JsonSerializer.Serialize(value, SerializerOptions);

    public static CommandDto ToDto(this Command command) => new(
        command.Id,
        command.DeviceId,
        command.CommandType,
        command.Status,
        command.PayloadJson,
        command.ResultJson,
        command.CreatedBy,
        command.CreatedAt,
        command.UpdatedAt,
        command.ExecutedAt);

    public static DeviceGroupDto ToDto(this DeviceGroup group) => new(
        group.Id,
        group.Name,
        group.Description,
        group.NotificationWindows
            .Select(w => new NotificationWindowDto(w.DayOfWeek, w.StartTime.ToString(), w.EndTime.ToString()))
            .ToList());
}
