using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Basic user entity for PaymentGatewayService relationships
/// This is a simplified representation - full user data is managed by UserService
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name
    /// </summary>
    [MaxLength(200)]
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the user's KYC status
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string KycStatus { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets when the user was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    
    /// <summary>
    /// Gets or sets the payments associated with this user
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Gets or sets the deposit codes associated with this user
    /// </summary>
    public ICollection<DepositCode> DepositCodes { get; set; } = new List<DepositCode>();

    /// <summary>
    /// Gets or sets the payment methods associated with this user
    /// </summary>
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}