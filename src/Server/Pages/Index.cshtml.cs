using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Extensions;
using POSMonitor.Shared.Contracts.Responses;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Pages;

public class IndexModel : PageModel
{
    private readonly POSMonitorDbContext _dbContext;

    public IReadOnlyList<DeviceSummaryDto> Devices { get; private set; } = Array.Empty<DeviceSummaryDto>();
    public IReadOnlyList<GroupSummary> GroupBreakdown { get; private set; } = Array.Empty<GroupSummary>();
    public DateTimeOffset GeneratedAt { get; private set; } = DateTimeOffset.UtcNow;

    public int TotalDevices => Devices.Count;
    public int OfflineDevices => Devices.Count(d => d.Status == DeviceStatus.Offline);
    public int WarningDevices => Devices.Count(d => d.Status is DeviceStatus.Degraded or DeviceStatus.Unknown);

    public IndexModel(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        GeneratedAt = DateTimeOffset.UtcNow;

        var entities = await _dbContext.Devices
            .AsNoTracking()
            .Include(d => d.GroupAssignments)
                .ThenInclude(ga => ga.DeviceGroup)
            .OrderBy(d => d.Hostname)
            .ToListAsync(cancellationToken);

        Devices = entities
            .Select(d => d.ToSummaryDto())
            .ToList();

        GroupBreakdown = Devices
            .GroupBy(d => string.IsNullOrWhiteSpace(d.GroupName) ? "ไม่มีกลุ่ม" : d.GroupName)
            .Select(g => new GroupSummary(
                g.Key ?? "ไม่มีกลุ่ม",
                g.Count(),
                g.Count(x => x.Status == DeviceStatus.Offline)))
            .OrderByDescending(g => g.Total)
            .ToList();
    }

    public record GroupSummary(string Name, int Total, int Offline);
}
