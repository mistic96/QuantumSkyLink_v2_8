namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Customer
/// </summary>
public record Customer
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Gets or sets the phone
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Gets or sets the address
    /// </summary>
    public OrderPaymentAddress? Address { get; set; }
    
    /// <summary>
    /// Gets or sets the payment information
    /// </summary>
    public PaymentInfo? PaymentInfo { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the customer has valid identity screening on file
    /// </summary>
    public bool HasValidIdentityScreeningOnFile { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the account is locked or frozen
    /// </summary>
    public bool IsAccountLockedOrFrozen { get; set; }
    
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Gets or sets the preferred name
    /// </summary>
    public string? PreferredName { get; set; }
    
    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public Guid Tag { get; set; }
    
    /// <summary>
    /// Gets or sets the identity total transaction amount
    /// </summary>
    public decimal IdentityTotalTransactionAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the external ID
    /// </summary>
    public string? ExternalId { get; set; }
}
