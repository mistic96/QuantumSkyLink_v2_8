using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Entities;

[Table("Wallets")]
[Index(nameof(Address), IsUnique = true)]
[Index(nameof(UserId), nameof(WalletType), IsUnique = false)]
[Index(nameof(Status), IsUnique = false)]
public class Wallet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(42)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string WalletType { get; set; } = "Standard"; // Standard, MultiSig, Cold, Hot

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active"; // Active, Suspended, Frozen, Archived

    [Required]
    [StringLength(10)]
    public string Network { get; set; } = "Ethereum"; // Ethereum, Polygon, BSC

    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; } = 0;

    [Column(TypeName = "decimal(18,8)")]
    public decimal LockedBalance { get; set; } = 0;

    [StringLength(1000)]
    public string? EncryptedPrivateKey { get; set; }

    [StringLength(500)]
    public string? PublicKey { get; set; }

    [StringLength(200)]
    public string? DerivationPath { get; set; }

    public int RequiredSignatures { get; set; } = 1;

    public int TotalSigners { get; set; } = 1;

    [StringLength(2000)]
    public string? Metadata { get; set; } // JSON for additional wallet data

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastTransactionAt { get; set; }

    // Navigation properties
    public ICollection<WalletSigner> Signers { get; set; } = new List<WalletSigner>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<WalletBalance> Balances { get; set; } = new List<WalletBalance>();
}
