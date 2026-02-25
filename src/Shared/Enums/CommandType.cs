namespace POSMonitor.Shared.Enums;

public enum CommandType
{
    Unknown = 0,
    RestartService = 1,
    RestartProcess = 2,
    LaunchProcess = 3,
    UpdateConfiguration = 4,
    Custom = 5
}
