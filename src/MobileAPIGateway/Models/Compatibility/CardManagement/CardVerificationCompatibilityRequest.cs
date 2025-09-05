using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.CardManagement;

/// <summary>
/// Card verification compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CardVerificationCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the card ID
    /// </summary>
    [Required]
    public string CardId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the verification code
    /// </summary>
    [Required]
    public string VerificationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the verification type
    /// </summary>
    public string? VerificationType { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
