using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;

namespace POSMonitor.Server.Pages.Groups;

[Authorize]
public class IndexModel : PageModel
{
    private readonly POSMonitorDbContext _dbContext;

    public IndexModel(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<DeviceGroupViewModel> Groups { get; set; } = new();
    public List<string> Companies { get; set; } = new();

    [BindProperty]
    public string? NewGroupName { get; set; }

    [BindProperty]
    public string? NewGroupDescription { get; set; }

    [BindProperty]
    public string? NewGroupCompanyCode { get; set; }

    public async Task OnGetAsync()
    {
        var groups = await _dbContext.DeviceGroups
            .Include(g => g.DeviceAssignments)
            .ThenInclude(a => a.Device)
            .OrderBy(g => g.Name)
            .ToListAsync();

        Groups = groups.Select(g => new DeviceGroupViewModel
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            CompanyCode = g.CompanyCode,
            DeviceCount = g.DeviceAssignments.Count,
            OnlineCount = g.DeviceAssignments.Count(a => a.Device.Status == POSMonitor.Shared.Enums.DeviceStatus.Online),
            OfflineCount = g.DeviceAssignments.Count(a => a.Device.Status == POSMonitor.Shared.Enums.DeviceStatus.Offline)
        }).ToList();

        Companies = await _dbContext.Devices
            .Where(d => d.CompanyCode != null)
            .Select(d => d.CompanyCode!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewGroupName))
        {
            TempData["Error"] = "กรุณาระบุชื่อกลุ่ม";
            return RedirectToPage();
        }
        var exists = await _dbContext.DeviceGroups.AnyAsync(g => g.Name == NewGroupName.Trim());
        if (exists)
        {
            TempData["Error"] = $"มีกลุ่มชื่อ '{NewGroupName.Trim()}' อยู่แล้ว";
            return RedirectToPage();
        }
        var group = new DeviceGroup
        {
            Name = NewGroupName.Trim(),
            Description = NewGroupDescription?.Trim(),
            CompanyCode = NewGroupCompanyCode?.Trim()
        };
        _dbContext.DeviceGroups.Add(group);
        await _dbContext.SaveChangesAsync();
        TempData["Success"] = $"สร้างกลุ่ม '{group.Name}' สำเร็จ";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var group = await _dbContext.DeviceGroups.FindAsync(id);
        if (group != null)
        {
            _dbContext.DeviceGroups.Remove(group);
            await _dbContext.SaveChangesAsync();
            TempData["Success"] = $"ลบกลุ่ม '{group.Name}' เรียบร้อย";
        }
        return RedirectToPage();
    }

    public class DeviceGroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? CompanyCode { get; set; }
        public int DeviceCount { get; set; }
        public int OnlineCount { get; set; }
        public int OfflineCount { get; set; }
    }
}
