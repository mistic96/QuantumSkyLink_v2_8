using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TokenService.Data.Entities;

[Table("TokenTransfers")]
[Index(nameof(FromAccountId), nameof(Status))]
[Index(nameof(ToAccountId), nameof(Status))]
[Index(nameof(QuantumLedgerTransactionId), IsUnique = true)]
[Index(nameof(TokenId), nameof(CreatedAt))]
public class TokenTransfer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TokenId { get; set; }
    public Token Token { get; set; } = null!;
    
    [Required]
    public Guid FromAccountId { get; set; }
    
    [Required]
    public Guid ToAccountId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(36,18)")]
    public decimal Amount { get; set; }
    
    // QuantumLedger Integration
    [Required]
    public Guid QuantumLedgerTransactionId { get; set; }
    
    [StringLength(66)]
    public string? ExternalTransactionHash { get; set; }
    
    [StringLength(100)]
    public string? MultiChainTransactionId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Rolled_Back
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    [StringLength(500)]
    public string? FailureReason { get; set; }
    
    // Fee information
    [Column(TypeName = "decimal(18,8)")]
    public decimal TransactionFee { get; set; }
    
    [StringLength(10)]
    public string? FeeCurrency { get; set; }
    
    // Gas information for external chains
    [Column(TypeName = "decimal(18,0)")]
    public decimal? GasUsed { get; set; }
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal? GasPrice { get; set; }
    
    /// <summary>
    /// Validates the token transfer
    /// </summary>
    public bool IsValid()
    {
        if (TokenId == Guid.Empty) return false;
        if (FromAccountId == Guid.Empty) return false;
        if (ToAccountId == Guid.Empty) return false;
        if (Amount <= 0) return false;
        if (QuantumLedgerTransactionId == Guid.Empty) return false;
        if (TransactionFee < 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// Marks the transfer as completed
    /// </summary>
    public void MarkAsCompleted(string? externalTxHash = null, string? multiChainTxId = null)
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        ExternalTransactionHash = externalTxHash;
        MultiChainTransactionId = multiChainTxId;
    }
    
    /// <summary>
    /// Marks the transfer as failed
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        Status = "Failed";
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the transfer as rolled back
    /// </summary>
    public void MarkAsRolledBack(string reason)
    {
        Status = "Rolled_Back";
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
    }
}
