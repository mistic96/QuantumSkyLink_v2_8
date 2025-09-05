namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Wallet balance model
/// </summary>
public class WalletBalance
{
    /// <summary>
    /// Gets or sets the wallet ID
    /// </summary>
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the balance
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Gets or sets the available balance
    /// </summary>
    public decimal AvailableBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the pending balance
    /// </summary>
    public decimal PendingBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the locked balance
    /// </summary>
    public decimal LockedBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
