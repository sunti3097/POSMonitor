using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using POSMonitor.Server.Options;

namespace POSMonitor.Server.Filters;

public class AgentApiKeyFilter : IAsyncActionFilter
{
    private readonly AgentAuthenticationOptions _options;
    private readonly ILogger<AgentApiKeyFilter> _logger;

    public AgentApiKeyFilter(IOptions<AgentAuthenticationOptions> options, ILogger<AgentApiKeyFilter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Agent API key not configured. Rejecting request.");
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            return Task.CompletedTask;
        }

        var request = context.HttpContext.Request;
        if (!request.Headers.TryGetValue(_options.HeaderName, out var providedKey) || providedKey != _options.ApiKey)
        {
            _logger.LogWarning("Unauthorized agent request from {RemoteIp}", context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        return next();
    }
}
