using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Entities;

[Table("TransactionSignatures")]
[Index(nameof(TransactionId), nameof(SignerId), IsUnique = true)]
[Index(nameof(Status), IsUnique = false)]
public class TransactionSignature
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    public Guid SignerId { get; set; }

    [Required]
    [StringLength(42)]
    public string SignerAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Signed, Rejected

    [StringLength(132)]
    public string? Signature { get; set; } // r + s + v signature

    [StringLength(66)]
    public string? R { get; set; }

    [StringLength(66)]
    public string? S { get; set; }

    public int? V { get; set; }

    [StringLength(1000)]
    public string? SignatureData { get; set; } // Additional signature metadata

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SignedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    // Navigation properties
    [ForeignKey("TransactionId")]
    public Transaction Transaction { get; set; } = null!;

    [ForeignKey("SignerId")]
    public WalletSigner Signer { get; set; } = null!;
}
