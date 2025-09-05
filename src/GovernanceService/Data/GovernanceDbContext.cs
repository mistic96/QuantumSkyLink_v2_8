using GovernanceService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GovernanceService.Data;

public class GovernanceDbContext : DbContext
{
    public GovernanceDbContext(DbContextOptions<GovernanceDbContext> options) : base(options)
    {
    }

    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<GovernanceRule> GovernanceRules { get; set; }
    public DbSet<VotingDelegation> VotingDelegations { get; set; }
    public DbSet<ProposalExecution> ProposalExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Minimal fluent API usage - only for complex scenarios that annotations cannot handle
        
        // Composite unique constraint for one vote per user per proposal
        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.ProposalId, v.VoterId })
            .IsUnique()
            .HasDatabaseName("IX_Votes_ProposalId_VoterId_Unique");

        // Composite unique constraint for delegation (one delegation per delegator-delegate-type combination)
        modelBuilder.Entity<VotingDelegation>()
            .HasIndex(vd => new { vd.DelegatorId, vd.DelegateId, vd.SpecificType })
            .IsUnique()
            .HasDatabaseName("IX_VotingDelegations_Delegator_Delegate_Type_Unique");

        // Ensure one active governance rule per proposal type
        modelBuilder.Entity<GovernanceRule>()
            .HasIndex(gr => gr.ApplicableType)
            .IsUnique()
            .HasFilter("\"IsActive\" = true")
            .HasDatabaseName("IX_GovernanceRules_ApplicableType_Active_Unique");

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITimestampEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (ITimestampEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}

public interface ITimestampEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
