using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;
using POSMonitor.Server.Extensions;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly POSMonitorDbContext _dbContext;

    public DevicesController(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceSummaryDto>>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = await _dbContext.Devices
            .AsNoTracking()
            .Include(d => d.GroupAssignments)
                .ThenInclude(ga => ga.DeviceGroup)
                    .ThenInclude(g => g.NotificationWindows)
            .ToListAsync(cancellationToken);
        return Ok(devices.Select(d => d.ToSummaryDto()));
    }

    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<DeviceDetailDto>> GetDeviceAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .AsNoTracking()
            .Include(d => d.GroupAssignments)
                .ThenInclude(ga => ga.DeviceGroup)
                    .ThenInclude(g => g.NotificationWindows)
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        if (device is null)
        {
            return NotFound();
        }

        return Ok(device.ToDetailDto());
    }

    [HttpPost("commands")]
    public async Task<ActionResult<CommandDto>> CreateCommandAsync([FromBody] CreateCommandRequest request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId, cancellationToken);
        if (device is null)
        {
            return NotFound($"Device {request.DeviceId} not found");
        }

        var command = new Command
        {
            DeviceId = device.Id,
            CommandType = request.CommandType,
            PayloadJson = string.IsNullOrWhiteSpace(request.PayloadJson) ? "{}" : request.PayloadJson,
            CreatedBy = request.RequestedBy,
            Status = CommandStatus.Pending
        };

        await _dbContext.Commands.AddAsync(command, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetCommandAsync), new { commandId = command.Id }, command.ToDto());
    }

    [HttpGet("commands/{commandId:guid}")]
    public async Task<ActionResult<CommandDto>> GetCommandAsync(Guid commandId, CancellationToken cancellationToken)
    {
        var command = await _dbContext.Commands.AsNoTracking().FirstOrDefaultAsync(c => c.Id == commandId, cancellationToken);
        if (command is null)
        {
            return NotFound();
        }

        return Ok(command.ToDto());
    }
}
