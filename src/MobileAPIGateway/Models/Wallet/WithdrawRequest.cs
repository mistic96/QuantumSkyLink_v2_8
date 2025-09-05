using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Withdraw request model
/// </summary>
public class WithdrawRequest
{
    /// <summary>
    /// Gets or sets the wallet ID
    /// </summary>
    [Required]
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    [Required]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    [Required]
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination address
    /// </summary>
    [Required]
    public string DestinationAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the blockchain network
    /// </summary>
    [Required]
    public string BlockchainNetwork { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the reference ID
    /// </summary>
    [StringLength(100)]
    public string? ReferenceId { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to include fee in the amount
    /// </summary>
    public bool IncludeFee { get; set; }
    
    /// <summary>
    /// Gets or sets the memo or tag
    /// </summary>
    [StringLength(100)]
    public string? Memo { get; set; }
}
