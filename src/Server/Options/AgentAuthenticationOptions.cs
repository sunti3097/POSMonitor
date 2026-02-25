namespace POSMonitor.Server.Options;

public class AgentAuthenticationOptions
{
    public const string SectionName = "AgentAuthentication";

    public string HeaderName { get; set; } = "X-Agent-Key";
    public string ApiKey { get; set; } = string.Empty;
}
