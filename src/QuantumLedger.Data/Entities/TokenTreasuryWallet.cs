using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Data.Entities;

[Table("TokenTreasuryWallets")]
public class TokenTreasuryWallet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TokenId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string WalletAddress { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
