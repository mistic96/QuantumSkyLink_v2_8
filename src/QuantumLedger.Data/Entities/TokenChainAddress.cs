using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Data.Entities;

[Table("TokenChainAddresses")]
public class TokenChainAddress
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TokenId { get; set; }
    
    [Required]
    public Guid ChainNetworkId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
