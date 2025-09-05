using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Wallet transaction request model for compatibility with the old MobileOrchestrator
/// </summary>
public class WalletTransactionRequest
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
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the start date
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction type
    /// </summary>
    public string? TransactionType { get; set; }
}
