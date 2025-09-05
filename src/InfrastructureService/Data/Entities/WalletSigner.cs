using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Entities;

[Table("WalletSigners")]
[Index(nameof(WalletId), nameof(UserId), IsUnique = true)]
[Index(nameof(Status), IsUnique = false)]
public class WalletSigner
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WalletId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(42)]
    public string SignerAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active"; // Active, Suspended, Revoked

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Signer"; // Owner, Signer, Observer

    public int SigningWeight { get; set; } = 1;

    [StringLength(500)]
    public string? PublicKey { get; set; }

    [StringLength(1000)]
    public string? Permissions { get; set; } // JSON for specific permissions

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSignedAt { get; set; }

    // Navigation properties
    [ForeignKey("WalletId")]
    public Wallet Wallet { get; set; } = null!;

    public ICollection<TransactionSignature> Signatures { get; set; } = new List<TransactionSignature>();
}
