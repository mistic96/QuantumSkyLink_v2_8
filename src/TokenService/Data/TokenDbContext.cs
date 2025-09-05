using Microsoft.EntityFrameworkCore;
using TokenService.Data.Entities;

namespace TokenService.Data;

public class TokenDbContext : DbContext
{
    public TokenDbContext(DbContextOptions<TokenDbContext> options) : base(options)
    {
    }

    public DbSet<Token> Tokens { get; set; }
    public DbSet<TokenSubmission> TokenSubmissions { get; set; }
    public DbSet<TokenTransfer> TokenTransfers { get; set; }
    public DbSet<TokenBalance> TokenBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Token entity relationships
        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasMany(t => t.Submissions)
                  .WithOne(s => s.Token)
                  .HasForeignKey(s => s.TokenId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Transfers)
                  .WithOne(tr => tr.Token)
                  .HasForeignKey(tr => tr.TokenId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Balances)
                  .WithOne(b => b.Token)
                  .HasForeignKey(b => b.TokenId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TokenSubmission entity
        modelBuilder.Entity<TokenSubmission>(entity =>
        {
            entity.HasOne(s => s.Token)
                  .WithMany(t => t.Submissions)
                  .HasForeignKey(s => s.TokenId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure TokenTransfer entity
        modelBuilder.Entity<TokenTransfer>(entity =>
        {
            entity.HasOne(tr => tr.Token)
                  .WithMany(t => t.Transfers)
                  .HasForeignKey(tr => tr.TokenId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TokenBalance entity
        modelBuilder.Entity<TokenBalance>(entity =>
        {
            entity.HasOne(b => b.Token)
                  .WithMany(t => t.Balances)
                  .HasForeignKey(b => b.TokenId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
