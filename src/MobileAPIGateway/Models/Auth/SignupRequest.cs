using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Signup request model for mobile users
/// </summary>
public class SignupRequest
{
    /// <summary>
    /// Gets or sets the email address (used as username)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number (optional)
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the country (optional)
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the city (optional)
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the preferred language (default: en)
    /// </summary>
    [MaxLength(10)]
    public string? PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets the preferred currency (default: USD)
    /// </summary>
    [MaxLength(10)]
    public string? PreferredCurrency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets whether the user accepts terms and conditions
    /// </summary>
    [Required]
    public bool AcceptTerms { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the user accepts privacy policy
    /// </summary>
    [Required]
    public bool AcceptPrivacyPolicy { get; set; } = false;
}
