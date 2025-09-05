using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SecurityService.Data.Entities;

[Table("MultiSignatureRequests")]
[Index(nameof(AccountId), nameof(Status), IsUnique = false)]
[Index(nameof(RequestedBy), nameof(Status), IsUnique = false)]
[Index(nameof(OperationType), nameof(Status), IsUnique = false)]
[Index(nameof(CreatedAt), IsUnique = false)]
public class MultiSignatureRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid RequestedBy { get; set; }

    [Required]
    [MaxLength(100)]
    public string OperationType { get; set; } = string.Empty; // Transfer, Withdrawal, AccountFreeze, etc.

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Amount { get; set; }

    [MaxLength(100)]
    public string? Currency { get; set; }

    [MaxLength(500)]
    public string? DestinationAddress { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string OperationData { get; set; } = "{}";

    [Required]
    public int RequiredSignatures { get; set; }

    [Required]
    public int CurrentSignatures { get; set; } = 0;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Expired, Executed

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ExecutedAt { get; set; }

    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    // Navigation property for signatures
    public ICollection<MultiSignatureApproval> Approvals { get; set; } = new List<MultiSignatureApproval>();
}
