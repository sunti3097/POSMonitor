using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using POSMonitor.Server.Options;

namespace POSMonitor.Server.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NotificationOptions _options;

    public NotificationService(
        ILogger<NotificationService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<NotificationOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task NotifyOfflineAsync(string deviceId, string hostname, DateTimeOffset lastSeen, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Device {DeviceId} ({Hostname}) offline since {LastSeen}", deviceId, hostname, lastSeen);

        if (_options.Email.Enabled)
        {
            // TODO: plug into real SMTP service
            _logger.LogInformation("[Email] Notify {Recipient}: Device {DeviceId} offline", _options.Email.To, deviceId);
        }

        if (_options.Teams.Enabled && !string.IsNullOrWhiteSpace(_options.Teams.WebhookUrl))
        {
            try
            {
                var payload = new
                {
                    title = "POS Device Offline",
                    text = $"Device {deviceId} ({hostname}) offline since {lastSeen:O}"
                };

                var client = _httpClientFactory.CreateClient();
                await client.PostAsJsonAsync(_options.Teams.WebhookUrl, payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Teams notification for {DeviceId}", deviceId);
            }
        }
    }
}
