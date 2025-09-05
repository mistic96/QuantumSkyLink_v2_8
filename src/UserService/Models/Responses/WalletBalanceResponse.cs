using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class WalletBalanceResponse
{
    public Guid WalletId { get; set; }
    
    public Guid UserId { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;
    
    public decimal AvailableBalance { get; set; }
    
    public decimal PendingBalance { get; set; }
    
    public decimal TotalBalance { get; set; }
    
    public decimal FrozenBalance { get; set; }
    
    public DateTime LastUpdated { get; set; }
    
    [StringLength(200)]
    public string? WalletAddress { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool IsFrozen { get; set; }
}
