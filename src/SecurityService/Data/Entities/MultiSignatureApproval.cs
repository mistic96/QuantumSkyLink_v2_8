using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SecurityService.Data.Entities;

[Table("MultiSignatureApprovals")]
[Index(nameof(RequestId), nameof(ApprovedBy), IsUnique = true)]
[Index(nameof(RequestId), nameof(Status), IsUnique = false)]
[Index(nameof(ApprovedBy), nameof(CreatedAt), IsUnique = false)]
public class MultiSignatureApproval
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RequestId { get; set; }

    [Required]
    public Guid ApprovedBy { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // Approved, Rejected

    [Required]
    [MaxLength(500)]
    public string Comments { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string SignatureData { get; set; } = string.Empty; // Cryptographic signature

    [Required]
    [MaxLength(100)]
    public string SignatureMethod { get; set; } = string.Empty; // ECDSA, RSA, etc.

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("RequestId")]
    public MultiSignatureRequest Request { get; set; } = null!;
}
