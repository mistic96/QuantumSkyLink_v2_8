using Microsoft.EntityFrameworkCore;
using QuantumLedger.Models.Account;

namespace QuantumLedger.Data;

/// <summary>
/// Database context for the multi-cloud account management system.
/// Supports 1M+ accounts with optimized indexing for high-performance operations.
/// </summary>
public class AccountsContext : DbContext
{
    public AccountsContext(DbContextOptions<AccountsContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the accounts DbSet.
    /// </summary>
    public DbSet<Account> Accounts { get; set; } = null!;

    /// <summary>
    /// Gets or sets the account keys DbSet.
    /// </summary>
    public DbSet<AccountKey> AccountKeys { get; set; } = null!;

    /// <summary>
    /// Gets or sets the public key registry DbSet.
    /// </summary>
    public DbSet<PublicKeyRegistryEntry> PublicKeyRegistry { get; set; } = null!;

    /// <summary>
    /// Gets or sets the request nonces DbSet.
    /// </summary>
    public DbSet<RequestNonce> RequestNonces { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.ExternalOwnerId).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OwnerIdType).HasMaxLength(50);
            entity.Property(e => e.VendorSystem).HasMaxLength(100);
            entity.Property(e => e.InternalReferenceId).HasMaxLength(26);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");

            // Indexes for high-performance lookups
            entity.HasIndex(e => new { e.ExternalOwnerId, e.VendorSystem })
                  .HasDatabaseName("IX_Accounts_ExternalOwner_Vendor");
            entity.HasIndex(e => e.InternalReferenceId)
                  .HasDatabaseName("IX_Accounts_InternalReference");
            entity.HasIndex(e => e.OwnerIdType)
                  .HasDatabaseName("IX_Accounts_OwnerIdType");
            entity.HasIndex(e => e.Status)
                  .HasDatabaseName("IX_Accounts_Status");
        });

        // Configure AccountKey entity
        modelBuilder.Entity<AccountKey>(entity =>
        {
            entity.HasKey(e => e.KeyId);
            entity.Property(e => e.Algorithm).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PublicKey).IsRequired();
            entity.Property(e => e.CloudProvider).IsRequired().HasMaxLength(20);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.AccountKeys)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.AccountId)
                  .HasDatabaseName("IX_AccountKeys_AccountId");
            entity.HasIndex(e => new { e.Algorithm, e.Status })
                  .HasDatabaseName("IX_AccountKeys_Algorithm_Status");
            entity.HasIndex(e => e.CloudProvider)
                  .HasDatabaseName("IX_AccountKeys_CloudProvider");

            // Check constraints for valid algorithms and providers
            entity.HasCheckConstraint("CK_AccountKeys_Algorithm", 
                "\"Algorithm\" IN ('Dilithium', 'Falcon', 'EC256')");
            entity.HasCheckConstraint("CK_AccountKeys_CloudProvider", 
                "\"CloudProvider\" IN ('AWS', 'Azure', 'GoogleCloud')");
        });

        // Configure PublicKeyRegistryEntry entity
        modelBuilder.Entity<PublicKeyRegistryEntry>(entity =>
        {
            entity.HasKey(e => e.PublicKeyHash);
            entity.Property(e => e.PublicKeyHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Algorithm).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PublicKey).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UsageCount).HasDefaultValue(0);

            // Foreign key relationship
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.PublicKeyEntries)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for high-performance signature verification
            entity.HasIndex(e => e.AccountId)
                  .HasDatabaseName("IX_PublicKeyRegistry_AccountId");
            entity.HasIndex(e => new { e.Algorithm, e.Status })
                  .HasDatabaseName("IX_PublicKeyRegistry_Algorithm_Status");
            entity.HasIndex(e => e.LastUsed)
                  .HasDatabaseName("IX_PublicKeyRegistry_LastUsed");

            // Check constraint for valid algorithms
            entity.HasCheckConstraint("CK_PublicKeyRegistry_Algorithm", 
                "\"Algorithm\" IN ('Dilithium', 'Falcon', 'EC256')");
        });

        // Configure RequestNonce entity
        modelBuilder.Entity<RequestNonce>(entity =>
        {
            entity.HasKey(e => e.NonceHash);
            entity.Property(e => e.NonceHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.OriginalNonce).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Timestamp).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.RequestType).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            // Foreign key relationship
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.RequestNonces)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for nonce validation and cleanup
            entity.HasIndex(e => e.AccountId)
                  .HasDatabaseName("IX_RequestNonces_AccountId");
            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("IX_RequestNonces_ExpiresAt");
            entity.HasIndex(e => e.Timestamp)
                  .HasDatabaseName("IX_RequestNonces_Timestamp");
        });
    }
}
