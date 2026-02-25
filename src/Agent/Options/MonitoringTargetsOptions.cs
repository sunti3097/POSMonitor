namespace POSMonitor.Agent.Options;

public class MonitoringTargetsOptions
{
    public const string SectionName = "MonitoringTargets";

    public NetworkOptions Network { get; set; } = new();
    public List<ServiceTargetOptions> Services { get; set; } = new();
    public List<ProcessTargetOptions> Processes { get; set; } = new();

    public class NetworkOptions
    {
        public List<string> PingHosts { get; set; } = new();
        public int TimeoutMilliseconds { get; set; } = 1500;
    }

    public List<string> PingTargets { get; set; } = new();

    public class ServiceTargetOptions
    {
        public string ServiceName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool AutoRestart { get; set; } = true;
    }

    public class ProcessTargetOptions
    {
        public string ProcessName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string ExecutablePath { get; set; } = string.Empty;
        public bool AutoRestart { get; set; } = true;
    }
}
