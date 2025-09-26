using Microsoft.EntityFrameworkCore;
using MonitorEntity = IsItLive.Api.Models.Monitor;

namespace IsItLive.Api.Models;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<MonitorEntity> Monitors => Set<MonitorEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MonitorEntity>(e =>
        {
            e.ToTable("monitors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Url).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.Property(x => x.LastCheckedUtc);
            e.HasIndex(x => x.Status);
        });
    }
}