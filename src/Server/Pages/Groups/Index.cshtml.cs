using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;
using POSMonitor.Shared.Enums;

namespace POSMonitor.Server.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    private readonly POSMonitorDbContext _dbContext;

    public IndexModel(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Groups grouped by Company -> Store
    public List<CompanyViewModel> CompanyGroups { get; set; } = new();
    public List<DeviceViewModel> UnassignedDevices { get; set; } = new();

    // Create group fields
    [BindProperty] public string? NewCompanyCode { get; set; }
    [BindProperty] public string? NewStoreCode { get; set; }
    [BindProperty] public string? NewGroupDescription { get; set; }
    [BindProperty] public List<Guid> SelectedDeviceIds { get; set; } = new();

    // All devices for device picker
    public List<DeviceViewModel> AllDevices { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var groups = await _dbContext.DeviceGroups
            .Include(g => g.DeviceAssignments)
                .ThenInclude(a => a.Device)
            .OrderBy(g => g.CompanyCode)
            .ThenBy(g => g.Name)
            .ToListAsync();

        CompanyGroups = groups
            .GroupBy(g => string.IsNullOrWhiteSpace(g.CompanyCode) ? "ไม่ระบุบริษัท" : g.CompanyCode)
            .OrderBy(g => g.Key)
            .Select(cg => new CompanyViewModel
            {
                CompanyCode = cg.Key,
                Stores = cg.Select(g => new StoreGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    StoreCode = g.Name, // group name = store code
                    CompanyCode = g.CompanyCode,
                    Description = g.Description,
                    Devices = g.DeviceAssignments.Select(a => new DeviceViewModel
                    {
                        Id = a.Device!.Id,
                        DeviceId = a.Device.DeviceId,
                        Hostname = a.Device.Hostname,
                        IpAddress = a.Device.IpAddress,
                        Status = a.Device.Status,
                        StoreCode = a.Device.StoreCode,
                        CompanyCode = a.Device.CompanyCode,
                        LastHeartbeatAt = a.Device.LastHeartbeatAt
                    }).OrderBy(d => d.DeviceId).ToList()
                }).OrderBy(s => s.StoreCode).ToList()
            }).ToList();

        AllDevices = await _dbContext.Devices
            .OrderBy(d => d.CompanyCode)
            .ThenBy(d => d.StoreCode)
            .ThenBy(d => d.DeviceId)
            .Select(d => new DeviceViewModel
            {
                Id = d.Id,
                DeviceId = d.DeviceId,
                Hostname = d.Hostname,
                IpAddress = d.IpAddress,
                Status = d.Status,
                StoreCode = d.StoreCode,
                CompanyCode = d.CompanyCode,
                LastHeartbeatAt = d.LastHeartbeatAt
            })
            .ToListAsync();

        // Devices not assigned to any group
        var assignedIds = groups
            .SelectMany(g => g.DeviceAssignments)
            .Select(a => a.DeviceId)
            .ToHashSet();

        UnassignedDevices = AllDevices.Where(d => !assignedIds.Contains(d.Id)).ToList();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCompanyCode) || string.IsNullOrWhiteSpace(NewStoreCode))
        {
            TempData["Error"] = "กรุณาระบุรหัสบริษัทและรหัสสาขา";
            return RedirectToPage();
        }

        var company = NewCompanyCode.Trim().ToUpper();
        var store = NewStoreCode.Trim();
        var groupName = $"{company}-{store}";

        var exists = await _dbContext.DeviceGroups.AnyAsync(g => g.Name == groupName);
        if (exists)
        {
            TempData["Error"] = $"มีกลุ่มสาขา '{store}' ของบริษัท '{company}' อยู่แล้ว";
            return RedirectToPage();
        }

        var group = new DeviceGroup
        {
            Name = groupName,
            CompanyCode = company,
            Description = NewGroupDescription?.Trim()
        };
        _dbContext.DeviceGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        // Assign selected devices
        if (SelectedDeviceIds.Any())
        {
            var validIds = await _dbContext.Devices
                .Where(d => SelectedDeviceIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            foreach (var deviceId in validIds)
            {
                // Remove from any existing group first
                var existing = await _dbContext.Set<DeviceGroupAssignment>()
                    .Where(a => a.DeviceId == deviceId)
                    .ToListAsync();
                _dbContext.Set<DeviceGroupAssignment>().RemoveRange(existing);

                _dbContext.Set<DeviceGroupAssignment>().Add(new DeviceGroupAssignment
                {
                    DeviceId = deviceId,
                    DeviceGroupId = group.Id
                });

                // Update device's CompanyCode and StoreCode
                var device = await _dbContext.Devices.FindAsync(deviceId);
                if (device != null)
                {
                    device.CompanyCode = company;
                    device.StoreCode = store;
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        TempData["Success"] = $"สร้างสาขา '{store}' บริษัท '{company}' สำเร็จ (เครื่อง {SelectedDeviceIds.Count} เครื่อง)";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var group = await _dbContext.DeviceGroups
            .Include(g => g.DeviceAssignments)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group != null)
        {
            _dbContext.Set<DeviceGroupAssignment>().RemoveRange(group.DeviceAssignments);
            _dbContext.DeviceGroups.Remove(group);
            await _dbContext.SaveChangesAsync();
            TempData["Success"] = $"ลบกลุ่ม '{group.Name}' เรียบร้อย";
        }
        return RedirectToPage();
    }

    public class CompanyViewModel
    {
        public string CompanyCode { get; set; } = "";
        public List<StoreGroupViewModel> Stores { get; set; } = new();
        public int TotalDevices => Stores.Sum(s => s.Devices.Count);
        public int OnlineDevices => Stores.Sum(s => s.Devices.Count(d => d.Status == DeviceStatus.Online));
        public int OfflineDevices => Stores.Sum(s => s.Devices.Count(d => d.Status == DeviceStatus.Offline));
    }

    public class StoreGroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string StoreCode { get; set; } = "";
        public string? CompanyCode { get; set; }
        public string? Description { get; set; }
        public List<DeviceViewModel> Devices { get; set; } = new();
        public int OnlineCount => Devices.Count(d => d.Status == DeviceStatus.Online);
        public int OfflineCount => Devices.Count(d => d.Status == DeviceStatus.Offline);
    }

    public class DeviceViewModel
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; } = "";
        public string Hostname { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public DeviceStatus Status { get; set; }
        public string? StoreCode { get; set; }
        public string? CompanyCode { get; set; }
        public DateTimeOffset? LastHeartbeatAt { get; set; }
    }
}
