using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using POSMonitor.Agent.Options;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Contracts.Responses;

namespace POSMonitor.Agent.Clients;

public class AgentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentApiClient> _logger;

    public AgentApiClient(HttpClient httpClient, IOptions<AgentOptions> options, ILogger<AgentApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (Uri.TryCreate(_options.ApiBaseUrl, UriKind.Absolute, out var baseUri))
        {
            _httpClient.BaseAddress = baseUri;
        }
    }

    public async Task<HeartbeatAcknowledgeDto?> SendHeartbeatAsync(HeartbeatReport report, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/agent/heartbeat")
        {
            Content = JsonContent.Create(report)
        };

        request.Headers.Add("X-Agent-Key", _options.ApiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Heartbeat rejected with status {StatusCode}", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<HeartbeatAcknowledgeDto>(cancellationToken: cancellationToken);
    }

    public async Task SendCommandResultAsync(Guid commandId, CommandResultRequest result, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/agent/commands/{commandId}/result")
        {
            Content = JsonContent.Create(result)
        };

        request.Headers.Add("X-Agent-Key", _options.ApiKey);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
