using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class WalletTransactionResponse
{
    public Guid TransactionId { get; set; }
    
    public Guid WalletId { get; set; }
    
    public Guid UserId { get; set; }
    
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Credit, Debit, Transfer
    
    public decimal Amount { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed, Cancelled
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [StringLength(200)]
    public string? FromAddress { get; set; }
    
    [StringLength(200)]
    public string? ToAddress { get; set; }
    
    [StringLength(100)]
    public string? TransactionHash { get; set; }
    
    public decimal? Fee { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public int? BlockNumber { get; set; }
    
    public int? Confirmations { get; set; }
}
