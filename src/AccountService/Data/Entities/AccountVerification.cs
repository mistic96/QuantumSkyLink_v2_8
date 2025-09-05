using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data.Entities;

[Table("AccountVerifications")]
[Index(nameof(AccountId), nameof(VerificationType), IsUnique = false)]
[Index(nameof(Status), IsUnique = false)]
[Index(nameof(SubmittedAt), IsUnique = false)]
public class AccountVerification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Account")]
    public Guid AccountId { get; set; }

    [Required]
    public VerificationType VerificationType { get; set; }

    [Required]
    public VerificationStatus Status { get; set; }

    [MaxLength(100)]
    public string? ExternalVerificationId { get; set; }

    [MaxLength(500)]
    public string? DocumentType { get; set; }

    [MaxLength(1000)]
    public string? DocumentPath { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    [MaxLength(50)]
    public string? CorrelationId { get; set; }

    [MaxLength(2000)]
    public string? Metadata { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Account Account { get; set; } = null!;
}

public enum VerificationType
{
    Identity = 1,
    Address = 2,
    Income = 3,
    Business = 4,
    BankAccount = 5,
    CreditCheck = 6,
    KYC = 7,
    AML = 8,
    Enhanced = 9
}

public enum VerificationStatus
{
    Pending = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    Expired = 5,
    RequiresAdditionalInfo = 6
}
