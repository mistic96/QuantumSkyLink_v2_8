using Microsoft.EntityFrameworkCore;
using UserService.Data.Entities;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserWallet> UserWallets { get; set; }
    public DbSet<UserSecuritySettings> UserSecuritySettings { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minimal Fluent API - Only for one-to-one relationships that annotations can't handle
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<UserWallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.SecuritySettings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSecuritySettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TimeSpan for PostgreSQL
        modelBuilder.Entity<UserSecuritySettings>()
            .Property(e => e.LockoutDuration)
            .HasConversion(
                v => v.Ticks,
                v => new TimeSpan(v));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                    user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is UserProfile profile)
            {
                if (entry.State == EntityState.Added)
                    profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is UserWallet wallet)
            {
                if (entry.State == EntityState.Added)
                    wallet.CreatedAt = DateTime.UtcNow;
                wallet.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is UserSecuritySettings security)
            {
                if (entry.State == EntityState.Added)
                    security.CreatedAt = DateTime.UtcNow;
                security.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Role role)
            {
                if (entry.State == EntityState.Added)
                    role.CreatedAt = DateTime.UtcNow;
                role.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Permission permission)
            {
                if (entry.State == EntityState.Added)
                    permission.CreatedAt = DateTime.UtcNow;
                permission.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
