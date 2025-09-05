using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Transfer compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class TransferCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source wallet address
    /// </summary>
    [Required]
    public string SourceWalletAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination wallet address
    /// </summary>
    [Required]
    public string DestinationWalletAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    [Required]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    [Required]
    public string CurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the memo
    /// </summary>
    public string? Memo { get; set; }
    
    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public string? Tag { get; set; }
}
