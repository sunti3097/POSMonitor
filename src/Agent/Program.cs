using POSMonitor.Agent;
using POSMonitor.Agent.Clients;
using POSMonitor.Agent.Options;
using POSMonitor.Agent.Repositories;
using POSMonitor.Agent.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<AgentOptions>(context.Configuration.GetSection(AgentOptions.SectionName));
        services.Configure<MonitoringTargetsOptions>(context.Configuration.GetSection(MonitoringTargetsOptions.SectionName));

        services.AddHttpClient<AgentApiClient>();

        services.AddSingleton<HardwareMetricsProvider>();
        services.AddSingleton<ServiceStatusCollector>();
        services.AddSingleton<ProcessMonitorService>();
        services.AddSingleton<NetworkProbeService>();
        services.AddSingleton<HeartbeatQueueRepository>();
        services.AddSingleton<CommandExecutor>();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
