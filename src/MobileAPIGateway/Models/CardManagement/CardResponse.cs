using System;

namespace MobileAPIGateway.Models.CardManagement;

/// <summary>
/// Represents a response for card operations
/// </summary>
public class CardResponse
{
    /// <summary>
    /// Gets or sets the operation status
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the operation message
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Gets or sets the card ID
    /// </summary>
    public string CardId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the operation
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the card data
    /// </summary>
    public PaymentCard Card { get; set; }
    
    /// <summary>
    /// Gets or sets the verification status if applicable
    /// </summary>
    public string VerificationStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the verification details if applicable
    /// </summary>
    public string VerificationDetails { get; set; }
}
