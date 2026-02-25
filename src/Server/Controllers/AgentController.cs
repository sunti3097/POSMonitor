using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;
using POSMonitor.Server.Extensions;
using POSMonitor.Server.Filters;
using POSMonitor.Server.Options;
using POSMonitor.Server.Utilities;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Enums;
using POSMonitor.Shared.Models;
using System.Text.Json;

namespace POSMonitor.Server.Controllers;

[ApiController]
[Route("api/agent")]
[ServiceFilter(typeof(AgentApiKeyFilter))]
public class AgentController : ControllerBase
{
    private readonly POSMonitorDbContext _dbContext;
    private readonly MonitoringOptions _monitoringOptions;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public AgentController(POSMonitorDbContext dbContext, IOptions<MonitoringOptions> monitoringOptions)
    {
        _dbContext = dbContext;
        _monitoringOptions = monitoringOptions.Value;
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<HeartbeatAcknowledgeDto>> PostHeartbeatAsync([FromBody] HeartbeatReport report, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .Include(d => d.GroupAssignments)
                .ThenInclude(ga => ga.DeviceGroup)
                    .ThenInclude(g => g.NotificationWindows)
            .FirstOrDefaultAsync(d => d.DeviceId == report.DeviceId, cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                DeviceId = report.DeviceId,
                Hostname = report.Hostname,
                IpAddress = report.IpAddress
            };

            await _dbContext.Devices.AddAsync(device, cancellationToken);
        }

        UpdateDeviceFromHeartbeat(device, report);

        var heartbeat = new Heartbeat
        {
            Device = device,
            ReportedAt = report.ReportedAt,
            NetworkStatus = report.NetworkStatus,
            HardwareSnapshotJson = JsonSerializer.Serialize(report.Hardware, _serializerOptions),
            ServicesJson = JsonSerializer.Serialize(report.Services, _serializerOptions),
            ProcessesJson = JsonSerializer.Serialize(report.TrackedProcesses, _serializerOptions)
        };

        await _dbContext.Heartbeats.AddAsync(heartbeat, cancellationToken);

        var pendingCommands = await _dbContext.Commands
            .Where(c => c.DeviceId == device.Id && c.Status == CommandStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var command in pendingCommands)
        {
            command.Status = CommandStatus.Acknowledged;
            command.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new HeartbeatAcknowledgeDto(
            true,
            pendingCommands.Select(c => new PendingCommandDto(c.Id, c.CommandType, c.PayloadJson, c.CreatedAt)).ToList()));
    }

    [HttpPost("commands/{commandId:guid}/result")]
    public async Task<IActionResult> PostCommandResultAsync(Guid commandId, [FromBody] CommandResultRequest request, CancellationToken cancellationToken)
    {
        var command = await _dbContext.Commands.FirstOrDefaultAsync(c => c.Id == commandId, cancellationToken);
        if (command is null)
        {
            return NotFound();
        }

        command.Status = request.Status;
        command.ExecutedAt = request.CompletedAt;
        command.UpdatedAt = DateTimeOffset.UtcNow;
        command.ResultJson = JsonSerializer.Serialize(new { request.Succeeded, request.Output }, _serializerOptions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private void UpdateDeviceFromHeartbeat(Device device, HeartbeatReport report)
    {
        device.Hostname = report.Hostname;
        device.IpAddress = report.IpAddress;
        device.NetworkStatus = report.NetworkStatus;
        device.Status = DeviceStatus.Online;
        device.LastHeartbeatAt = report.ReportedAt;
        device.LastHardwareSnapshotJson = JsonSerializer.Serialize(report.Hardware, _serializerOptions);
        device.LastServicesJson = JsonSerializer.Serialize(report.Services, _serializerOptions);
        device.LastProcessesJson = JsonSerializer.Serialize(report.TrackedProcesses, _serializerOptions);
        device.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
