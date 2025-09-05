using Microsoft.EntityFrameworkCore;
using ComplianceService.Data.Entities;

namespace ComplianceService.Data;

public class ComplianceDbContext : DbContext
{
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<KycVerification> KycVerifications { get; set; }
    public DbSet<ComplianceCase> ComplianceCases { get; set; }
    public DbSet<CaseDocument> CaseDocuments { get; set; }
    public DbSet<CaseReview> CaseReviews { get; set; }
    public DbSet<ComplianceEvent> ComplianceEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure case number generation
        modelBuilder.Entity<ComplianceCase>()
            .Property(c => c.CaseNumber)
            .HasDefaultValueSql("'CASE-' || EXTRACT(YEAR FROM NOW()) || '-' || LPAD(NEXTVAL('case_number_seq')::TEXT, 6, '0')");

        // Configure decimal precision for risk scores
        modelBuilder.Entity<KycVerification>()
            .Property(k => k.RiskScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CaseReview>()
            .Property(r => r.ConfidenceScore)
            .HasPrecision(5, 2);

        // Configure relationships with cascade delete behavior
        modelBuilder.Entity<ComplianceCase>()
            .HasOne(c => c.KycVerification)
            .WithMany(k => k.ComplianceCases)
            .HasForeignKey(c => c.KycVerificationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CaseDocument>()
            .HasOne(d => d.ComplianceCase)
            .WithMany(c => c.CaseDocuments)
            .HasForeignKey(d => d.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseReview>()
            .HasOne(r => r.ComplianceCase)
            .WithMany(c => c.CaseReviews)
            .HasForeignKey(r => r.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplianceEvent>()
            .HasOne(e => e.KycVerification)
            .WithMany(k => k.ComplianceEvents)
            .HasForeignKey(e => e.KycVerificationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ComplianceEvent>()
            .HasOne(e => e.ComplianceCase)
            .WithMany(c => c.ComplianceEvents)
            .HasForeignKey(e => e.CaseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure indexes for performance
        modelBuilder.Entity<KycVerification>()
            .HasIndex(k => new { k.UserId, k.Status })
            .HasDatabaseName("IX_KycVerifications_UserId_Status");

        modelBuilder.Entity<ComplianceCase>()
            .HasIndex(c => new { c.UserId, c.Status })
            .HasDatabaseName("IX_ComplianceCases_UserId_Status");

        modelBuilder.Entity<ComplianceCase>()
            .HasIndex(c => new { c.Priority, c.Status })
            .HasDatabaseName("IX_ComplianceCases_Priority_Status");

        modelBuilder.Entity<ComplianceEvent>()
            .HasIndex(e => new { e.UserId, e.EventType })
            .HasDatabaseName("IX_ComplianceEvents_UserId_EventType");

        modelBuilder.Entity<ComplianceEvent>()
            .HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_ComplianceEvents_Timestamp");

        // Configure sequence for case numbers
        modelBuilder.HasSequence<int>("case_number_seq")
            .StartsAt(1)
            .IncrementsBy(1);
    }
}
