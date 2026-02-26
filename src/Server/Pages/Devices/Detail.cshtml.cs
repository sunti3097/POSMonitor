using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data;
using POSMonitor.Server.Extensions;
using POSMonitor.Shared.Contracts.Responses;

namespace POSMonitor.Server.Pages.Devices;

public class DetailModel : PageModel
{
    private readonly POSMonitorDbContext _dbContext;

    public DetailModel(POSMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DeviceDetailDto Device { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var device = await _dbContext.Devices
            .AsNoTracking()
            .Include(d => d.GroupAssignments)
                .ThenInclude(ga => ga.DeviceGroup)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
        {
            return NotFound();
        }

        Device = device.ToDetailDto();
        return Page();
    }
}
