namespace POSMonitor.Agent.Options;

public class AgentOptions
{
    public const string SectionName = "Agent";

    public string DeviceId { get; set; } = string.Empty;
    public string? HostnameOverride { get; set; }
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int HeartbeatIntervalSeconds { get; set; } = 300;
    public string SqlExpressConnectionString { get; set; } = string.Empty;
    public string? SqlPasswordEncrypted { get; set; }
    public string ServiceName { get; set; } = "POSMonitorAgent";
}
