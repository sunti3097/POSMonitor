using Microsoft.AspNetCore.Identity;

namespace POSMonitor.Server.Data.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool MustChangePassword { get; set; } = false;
}
