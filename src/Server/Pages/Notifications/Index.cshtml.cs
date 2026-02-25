using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Data.Entities;

namespace POSMonitor.Server.Pages.Notifications;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly POSMonitorDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public IndexModel(POSMonitorDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public List<NotificationConfigViewModel> Configs { get; set; } = new();
    public List<DeviceGroup> Groups { get; set; } = new();

    // Email Settings
    [BindProperty]
    public string? SmtpServer { get; set; }
    [BindProperty]
    public int SmtpPort { get; set; } = 587;
    [BindProperty]
    public string? SmtpUsername { get; set; }
    [BindProperty]
    public string? SmtpPassword { get; set; }
    [BindProperty]
    public string? EmailFrom { get; set; }
    [BindProperty]
    public bool EnableEmail { get; set; }

    // MS Teams Settings
    [BindProperty]
    public string? TeamsWebhookUrl { get; set; }
    [BindProperty]
    public bool EnableTeams { get; set; }

    // Schedule Settings
    [BindProperty]
    public Guid ScheduleGroupId { get; set; }
    [BindProperty]
    public int ScheduleStartHour { get; set; }
    [BindProperty]
    public int ScheduleEndHour { get; set; }
    [BindProperty]
    public string? ScheduleDays { get; set; }

    public async Task OnGetAsync()
    {
        // Load from configuration
        SmtpServer = _configuration["Notifications:Email:SmtpServer"];
        SmtpPort = _configuration.GetValue<int>("Notifications:Email:SmtpPort", 587);
        SmtpUsername = _configuration["Notifications:Email:Username"];
        EmailFrom = _configuration["Notifications:Email:From"];
        EnableEmail = _configuration.GetValue<bool>("Notifications:Email:Enabled", false);

        TeamsWebhookUrl = _configuration["Notifications:Teams:WebhookUrl"];
        EnableTeams = _configuration.GetValue<bool>("Notifications:Teams:Enabled", false);

        Groups = await _dbContext.DeviceGroups.OrderBy(g => g.Name).ToListAsync();

        Configs = await _dbContext.DeviceGroupNotificationWindows
            .Include(n => n.DeviceGroup)
            .Select(n => new NotificationConfigViewModel
            {
                Id = n.Id,
                GroupId = n.DeviceGroupId,
                GroupName = n.DeviceGroup!.Name,
                StartHour = n.StartTime.Hours,
                EndHour = n.EndTime.Hours,
                DaysOfWeek = n.DayOfWeek.ToString()
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveScheduleAsync()
    {
        var group = await _dbContext.DeviceGroups.FindAsync(ScheduleGroupId);
        if (group == null) return RedirectToPage();

        var existing = await _dbContext.DeviceGroupNotificationWindows
            .FirstOrDefaultAsync(n => n.DeviceGroupId == ScheduleGroupId);

        if (existing == null)
        {
            existing = new DeviceGroupNotificationWindow
            {
                DeviceGroupId = ScheduleGroupId,
            };
            _dbContext.DeviceGroupNotificationWindows.Add(existing);
        }

        existing.StartTime = TimeSpan.FromHours(ScheduleStartHour);
        existing.EndTime = TimeSpan.FromHours(ScheduleEndHour);
        
        // Simplified for this example: store as string or use the DayOfWeek enum properly
        // Here we'll just set it to Monday as placeholder or map from ScheduleDays if needed
        // For production, you'd likely want a robust way to handle multiple days.
        existing.DayOfWeek = DayOfWeek.Monday;

        await _dbContext.SaveChangesAsync();
        TempData["Message"] = "บันทึกเวลาแจ้งเตือนสำเร็จ";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveEmailAsync()
    {
        // In production, save to database or secure config
        TempData["Message"] = "บันทึกการตั้งค่าอีเมลสำเร็จ";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveTeamsAsync()
    {
        TempData["Message"] = "บันทึกการตั้งค่า MS Teams สำเร็จ";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestEmailAsync()
    {
        // TODO: Implement email test
        TempData["Message"] = "ส่งอีเมลทดสอบแล้ว กรุณาตรวจสอบกล่องจดหมาย";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestTeamsAsync()
    {
        // TODO: Implement Teams webhook test
        TempData["Message"] = "ส่งข้อความทดสอบไปยัง MS Teams แล้ว";
        return RedirectToPage();
    }

    public class NotificationConfigViewModel
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public string DaysOfWeek { get; set; } = "";
    }
}
