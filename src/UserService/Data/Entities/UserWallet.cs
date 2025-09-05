using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Data.Entities;

[Table("UserWallets")]
public class UserWallet
{
    [Key]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string WalletAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PublicKey { get; set; } = string.Empty;

    [Required]
    public int RequiredSignatures { get; set; } = 2;

    [Required]
    public int TotalSigners { get; set; } = 3;

    [MaxLength(50)]
    public string WalletType { get; set; } = "MultiSig";

    [MaxLength(50)]
    public string Network { get; set; } = "Ethereum";

    public bool IsActive { get; set; } = true;

    public bool IsVerified { get; set; } = false;

    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; } = 0;

    [MaxLength(10)]
    public string BalanceCurrency { get; set; } = "ETH";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastTransactionAt { get; set; }

    // Navigation Property
    public virtual User User { get; set; } = null!;
}
