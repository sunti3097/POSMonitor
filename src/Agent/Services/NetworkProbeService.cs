using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using POSMonitor.Agent.Options;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Agent.Services;

public class NetworkProbeService
{
    private readonly MonitoringTargetsOptions.NetworkOptions _options;
    private readonly ILogger<NetworkProbeService> _logger;

    public NetworkProbeService(IOptions<MonitoringTargetsOptions> monitoringOptions, ILogger<NetworkProbeService> logger)
    {
        _options = monitoringOptions.Value.Network;
        _logger = logger;
    }

    public async Task<(NetworkStatus status, string ipAddress)> ProbeAsync(CancellationToken cancellationToken)
    {
        var localIp = GetLocalIpAddress();

        foreach (var host in _options.PingHosts)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, _options.TimeoutMilliseconds);
                if (reply.Status == IPStatus.Success)
                {
                    return (NetworkStatus.Connected, localIp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ping to {Host} failed", host);
            }
        }

        return (NetworkStatus.Disconnected, localIp);
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                .ToString() ?? "0.0.0.0";
        }
        catch
        {
            return "0.0.0.0";
        }
    }
}
