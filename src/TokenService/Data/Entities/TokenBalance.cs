using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TokenService.Data.Entities;

[Table("TokenBalances")]
[Index(nameof(TokenId), nameof(AccountId), IsUnique = true)]
[Index(nameof(AccountId))]
[Index(nameof(TokenId))]
public class TokenBalance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TokenId { get; set; }
    public Token Token { get; set; } = null!;
    
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(36,18)")]
    public decimal Balance { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(36,18)")]
    public decimal LockedBalance { get; set; }
    
    [Required]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // QuantumLedger synchronization
    public DateTime? LastSyncedWithQuantumLedger { get; set; }
    
    [StringLength(100)]
    public string? LastQuantumLedgerTransactionId { get; set; }
    
    /// <summary>
    /// Gets the available balance (total - locked)
    /// </summary>
    public decimal AvailableBalance => Balance - LockedBalance;
    
    /// <summary>
    /// Validates the token balance
    /// </summary>
    public bool IsValid()
    {
        if (TokenId == Guid.Empty) return false;
        if (AccountId == Guid.Empty) return false;
        if (Balance < 0) return false;
        if (LockedBalance < 0) return false;
        if (LockedBalance > Balance) return false;
        
        return true;
    }
    
    /// <summary>
    /// Updates the balance
    /// </summary>
    public void UpdateBalance(decimal newBalance, decimal newLockedBalance = 0)
    {
        Balance = newBalance;
        LockedBalance = newLockedBalance;
        LastUpdated = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Locks a specific amount
    /// </summary>
    public bool LockAmount(decimal amount)
    {
        if (amount <= 0 || AvailableBalance < amount)
            return false;
            
        LockedBalance += amount;
        LastUpdated = DateTime.UtcNow;
        return true;
    }
    
    /// <summary>
    /// Unlocks a specific amount
    /// </summary>
    public bool UnlockAmount(decimal amount)
    {
        if (amount <= 0 || LockedBalance < amount)
            return false;
            
        LockedBalance -= amount;
        LastUpdated = DateTime.UtcNow;
        return true;
    }
    
    /// <summary>
    /// Deducts from balance (for transfers)
    /// </summary>
    public bool DeductBalance(decimal amount)
    {
        if (amount <= 0 || Balance < amount)
            return false;
            
        Balance -= amount;
        LastUpdated = DateTime.UtcNow;
        return true;
    }
    
    /// <summary>
    /// Adds to balance (for incoming transfers)
    /// </summary>
    public void AddBalance(decimal amount)
    {
        if (amount > 0)
        {
            Balance += amount;
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Marks as synced with QuantumLedger
    /// </summary>
    public void MarkAsSynced(string? transactionId = null)
    {
        LastSyncedWithQuantumLedger = DateTime.UtcNow;
        LastQuantumLedgerTransactionId = transactionId;
    }
}
