using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using POSMonitor.Agent.Options;
using POSMonitor.Agent.Security;
using POSMonitor.Shared.Contracts.Requests;
using System.Text.Json;

namespace POSMonitor.Agent.Repositories;

public class HeartbeatQueueRepository
{
    private readonly string _connectionString;
    private readonly ILogger<HeartbeatQueueRepository> _logger;

    private const string TableSql = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HeartbeatQueue' AND xtype='U')
BEGIN
    CREATE TABLE HeartbeatQueue (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Payload NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );
END";

    public HeartbeatQueueRepository(IOptions<AgentOptions> options, ILogger<HeartbeatQueueRepository> logger)
    {
        _connectionString = BuildConnectionString(options.Value);
        _logger = logger;
        Initialize().GetAwaiter().GetResult();
    }

    private static string BuildConnectionString(AgentOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SqlPasswordEncrypted))
        {
            var builder = new SqlConnectionStringBuilder(options.SqlExpressConnectionString);
            builder.Password = SecretProtector.Decrypt(options.SqlPasswordEncrypted);
            builder.Encrypt = false;
            return builder.ToString();
        }

        var defaultBuilder = new SqlConnectionStringBuilder(options.SqlExpressConnectionString)
        {
            Encrypt = false
        };

        return defaultBuilder.ToString();
    }

    private async Task Initialize()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(TableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task EnqueueAsync(HeartbeatReport report, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(report);
        var command = new SqlCommand("INSERT INTO HeartbeatQueue (Id, Payload) VALUES (@Id, @Payload)", connection);
        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@Payload", payload);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<(Guid id, HeartbeatReport report)>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var pending = new List<(Guid, HeartbeatReport)>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new SqlCommand("SELECT TOP 50 Id, Payload FROM HeartbeatQueue ORDER BY CreatedAt", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetGuid(0);
            var payload = reader.GetString(1);
            var report = JsonSerializer.Deserialize<HeartbeatReport>(payload);
            if (report != null)
            {
                pending.Add((id, report));
            }
        }

        return pending;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new SqlCommand("DELETE FROM HeartbeatQueue WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
