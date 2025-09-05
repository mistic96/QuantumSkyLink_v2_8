using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Wallet transaction response model for compatibility with the old MobileOrchestrator
/// </summary>
public class WalletTransactionResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the wallet address
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transactions
    /// </summary>
    public List<WalletTransactionItem> Transactions { get; set; } = new List<WalletTransactionItem>();
    
    /// <summary>
    /// Gets or sets the total count
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Wallet transaction item model for compatibility with the old MobileOrchestrator
/// </summary>
public class WalletTransactionItem
{
    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
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
    /// Gets or sets the amount in USD
    /// </summary>
    [JsonPropertyName("amountUSD")]
    public decimal AmountUSD { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the source address
    /// </summary>
    public string SourceAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination address
    /// </summary>
    public string DestinationAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the fee
    /// </summary>
    public decimal Fee { get; set; }
    
    /// <summary>
    /// Gets or sets the fee currency code
    /// </summary>
    public string FeeCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
