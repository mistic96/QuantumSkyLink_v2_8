using Microsoft.EntityFrameworkCore;
using MarketplaceService.Data.Entities;

namespace MarketplaceService.Data;

/// <summary>
/// Database context for the MarketplaceService
/// </summary>
public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<MarketListing> MarketListings { get; set; }
    public DbSet<MarketplaceOrder> MarketplaceOrders { get; set; }
    public DbSet<EscrowAccount> EscrowAccounts { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<PriceHistory> PriceHistory { get; set; }
    public DbSet<OrderHistory> OrderHistory { get; set; }
    public DbSet<EscrowHistory> EscrowHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureMarketListing(modelBuilder);
        ConfigureMarketplaceOrder(modelBuilder);
        ConfigurePaymentTransaction(modelBuilder);
        ConfigureEscrowAccount(modelBuilder);
        ConfigurePriceHistory(modelBuilder);
        ConfigureOrderHistory(modelBuilder);
        ConfigureEscrowHistory(modelBuilder);

        // Configure timestamp entities
        ConfigureTimestampEntities(modelBuilder);
    }

    private static void ConfigureMarketListing(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MarketListing>();

        // Configure relationships
        entity.HasMany(l => l.Orders)
            .WithOne(o => o.MarketListing)
            .HasForeignKey(o => o.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(l => l.PriceHistory)
            .WithOne(p => p.MarketListing)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure constraints
        entity.HasCheckConstraint("CK_MarketListing_TotalQuantity_Positive", "\"TotalQuantity\" > 0");
        entity.HasCheckConstraint("CK_MarketListing_RemainingQuantity_NonNegative", "\"RemainingQuantity\" >= 0");
        entity.HasCheckConstraint("CK_MarketListing_RemainingQuantity_LessOrEqual_Total", "\"RemainingQuantity\" <= \"TotalQuantity\"");
        entity.HasCheckConstraint("CK_MarketListing_MinimumPurchaseQuantity_Positive", "\"MinimumPurchaseQuantity\" > 0");
        entity.HasCheckConstraint("CK_MarketListing_BasePrice_Positive", "\"BasePrice\" IS NULL OR \"BasePrice\" > 0");
        entity.HasCheckConstraint("CK_MarketListing_CommissionPercentage_Range", "\"CommissionPercentage\" >= 0 AND \"CommissionPercentage\" <= 1");

        // Configure asset type constraints
        entity.HasCheckConstraint("CK_MarketListing_PlatformToken_TokenId", 
            "(\"AssetType\" = 1 AND \"TokenId\" IS NOT NULL) OR (\"AssetType\" != 1 AND \"AssetSymbol\" IS NOT NULL)");
    }

    private static void ConfigureMarketplaceOrder(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MarketplaceOrder>();

        // Configure relationships
        entity.HasOne(o => o.MarketListing)
            .WithMany(l => l.Orders)
            .HasForeignKey(o => o.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(o => o.EscrowAccount)
            .WithOne(e => e.MarketplaceOrder)
            .HasForeignKey<EscrowAccount>(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(o => o.OrderHistory)
            .WithOne(h => h.MarketplaceOrder)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure constraints
        entity.HasCheckConstraint("CK_MarketplaceOrder_Quantity_Positive", "\"Quantity\" > 0");
        entity.HasCheckConstraint("CK_MarketplaceOrder_PricePerToken_Positive", "\"PricePerToken\" > 0");
        entity.HasCheckConstraint("CK_MarketplaceOrder_TotalAmount_Positive", "\"TotalAmount\" > 0");
        entity.HasCheckConstraint("CK_MarketplaceOrder_FinalAmount_Positive", "\"FinalAmount\" > 0");
        entity.HasCheckConstraint("CK_MarketplaceOrder_RetryCount_NonNegative", "\"RetryCount\" >= 0");
        entity.HasCheckConstraint("CK_MarketplaceOrder_MaxRetryAttempts_Positive", "\"MaxRetryAttempts\" > 0");
    }

    private static void ConfigurePaymentTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PaymentTransaction>();

        // Relationship to marketplace order
        entity.HasOne<MarketplaceOrder>()
            .WithMany()
            .HasForeignKey(pt => pt.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes and constraints
        entity.HasIndex(pt => pt.OrderId);
        entity.HasCheckConstraint("CK_PaymentTransaction_Amount_Positive", "\"Amount\" >= 0");
    }

    private static void ConfigureEscrowAccount(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EscrowAccount>();

        // Configure relationships
        entity.HasOne(e => e.MarketplaceOrder)
            .WithOne(o => o.EscrowAccount)
            .HasForeignKey<EscrowAccount>(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(e => e.EscrowHistory)
            .WithOne(h => h.EscrowAccount)
            .HasForeignKey(h => h.EscrowAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure constraints
        entity.HasCheckConstraint("CK_EscrowAccount_EscrowAmount_Positive", "\"EscrowAmount\" > 0");
        entity.HasCheckConstraint("CK_EscrowAccount_TokenQuantity_Positive", "\"TokenQuantity\" > 0");
        entity.HasCheckConstraint("CK_EscrowAccount_AutoReleaseDelayHours_Positive", "\"AutoReleaseDelayHours\" > 0");
        entity.HasCheckConstraint("CK_EscrowAccount_ReleaseAttempts_NonNegative", "\"ReleaseAttempts\" >= 0");
        entity.HasCheckConstraint("CK_EscrowAccount_MaxReleaseAttempts_Positive", "\"MaxReleaseAttempts\" > 0");

        // Configure asset type constraints
        entity.HasCheckConstraint("CK_EscrowAccount_PlatformToken_TokenId", 
            "(\"AssetType\" = 1 AND \"TokenId\" IS NOT NULL) OR (\"AssetType\" != 1 AND \"AssetSymbol\" IS NOT NULL)");
    }

    private static void ConfigurePriceHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PriceHistory>();

        // Configure relationships
        entity.HasOne(p => p.MarketListing)
            .WithMany(l => l.PriceHistory)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure constraints
        entity.HasCheckConstraint("CK_PriceHistory_PricePerToken_Positive", "\"PricePerToken\" > 0");
        entity.HasCheckConstraint("CK_PriceHistory_MarketPrice_Positive", "\"MarketPrice\" IS NULL OR \"MarketPrice\" > 0");
        entity.HasCheckConstraint("CK_PriceHistory_MarginPercentage_Range", "\"MarginPercentage\" IS NULL OR (\"MarginPercentage\" >= -1 AND \"MarginPercentage\" <= 10)");
    }

    private static void ConfigureOrderHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrderHistory>();

        // Configure relationships
        entity.HasOne(h => h.MarketplaceOrder)
            .WithMany(o => o.OrderHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureEscrowHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EscrowHistory>();

        // Configure relationships
        entity.HasOne(h => h.EscrowAccount)
            .WithMany(e => e.EscrowHistory)
            .HasForeignKey(h => h.EscrowAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure constraints
        entity.HasCheckConstraint("CK_EscrowHistory_Amount_Positive", "\"Amount\" IS NULL OR \"Amount\" > 0");
    }

    private static void ConfigureTimestampEntities(ModelBuilder modelBuilder)
    {
        // Configure automatic timestamp updates for all entities implementing ITimestampEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITimestampEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ITimestampEntity.CreatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ITimestampEntity.UpdatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
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
                    break;
            }
        }
    }
}
