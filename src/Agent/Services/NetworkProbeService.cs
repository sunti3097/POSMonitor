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

    public async Task<(NetworkStatus status, string ipAddress, string macAddress)> ProbeAsync(CancellationToken cancellationToken)
    {
        var localIp = GetLocalIpAddress();
        var macAddress = GetMacAddress();

        foreach (var host in _options.PingHosts)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, _options.TimeoutMilliseconds);
                if (reply.Status == IPStatus.Success)
                {
                    return (NetworkStatus.Connected, localIp, macAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ping to {Host} failed", host);
            }
        }

        return (NetworkStatus.Disconnected, localIp, macAddress);
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

    private static string GetMacAddress()
    {
        try
        {
            var macAddr = (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            if (string.IsNullOrEmpty(macAddr)) return "-";
            
            // Format MAC address with hyphens (e.g., 00-11-22-33-44-55)
            return string.Join("-", Enumerable.Range(0, macAddr.Length / 2).Select(i => macAddr.Substring(i * 2, 2)));
        }
        catch
        {
            return "-";
        }
    }
}
