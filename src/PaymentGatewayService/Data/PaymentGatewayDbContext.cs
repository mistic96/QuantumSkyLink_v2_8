using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Data;

public class PaymentGatewayDbContext : DbContext
{
    public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<DepositCode> DepositCodes { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
    public DbSet<PaymentProvider> PaymentProviders { get; set; } = null!;
    public DbSet<RefundTransaction> RefundTransactions { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure DepositCode entity
        modelBuilder.Entity<DepositCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });
        
        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
        
        // Configure PaymentTransaction entity
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
        
        // Configure PaymentProvider entity
        modelBuilder.Entity<PaymentProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
        
        // Configure RefundTransaction entity
        modelBuilder.Entity<RefundTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
    }
}
