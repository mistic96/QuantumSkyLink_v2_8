namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Deposit response model
/// </summary>
public class DepositResponse
{
    /// <summary>
    /// Gets or sets the wallet ID
    /// </summary>
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the blockchain network
    /// </summary>
    public string BlockchainNetwork { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the deposit address
    /// </summary>
    public string DepositAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the memo or tag
    /// </summary>
    public string? Memo { get; set; }
    
    /// <summary>
    /// Gets or sets the QR code URL
    /// </summary>
    public string? QrCodeUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum deposit amount
    /// </summary>
    public decimal? MinimumDepositAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the expected confirmation time in minutes
    /// </summary>
    public int? ExpectedConfirmationTime { get; set; }
    
    /// <summary>
    /// Gets or sets the reference ID
    /// </summary>
    public string? ReferenceId { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit instructions
    /// </summary>
    public string? DepositInstructions { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction ID
    /// </summary>
    public string? TransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated confirmation time
    /// </summary>
    public TimeSpan? EstimatedConfirmationTime { get; set; }
    
    /// <summary>
    /// Gets or sets the expiry time
    /// </summary>
    public DateTime? ExpiryTime { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum amount
    /// </summary>
    public decimal? MinimumAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum amount
    /// </summary>
    public decimal? MaximumAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the network fee
    /// </summary>
    public decimal? NetworkFee { get; set; }
    
    /// <summary>
    /// Gets or sets the instructions
    /// </summary>
    public string? Instructions { get; set; }
}
