using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Wallet balance response model for compatibility with the old MobileOrchestrator
/// </summary>
public class WalletBalanceResponse
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
    /// Gets or sets the available balance
    /// </summary>
    public decimal AvailableBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the pending balance
    /// </summary>
    public decimal PendingBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the total balance
    /// </summary>
    public decimal TotalBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the balance in USD
    /// </summary>
    [JsonPropertyName("balanceUSD")]
    public decimal BalanceUSD { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
