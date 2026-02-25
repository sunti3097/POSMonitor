using System.Text;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Agent.Services;

public class CommandExecutor
{
    private readonly ILogger<CommandExecutor> _logger;

    public CommandExecutor(ILogger<CommandExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<CommandResultRequest> ExecuteAsync(PendingCommandDto command, CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        var succeeded = true;

        try
        {
            switch (command.CommandType)
            {
                case CommandType.RestartService:
                    output.AppendLine("RestartService invoked");
                    break;
                case CommandType.RestartProcess:
                    output.AppendLine("RestartProcess invoked");
                    break;
                case CommandType.LaunchProcess:
                    output.AppendLine("LaunchProcess invoked");
                    break;
                case CommandType.UpdateConfiguration:
                    output.AppendLine("UpdateConfiguration invoked");
                    break;
                default:
                    output.AppendLine($"Command {command.CommandType} not implemented");
                    succeeded = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command {CommandId}", command.CommandId);
            output.AppendLine(ex.Message);
            succeeded = false;
        }

        await Task.CompletedTask;

        return new CommandResultRequest(
            succeeded ? CommandStatus.Succeeded : CommandStatus.Failed,
            succeeded,
            output.ToString(),
            DateTimeOffset.UtcNow);
    }
}
