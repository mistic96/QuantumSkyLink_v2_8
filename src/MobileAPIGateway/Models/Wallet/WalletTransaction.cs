namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Wallet transaction model
/// </summary>
public class WalletTransaction
{
    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the wallet ID
    /// </summary>
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction type
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the reference ID
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the fee amount
    /// </summary>
    public decimal FeeAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the fee currency code
    /// </summary>
    public string FeeCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the blockchain transaction ID
    /// </summary>
    public string? BlockchainTransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the blockchain network
    /// </summary>
    public string? BlockchainNetwork { get; set; }
}
