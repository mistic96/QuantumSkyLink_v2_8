namespace MobileAPIGateway.Models.CardManagement;

/// <summary>
/// Represents a request to verify a payment card
/// </summary>
public class CardVerificationRequest
{
    /// <summary>
    /// Gets or sets the card ID to verify
    /// </summary>
    public string CardId { get; set; }
    
    /// <summary>
    /// Gets or sets the verification code
    /// </summary>
    public string VerificationCode { get; set; }
    
    /// <summary>
    /// Gets or sets the verification method (e.g., MicroDeposit, 3DS, etc.)
    /// </summary>
    public string VerificationMethod { get; set; }
}
