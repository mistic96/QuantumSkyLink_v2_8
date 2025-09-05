using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountTransaction> AccountTransactions { get; set; }
    public DbSet<AccountVerification> AccountVerifications { get; set; }
    public DbSet<AccountLimit> AccountLimits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minimal fluent API - only for cases where annotations cannot handle
        // Following UserService pattern of heavy annotations with minimal fluent API

        // Account number generation sequence (PostgreSQL specific)
        modelBuilder.HasSequence<long>("AccountNumberSequence")
            .StartsAt(1000000)
            .IncrementsBy(1);

        // Ensure decimal precision for financial calculations
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Balance)
                .HasPrecision(18, 8);
            entity.Property(e => e.DailyLimit)
                .HasPrecision(18, 8);
            entity.Property(e => e.MonthlyLimit)
                .HasPrecision(18, 8);
        });

        modelBuilder.Entity<AccountTransaction>(entity =>
        {
            entity.Property(e => e.Amount)
                .HasPrecision(18, 8);
            entity.Property(e => e.Fee)
                .HasPrecision(18, 8);
            entity.Property(e => e.BalanceAfter)
                .HasPrecision(18, 8);
        });

        modelBuilder.Entity<AccountLimit>(entity =>
        {
            entity.Property(e => e.LimitAmount)
                .HasPrecision(18, 8);
            entity.Property(e => e.UsedAmount)
                .HasPrecision(18, 8);
        });

        // Seed default account limits for new accounts
        SeedDefaultLimits(modelBuilder);
    }

    private static void SeedDefaultLimits(ModelBuilder modelBuilder)
    {
        // Default limits that will be applied to new accounts
        // These are template limits - actual limits will be created per account
        var defaultLimits = new[]
        {
            new { Id = Guid.NewGuid(), LimitType = LimitType.DailyWithdrawal, Amount = 10000m, Period = LimitPeriod.Daily },
            new { Id = Guid.NewGuid(), LimitType = LimitType.DailyDeposit, Amount = 50000m, Period = LimitPeriod.Daily },
            new { Id = Guid.NewGuid(), LimitType = LimitType.MonthlyWithdrawal, Amount = 100000m, Period = LimitPeriod.Monthly },
            new { Id = Guid.NewGuid(), LimitType = LimitType.MonthlyDeposit, Amount = 500000m, Period = LimitPeriod.Monthly },
            new { Id = Guid.NewGuid(), LimitType = LimitType.SingleTransaction, Amount = 25000m, Period = LimitPeriod.PerTransaction }
        };

        // Note: Actual seeding will be handled in service layer for per-account limits
        // This is just for reference of default limit structure
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving (following UserService pattern)
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Account account)
            {
                if (entry.State == EntityState.Added)
                {
                    account.CreatedAt = DateTime.UtcNow;
                }
                account.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is AccountTransaction transaction)
            {
                if (entry.State == EntityState.Added)
                {
                    transaction.CreatedAt = DateTime.UtcNow;
                    transaction.Timestamp = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is AccountVerification verification)
            {
                if (entry.State == EntityState.Added)
                {
                    verification.CreatedAt = DateTime.UtcNow;
                    verification.SubmittedAt = DateTime.UtcNow;
                }
                verification.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is AccountLimit limit)
            {
                if (entry.State == EntityState.Added)
                {
                    limit.CreatedAt = DateTime.UtcNow;
                }
                limit.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
