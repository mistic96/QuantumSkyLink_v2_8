namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Transfer compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class TransferCompatibilityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the transfer was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source wallet address
    /// </summary>
    public string SourceWalletAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination wallet address
    /// </summary>
    public string DestinationWalletAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the fee
    /// </summary>
    public decimal Fee { get; set; }
    
    /// <summary>
    /// Gets or sets the fee currency code
    /// </summary>
    public string FeeCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
