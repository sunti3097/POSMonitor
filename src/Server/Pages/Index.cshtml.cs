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
    public List<CompanyGroup> CompanyGroups { get; private set; } = new();
    public DateTimeOffset GeneratedAt { get; private set; } = DateTimeOffset.UtcNow;

    public int TotalDevices => Devices.Count;
    public int OnlineDevices => Devices.Count(d => d.Status == DeviceStatus.Online);
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
            .OrderBy(d => d.CompanyCode)
            .ThenBy(d => d.StoreCode)
            .ThenBy(d => d.DeviceId)
            .ToListAsync(cancellationToken);

        Devices = entities.Select(d => d.ToSummaryDto()).ToList();

        CompanyGroups = Devices
            .GroupBy(d => string.IsNullOrWhiteSpace(d.CompanyCode) ? "ไม่ระบุบริษัท" : d.CompanyCode)
            .OrderBy(g => g.Key)
            .Select(cg => new CompanyGroup
            {
                CompanyCode = cg.Key,
                Stores = cg
                    .GroupBy(d => string.IsNullOrWhiteSpace(d.StoreCode) ? "ไม่ระบุสาขา" : d.StoreCode)
                    .OrderBy(sg => sg.Key)
                    .Select(sg => new StoreGroup
                    {
                        StoreCode = sg.Key,
                        GroupName = sg.FirstOrDefault(d => !string.IsNullOrEmpty(d.GroupName))?.GroupName ?? sg.Key,
                        Devices = sg.ToList()
                    }).ToList()
            }).ToList();
    }

    public class CompanyGroup
    {
        public string CompanyCode { get; set; } = "";
        public List<StoreGroup> Stores { get; set; } = new();
        public int TotalDevices => Stores.Sum(s => s.Devices.Count);
        public int OnlineCount => Stores.Sum(s => s.OnlineCount);
        public int OfflineCount => Stores.Sum(s => s.OfflineCount);
    }

    public class StoreGroup
    {
        public string StoreCode { get; set; } = "";
        public string GroupName { get; set; } = "";
        public List<DeviceSummaryDto> Devices { get; set; } = new();
        public int OnlineCount => Devices.Count(d => d.Status == DeviceStatus.Online);
        public int OfflineCount => Devices.Count(d => d.Status == DeviceStatus.Offline);
    }
}
