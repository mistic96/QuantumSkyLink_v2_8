using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Data.Entities;

[Table("MultisigPublicKeys")]
public class MultisigPublicKey
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid WalletId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string PublicKey { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string SignerId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
