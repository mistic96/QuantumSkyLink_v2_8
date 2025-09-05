using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.Data.Entities;

[Table("WalletBalances")]
[Index(nameof(WalletId), nameof(TokenSymbol), IsUnique = true)]
[Index(nameof(TokenAddress), IsUnique = false)]
public class WalletBalance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WalletId { get; set; }

    [Required]
    [StringLength(10)]
    public string TokenSymbol { get; set; } = "ETH";

    [StringLength(42)]
    public string? TokenAddress { get; set; }

    [Required]
    [StringLength(100)]
    public string TokenName { get; set; } = "Ethereum";

    public int TokenDecimals { get; set; } = 18;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal LockedBalance { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal AvailableBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? UsdValue { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? TokenPrice { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public DateTime? LastSyncedAt { get; set; }

    [StringLength(500)]
    public string? Metadata { get; set; } // JSON for additional token data

    // Navigation properties
    [ForeignKey("WalletId")]
    public Wallet Wallet { get; set; } = null!;
}
