using Microsoft.Extensions.Options;
using POSMonitor.Agent.Clients;
using POSMonitor.Agent.Options;
using POSMonitor.Agent.Repositories;
using POSMonitor.Agent.Services;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Enums;
using POSMonitor.Shared.Models;

namespace POSMonitor.Agent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AgentOptions _options;
    private readonly AgentApiClient _apiClient;
    private readonly HardwareMetricsProvider _hardwareMetricsProvider;
    private readonly ServiceStatusCollector _serviceStatusCollector;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly NetworkProbeService _networkProbeService;
    private readonly HeartbeatQueueRepository _queueRepository;
    private readonly CommandExecutor _commandExecutor;

    public Worker(
        ILogger<Worker> logger,
        IOptions<AgentOptions> options,
        AgentApiClient apiClient,
        HardwareMetricsProvider hardwareMetricsProvider,
        ServiceStatusCollector serviceStatusCollector,
        ProcessMonitorService processMonitorService,
        NetworkProbeService networkProbeService,
        HeartbeatQueueRepository queueRepository,
        CommandExecutor commandExecutor)
    {
        _logger = logger;
        _options = options.Value;
        _apiClient = apiClient;
        _hardwareMetricsProvider = hardwareMetricsProvider;
        _serviceStatusCollector = serviceStatusCollector;
        _processMonitorService = processMonitorService;
        _networkProbeService = networkProbeService;
        _queueRepository = queueRepository;
        _commandExecutor = commandExecutor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent worker started for {Device}", _options.DeviceId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var report = await BuildReportAsync(stoppingToken);
                await _queueRepository.EnqueueAsync(report, stoppingToken);

                await DrainQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process heartbeat cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.HeartbeatIntervalSeconds), stoppingToken);
        }
    }

    private async Task DrainQueueAsync(CancellationToken cancellationToken)
    {
        var pending = await _queueRepository.GetPendingAsync(cancellationToken);
        foreach (var (id, report) in pending)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var response = await _apiClient.SendHeartbeatAsync(report, cancellationToken);
                if (response == null)
                {
                    continue;
                }

                await _queueRepository.DeleteAsync(id, cancellationToken);
                await HandleCommandsAsync(response.PendingCommands, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send queued heartbeat");
                break; // stop draining to retry later
            }
        }
    }

    private async Task HandleCommandsAsync(IReadOnlyCollection<PendingCommandDto> commands, CancellationToken cancellationToken)
    {
        foreach (var command in commands)
        {
            var result = await _commandExecutor.ExecuteAsync(command, cancellationToken);
            await _apiClient.SendCommandResultAsync(command.CommandId, result, cancellationToken);
        }
    }

    private async Task<HeartbeatReport> BuildReportAsync(CancellationToken cancellationToken)
    {
        var (networkStatus, ipAddress, macAddress) = await _networkProbeService.ProbeAsync(cancellationToken);
        var hardware = await _hardwareMetricsProvider.CaptureAsync(cancellationToken);
        var services = await _serviceStatusCollector.CollectAsync(cancellationToken);
        var processes = await _processMonitorService.CollectAsync(cancellationToken);

        var hostname = string.IsNullOrWhiteSpace(_options.HostnameOverride) 
            ? (Environment.MachineName ?? System.Net.Dns.GetHostName() ?? "Unknown-PC") 
            : _options.HostnameOverride;

        return new HeartbeatReport(
            _options.DeviceId,
            hostname,
            ipAddress,
            macAddress,
            networkStatus,
            hardware,
            services,
            processes,
            DateTimeOffset.UtcNow);
    }
}
