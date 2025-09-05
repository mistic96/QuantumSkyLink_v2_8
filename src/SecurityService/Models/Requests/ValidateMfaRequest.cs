using System.ComponentModel.DataAnnotations;

namespace SecurityService.Models.Requests;

public class ValidateMfaRequest
{
    [Required]
    [StringLength(50)]
    public string TokenType { get; set; } = string.Empty; // TOTP, SMS, Email, BackupCode

    [Required]
    [StringLength(20, MinimumLength = 4)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Purpose { get; set; } = string.Empty; // Login, Transaction, AccountChange, etc.

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
