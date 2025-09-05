using System.ComponentModel.DataAnnotations;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models.Requests;

/// <summary>
/// Request model for creating a payment method
/// </summary>
public class CreatePaymentMethodRequest
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway ID
    /// </summary>
    [Required]
    public Guid PaymentGatewayId { get; set; }

    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    [Required]
    public PaymentMethodType MethodType { get; set; }

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gateway method ID
    /// </summary>
    [StringLength(500)]
    public string? GatewayMethodId { get; set; }

    /// <summary>
    /// Gets or sets whether this should be the default method
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the expiry date for cards
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the last 4 digits
    /// </summary>
    [StringLength(4)]
    public string? Last4Digits { get; set; }

    /// <summary>
    /// Gets or sets the card brand
    /// </summary>
    [StringLength(50)]
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the country code
    /// </summary>
    [StringLength(2)]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    [StringLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public Dictionary<string, object>? BillingAddress { get; set; }
}

/// <summary>
/// Request model for updating a payment method
/// </summary>
public class UpdatePaymentMethodRequest
{
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    [Required]
    public Guid MethodId { get; set; }

    /// <summary>
    /// Gets or sets the user ID (for authorization)
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    [StringLength(200)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets whether the method is active
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether this should be the default method
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the expiry date for cards
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public Dictionary<string, object>? BillingAddress { get; set; }
}
