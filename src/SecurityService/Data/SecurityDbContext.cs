using Microsoft.EntityFrameworkCore;
using SecurityService.Data.Entities;

namespace SecurityService.Data;

public class SecurityDbContext : DbContext
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
    {
    }

    public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
    public DbSet<MultiSignatureRequest> MultiSignatureRequests { get; set; }
    public DbSet<MultiSignatureApproval> MultiSignatureApprovals { get; set; }
    public DbSet<MfaToken> MfaTokens { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minimal fluent API usage - only for relationships that can't be handled by annotations
        modelBuilder.Entity<MultiSignatureApproval>()
            .HasOne(a => a.Request)
            .WithMany(r => r.Approvals)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PostgreSQL-specific features
        modelBuilder.HasDefaultSchema("security");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps for entities that have UpdatedAt property
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is SecurityPolicy policy)
            {
                policy.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MultiSignatureRequest request)
            {
                request.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
