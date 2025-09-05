using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents a user's payment method (card, bank account, etc.)
/// </summary>
[Table("PaymentMethods")]
[Index(nameof(UserId))]
[Index(nameof(IsActive))]
[Index(nameof(MethodType))]
[Index(nameof(PaymentGatewayId))]
[Index(nameof(IsVerified))]
public class PaymentMethod : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment method
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who owns this payment method
    /// </summary>
    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway associated with this method
    /// </summary>
    [Required]
    [ForeignKey("PaymentGateway")]
    public Guid PaymentGatewayId { get; set; }

    /// <summary>
    /// Gets or sets the type of payment method
    /// </summary>
    [Required]
    public PaymentMethodType MethodType { get; set; }

    /// <summary>
    /// Gets or sets the display name for this payment method
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external gateway method ID
    /// </summary>
    [MaxLength(500)]
    public string? GatewayMethodId { get; set; }

    /// <summary>
    /// Gets or sets whether this payment method is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this payment method has been verified
    /// </summary>
    [Required]
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this is the default payment method for the user
    /// </summary>
    [Required]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets additional metadata for the payment method (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the expiry date for cards
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the last 4 digits of the card/account number
    /// </summary>
    [MaxLength(4)]
    public string? Last4Digits { get; set; }

    /// <summary>
    /// Gets or sets the card brand (Visa, MasterCard, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the country code for the payment method
    /// </summary>
    [MaxLength(2)]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the currency supported by this payment method
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the billing address (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? BillingAddress { get; set; }

    /// <summary>
    /// Gets or sets when this payment method was last used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets when this payment method was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Gets or sets the payment gateway associated with this method
    /// </summary>
    public PaymentGateway PaymentGateway { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of payments made with this method
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
