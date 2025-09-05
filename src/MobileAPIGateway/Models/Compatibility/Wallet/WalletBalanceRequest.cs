using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Wallet balance request model for compatibility with the old MobileOrchestrator
/// </summary>
public class WalletBalanceRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the wallet address
    /// </summary>
    public string? WalletAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
}
