using Microsoft.EntityFrameworkCore;
using QuantumLedger.Models;
using QuantumLedger.Data.Entities;

namespace QuantumLedger.Data;

public class LedgerContext : DbContext
{
    public LedgerContext(DbContextOptions<LedgerContext> options) : base(options) { }
    
    // Security audit logging for compliance and monitoring
    public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }

    // Object storage references for published objects (S3 metadata)
    public DbSet<ObjectStorageReference> ObjectStorageReferences { get; set; }
    
    // Define DbSets for your entities
    public DbSet<MultisigWallet> MultisigWallets { get; set; }
    public DbSet<MultisigPublicKey> MultisigPublicKeys { get; set; }
    public DbSet<ChainNetwork> ChainNetworks { get; set; }

    // Token lifecycle entities
    public DbSet<Token> Tokens { get; set; }
    public DbSet<TokenDecision> TokenDecisions { get; set; }
    public DbSet<TokenChainAddress> TokenChainAddresses { get; set; }
    public DbSet<QuantumLedger.Models.ExternalMintRecord> ExternalMintRecords { get; set; }
    public DbSet<TokenTreasuryWallet> TokenTreasuryWallets { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure SecurityAuditLog entity
        modelBuilder.Entity<SecurityAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RequestId).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(20);
            
            // Create indexes for common queries
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_SecurityAuditLogs_Timestamp");
            entity.HasIndex(e => e.EventType).HasDatabaseName("IX_SecurityAuditLogs_EventType");
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_SecurityAuditLogs_Category");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_SecurityAuditLogs_UserId");
            entity.HasIndex(e => e.IpAddress).HasDatabaseName("IX_SecurityAuditLogs_IpAddress");
            entity.HasIndex(e => e.Severity).HasDatabaseName("IX_SecurityAuditLogs_Severity");
            entity.HasIndex(e => e.RequiresAttention).HasDatabaseName("IX_SecurityAuditLogs_RequiresAttention");
            
            // Composite indexes for common query patterns
            entity.HasIndex(e => new { e.Timestamp, e.Category }).HasDatabaseName("IX_SecurityAuditLogs_Timestamp_Category");
            entity.HasIndex(e => new { e.UserId, e.Timestamp }).HasDatabaseName("IX_SecurityAuditLogs_UserId_Timestamp");
        });
    }
}
