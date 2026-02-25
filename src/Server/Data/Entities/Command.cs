using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Data.Entities;

public class Command
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public CommandType CommandType { get; set; } = CommandType.Unknown;
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    public string PayloadJson { get; set; } = string.Empty;
    public string? ResultJson { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ExecutedAt { get; set; }
}
