using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Data;

/// <summary>
/// Database context for the Payment Gateway Service
/// </summary>
public class PaymentDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the PaymentDbContext class
    /// </summary>
    /// <param name="options">The database context options</param>
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the payments DbSet
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Gets or sets the payment gateways DbSet
    /// </summary>
    public DbSet<PaymentGateway> PaymentGateways { get; set; }

    /// <summary>
    /// Gets or sets the payment methods DbSet
    /// </summary>
    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    /// <summary>
    /// Gets or sets the payment attempts DbSet
    /// </summary>
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; }

    /// <summary>
    /// Gets or sets the refunds DbSet
    /// </summary>
    public DbSet<Refund> Refunds { get; set; }

    /// <summary>
    /// Gets or sets the payment webhooks DbSet
    /// </summary>
    public DbSet<PaymentWebhook> PaymentWebhooks { get; set; }

    /// <summary>
    /// Gets or sets the deposit codes DbSet
    /// </summary>
    public DbSet<DepositCode> DepositCodes { get; set; }

    /// <summary>
    /// Gets or sets the users DbSet (simplified for PaymentGatewayService)
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configures the model using Entity Framework conventions and minimal fluent API
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply heavy EF annotations pattern - minimal fluent API usage
        // Only use fluent API for complex scenarios that cannot be handled by annotations

        // Configure timestamp entities to automatically set CreatedAt and UpdatedAt
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

        // Configure decimal precision for all decimal properties
        // This is one case where fluent API is more efficient than annotations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    // Only set precision if not already configured via annotations
                    if (property.GetColumnType() == null)
                    {
                        property.SetColumnType("decimal(18,8)");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saves changes to the database with automatic timestamp updates
    /// </summary>
    /// <returns>The number of state entries written to the database</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves changes to the database asynchronously with automatic timestamp updates
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the timestamps for entities that implement ITimestampEntity
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<ITimestampEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Prevent CreatedAt from being modified
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
