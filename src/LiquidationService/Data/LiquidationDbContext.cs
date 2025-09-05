using LiquidationService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data;

/// <summary>
/// Database context for the Liquidation Service
/// </summary>
public class LiquidationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the LiquidationDbContext
    /// </summary>
    /// <param name="options">Database context options</param>
    public LiquidationDbContext(DbContextOptions<LiquidationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Liquidation requests
    /// </summary>
    public DbSet<LiquidationRequest> LiquidationRequests { get; set; }

    /// <summary>
    /// Liquidity providers
    /// </summary>
    public DbSet<LiquidityProvider> LiquidityProviders { get; set; }

    /// <summary>
    /// Compliance checks
    /// </summary>
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; }

    /// <summary>
    /// Liquidation transactions
    /// </summary>
    public DbSet<LiquidationTransaction> LiquidationTransactions { get; set; }

    /// <summary>
    /// Liquidation records (new)
    /// </summary>
    public DbSet<LiquidationRecord> LiquidationRecords { get; set; }

    /// <summary>
    /// Market price snapshots
    /// </summary>
    public DbSet<MarketPriceSnapshot> MarketPriceSnapshots { get; set; }

    /// <summary>
    /// Asset eligibility rules
    /// </summary>
    public DbSet<AssetEligibility> AssetEligibilities { get; set; }

    /// <summary>
    /// Configure entity relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        ConfigureRelationships(modelBuilder);
    }

    /// <summary>
    /// Configure entity relationships
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // LiquidationRequest -> LiquidityProvider (optional)
        modelBuilder.Entity<LiquidationRequest>()
            .HasOne(lr => lr.LiquidityProvider)
            .WithMany()
            .HasForeignKey(lr => lr.LiquidityProviderId)
            .OnDelete(DeleteBehavior.SetNull);

        // LiquidationRequest -> ComplianceChecks (one-to-many)
        modelBuilder.Entity<LiquidationRequest>()
            .HasMany(lr => lr.ComplianceChecks)
            .WithOne(cc => cc.LiquidationRequest)
            .HasForeignKey(cc => cc.LiquidationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // LiquidationRequest -> LiquidationTransactions (one-to-many)
        modelBuilder.Entity<LiquidationRequest>()
            .HasMany(lr => lr.LiquidationTransactions)
            .WithOne(lt => lt.LiquidationRequest)
            .HasForeignKey(lt => lt.LiquidationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // LiquidationRequest -> MarketPriceSnapshots (one-to-many)
        modelBuilder.Entity<LiquidationRequest>()
            .HasMany(lr => lr.MarketPriceSnapshots)
            .WithOne(mps => mps.LiquidationRequest)
            .HasForeignKey(mps => mps.LiquidationRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        // LiquidityProvider -> LiquidationTransactions (one-to-many)
        modelBuilder.Entity<LiquidityProvider>()
            .HasMany<LiquidationTransaction>()
            .WithOne(lt => lt.LiquidityProvider)
            .HasForeignKey(lt => lt.LiquidityProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    /// <returns>Number of affected rows</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected rows</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update timestamps for entities that implement ITimestampEntity
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<ITimestampEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // Prevent modification of CreatedAt
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
