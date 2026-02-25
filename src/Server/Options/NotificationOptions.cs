namespace POSMonitor.Server.Options;

public class NotificationOptions
{
    public const string SectionName = "Notifications";

    public EmailOptions Email { get; set; } = new();
    public TeamsOptions Teams { get; set; } = new();

    public class EmailOptions
    {
        public bool Enabled { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }

    public class TeamsOptions
    {
        public bool Enabled { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
    }
}
