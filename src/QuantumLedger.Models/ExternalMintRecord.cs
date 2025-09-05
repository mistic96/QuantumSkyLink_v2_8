using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Models;

[Table("ExternalMintRecords")]
public class ExternalMintRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TokenId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ExternalChain { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string TransactionHash { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }
    
    [MaxLength(100)]
    public string MintedTo { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
