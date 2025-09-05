using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class WalletValidationResponse
{
    public bool IsValid { get; set; }
    
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? ValidationMessage { get; set; }
    
    [StringLength(100)]
    public string? AddressType { get; set; }
    
    [StringLength(50)]
    public string? Network { get; set; }
    
    public bool IsTestnet { get; set; }
    
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}
