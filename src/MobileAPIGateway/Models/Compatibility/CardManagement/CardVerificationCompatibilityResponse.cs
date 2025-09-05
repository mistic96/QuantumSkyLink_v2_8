namespace MobileAPIGateway.Models.Compatibility.CardManagement;

/// <summary>
/// Card verification compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CardVerificationCompatibilityResponse
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
    /// Gets or sets the card ID
    /// </summary>
    public string CardId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the verification status
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the verification type
    /// </summary>
    public string? VerificationType { get; set; }
    
    /// <summary>
    /// Gets or sets the verification date
    /// </summary>
    public DateTime VerificationDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
