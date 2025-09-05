namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Transfer response model
/// </summary>
public class TransferResponse
{
    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source wallet ID
    /// </summary>
    public string SourceWalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination wallet ID
    /// </summary>
    public string DestinationWalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the fee amount
    /// </summary>
    public decimal FeeAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the fee currency code
    /// </summary>
    public string FeeCurrencyCode { get; set; } = string.Empty;
    
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
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the reference ID
    /// </summary>
    public string? ReferenceId { get; set; }
    
    /// <summary>
    /// Gets or sets the blockchain transaction ID
    /// </summary>
    public string? BlockchainTransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the blockchain network
    /// </summary>
    public string? BlockchainNetwork { get; set; }
}
