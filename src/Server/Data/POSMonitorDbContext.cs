using Microsoft.EntityFrameworkCore;
using POSMonitor.Server.Data.Entities;

namespace POSMonitor.Server.Data;

public class POSMonitorDbContext : DbContext
{
    public POSMonitorDbContext(DbContextOptions<POSMonitorDbContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Heartbeat> Heartbeats => Set<Heartbeat>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<DeviceGroup> DeviceGroups => Set<DeviceGroup>();
    public DbSet<DeviceGroupNotificationWindow> DeviceGroupNotificationWindows => Set<DeviceGroupNotificationWindow>();
    public DbSet<DeviceGroupAssignment> DeviceGroupAssignments => Set<DeviceGroupAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DeviceId).IsUnique();
            entity.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Hostname).HasMaxLength(200);
            entity.Property(x => x.IpAddress).HasMaxLength(60);
            entity.Property(x => x.LastHardwareSnapshotJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.LastServicesJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.LastProcessesJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Heartbeat>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.DeviceId, x.ReportedAt });
            entity.Property(x => x.HardwareSnapshotJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ServicesJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ProcessesJson).HasColumnType("nvarchar(max)");
            entity.HasOne(x => x.Device)
                .WithMany(d => d.Heartbeats)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Command>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.DeviceId, x.Status });
            entity.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ResultJson).HasColumnType("nvarchar(max)");
            entity.HasOne(x => x.Device)
                .WithMany(d => d.Commands)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceGroup>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<DeviceGroupNotificationWindow>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.DeviceGroup)
                .WithMany(g => g.NotificationWindows)
                .HasForeignKey(x => x.DeviceGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceGroupAssignment>(entity =>
        {
            entity.HasKey(x => new { x.DeviceId, x.DeviceGroupId });
            entity.HasOne(x => x.Device)
                .WithMany(d => d.GroupAssignments)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.DeviceGroup)
                .WithMany(g => g.DeviceAssignments)
                .HasForeignKey(x => x.DeviceGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
