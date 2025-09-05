using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; } = "en";

    [MaxLength(10)]
    public string? PreferredCurrency { get; set; } = "USD";

    public bool AcceptTerms { get; set; } = false;

    public bool AcceptPrivacyPolicy { get; set; } = false;
}
