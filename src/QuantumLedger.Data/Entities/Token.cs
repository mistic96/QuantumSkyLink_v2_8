using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Data.Entities;

[Table("Tokens")]
public class Token
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ContractAddress { get; set; } = string.Empty;
    
    public int Decimals { get; set; } = 18;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
