using Microsoft.EntityFrameworkCore;
using InfrastructureService.Data.Entities;

namespace InfrastructureService.Data;

public class InfrastructureDbContext : DbContext
{
    public InfrastructureDbContext(DbContextOptions<InfrastructureDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletSigner> WalletSigners { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionSignature> TransactionSignatures { get; set; }
    public DbSet<WalletBalance> WalletBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minimal fluent API - only for complex scenarios that annotations cannot handle
        // Following the established pattern of heavy annotations with minimal fluent API

        // Configure decimal precision for financial calculations
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 8);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.LockedBalance)
            .HasPrecision(18, 8);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 8);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.GasPrice)
            .HasPrecision(18, 8);

        modelBuilder.Entity<WalletBalance>()
            .Property(wb => wb.Balance)
            .HasPrecision(18, 8);

        modelBuilder.Entity<WalletBalance>()
            .Property(wb => wb.LockedBalance)
            .HasPrecision(18, 8);

        modelBuilder.Entity<WalletBalance>()
            .Property(wb => wb.AvailableBalance)
            .HasPrecision(18, 8);

        modelBuilder.Entity<WalletBalance>()
            .Property(wb => wb.UsdValue)
            .HasPrecision(18, 8);

        modelBuilder.Entity<WalletBalance>()
            .Property(wb => wb.TokenPrice)
            .HasPrecision(18, 8);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps for entities that have UpdatedAt property
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Wallet wallet)
                wallet.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is WalletSigner signer)
                signer.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Transaction transaction)
                transaction.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is WalletBalance balance)
                balance.LastUpdated = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
