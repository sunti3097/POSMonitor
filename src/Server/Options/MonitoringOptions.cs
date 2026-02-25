namespace POSMonitor.Server.Options;

public class MonitoringOptions
{
    public const string SectionName = "Monitoring";

    public int OfflineThresholdMinutes { get; set; } = 15;
    public int HeartbeatRetentionDays { get; set; } = 14;
    public int PollingIntervalSeconds { get; set; } = 30;
}
