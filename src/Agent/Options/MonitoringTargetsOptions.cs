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

    public class ServiceTargetOptions
    {
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool AutoRestart { get; set; } = true;
    }

    public class ProcessTargetOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool AutoRestart { get; set; } = true;
    }
}
