using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;
using POSMonitor.Server.Extensions;
using POSMonitor.Shared.Contracts.Requests;
using POSMonitor.Shared.Contracts.Responses;

namespace POSMonitor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceGroupsController : ControllerBase
{
    private readonly POSMonitorDbContext _dbContext;
    private static readonly string[] TimeFormats = ["hh\\:mm", "h\\:mm"];

    public DeviceGroupsController(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceGroupDto>>> GetGroupsAsync(CancellationToken cancellationToken)
    {
        var groups = await _dbContext.DeviceGroups
            .AsNoTracking()
            .Include(g => g.NotificationWindows)
            .Include(g => g.DeviceAssignments)
            .ThenInclude(da => da.Device)
            .ToListAsync(cancellationToken);

        return Ok(groups.Select(g => g.ToDto()));
    }

    [HttpGet("{groupId:guid}")]
    public async Task<ActionResult<DeviceGroupDto>> GetGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await _dbContext.DeviceGroups
            .AsNoTracking()
            .Include(g => g.NotificationWindows)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null)
        {
            return NotFound();
        }

        return Ok(group.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<DeviceGroupDto>> CreateGroupAsync([FromBody] UpsertDeviceGroupRequest request, CancellationToken cancellationToken)
    {
        var group = new DeviceGroup
        {
            Name = request.Name,
            Description = request.Description,
            NotificationWindows = request.NotificationWindows
                .Select(CreateWindowEntity)
                .ToList()
        };

        _dbContext.DeviceGroups.Add(group);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetGroupAsync), new { groupId = group.Id }, group.ToDto());
    }

    [HttpPut("{groupId:guid}")]
    public async Task<ActionResult<DeviceGroupDto>> UpdateGroupAsync(Guid groupId, [FromBody] UpsertDeviceGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await _dbContext.DeviceGroups
            .Include(g => g.NotificationWindows)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null)
        {
            return NotFound();
        }

        group.Name = request.Name;
        group.Description = request.Description;

        _dbContext.DeviceGroupNotificationWindows.RemoveRange(group.NotificationWindows);
        group.NotificationWindows = request.NotificationWindows.Select(CreateWindowEntity).ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(group).Collection(g => g.NotificationWindows).LoadAsync(cancellationToken);
        return Ok(group.ToDto());
    }

    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var group = await _dbContext.DeviceGroups.FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        _dbContext.DeviceGroups.Remove(group);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{groupId:guid}/devices")]
    public async Task<IActionResult> AssignDevicesAsync(Guid groupId, [FromBody] AssignDevicesRequest request, CancellationToken cancellationToken)
    {
        var group = await _dbContext.DeviceGroups
            .Include(g => g.DeviceAssignments)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null)
        {
            return NotFound();
        }

        var deviceMap = await _dbContext.Devices
            .Where(d => request.DeviceIds.Contains(d.DeviceId))
            .ToDictionaryAsync(d => d.DeviceId, d => d, cancellationToken);

        foreach (var deviceId in request.DeviceIds)
        {
            if (!deviceMap.TryGetValue(deviceId, out var device))
            {
                continue;
            }

            if (group.DeviceAssignments.Any(a => a.DeviceId == device.Id))
            {
                continue;
            }

            group.DeviceAssignments.Add(new DeviceGroupAssignment
            {
                DeviceId = device.Id,
                DeviceGroupId = group.Id
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{groupId:guid}/devices/{deviceId:guid}")]
    public async Task<IActionResult> RemoveDeviceAsync(Guid groupId, Guid deviceId, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.DeviceGroupAssignments
            .FirstOrDefaultAsync(a => a.DeviceGroupId == groupId && a.DeviceId == deviceId, cancellationToken);

        if (assignment is null)
        {
            return NotFound();
        }

        _dbContext.DeviceGroupAssignments.Remove(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static DeviceGroupNotificationWindow CreateWindowEntity(NotificationWindowRequest request)
    {
        return new DeviceGroupNotificationWindow
        {
            DayOfWeek = request.DayOfWeek,
            StartTime = ParseTime(request.StartTime),
            EndTime = ParseTime(request.EndTime)
        };
    }

    private static TimeSpan ParseTime(string value)
    {
        if (TimeSpan.TryParseExact(value, TimeFormats, null, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid time format: {value}. Expected HH:mm");
    }
}
