using LiquidStorageCloud.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LiquidStorageCloud.DataManagement.EntityFramework;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id)
                .UseIdentityColumn() // SQL Server auto-increment
                .HasColumnType("bigint"); // Use bigint for large number of records
            
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.EntityType).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Namespace).IsRequired();
            entity.Property(e => e.TableName).IsRequired();
            entity.Property(e => e.EntityData).IsRequired()
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            // Indexes
            entity.HasIndex(e => e.EntityType)
                .HasDatabaseName("IX_Messages_EntityType");

            entity.HasIndex(e => e.EntityId)
                .HasDatabaseName("IX_Messages_EntityId");

            entity.HasIndex(e => e.SolidState)
                .HasDatabaseName("IX_Messages_SolidState");

            // Composite index for common query patterns
            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                .HasDatabaseName("IX_Messages_EntityType_EntityId");

            entity.HasIndex(e => new { e.EntityType, e.SolidState })
                .HasDatabaseName("IX_Messages_EntityType_SolidState");
        });
    }
}
