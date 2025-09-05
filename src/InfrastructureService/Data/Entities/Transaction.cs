using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Entities;

[Table("Transactions")]
[Index(nameof(Hash), IsUnique = true)]
[Index(nameof(WalletId), nameof(Status), IsUnique = false)]
[Index(nameof(FromAddress), IsUnique = false)]
[Index(nameof(ToAddress), IsUnique = false)]
[Index(nameof(CreatedAt), IsUnique = false)]
public class Transaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WalletId { get; set; }

    [StringLength(66)]
    public string? Hash { get; set; } // Blockchain transaction hash

    [Required]
    [StringLength(42)]
    public string FromAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(42)]
    public string ToAddress { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string TokenSymbol { get; set; } = "ETH";

    [StringLength(42)]
    public string? TokenAddress { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal GasPrice { get; set; }

    [Required]
    public long GasLimit { get; set; }

    public long? GasUsed { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Failed, Cancelled

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = "Transfer"; // Transfer, Contract, MultiSig, Swap

    [Required]
    [StringLength(10)]
    public string Network { get; set; } = "Ethereum";

    public long? BlockNumber { get; set; }

    public int? TransactionIndex { get; set; }

    public long Nonce { get; set; }

    [StringLength(2000)]
    public string? Data { get; set; } // Transaction data/input

    [StringLength(1000)]
    public string? Metadata { get; set; } // JSON for additional transaction data

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    public int RequiredSignatures { get; set; } = 1;

    public int CurrentSignatures { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? BroadcastAt { get; set; }

    // Navigation properties
    [ForeignKey("WalletId")]
    public Wallet Wallet { get; set; } = null!;

    public ICollection<TransactionSignature> Signatures { get; set; } = new List<TransactionSignature>();
}
