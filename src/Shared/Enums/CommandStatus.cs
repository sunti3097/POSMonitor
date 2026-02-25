namespace POSMonitor.Shared.Enums;

public enum CommandStatus
{
    Pending = 0,
    Acknowledged = 1,
    Executing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}
