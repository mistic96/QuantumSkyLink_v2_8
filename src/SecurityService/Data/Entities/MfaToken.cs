using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SecurityService.Data.Entities;

[Table("MfaTokens")]
[Index(nameof(UserId), nameof(TokenType), nameof(IsUsed), IsUnique = false)]
[Index(nameof(UserId), nameof(ExpiresAt), IsUnique = false)]
[Index(nameof(TokenHash), IsUnique = true)]
public class MfaToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TokenType { get; set; } = string.Empty; // TOTP, SMS, Email, BackupCode

    [Required]
    [MaxLength(256)]
    public string TokenHash { get; set; } = string.Empty; // Hashed token for security

    [Required]
    [MaxLength(100)]
    public string Purpose { get; set; } = string.Empty; // Login, Transaction, AccountChange, etc.

    [Required]
    public bool IsUsed { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DeliveryMethod { get; set; } // SMS, Email, App

    [MaxLength(100)]
    public string? DeliveryTarget { get; set; } // Phone number, email address

    [Required]
    public int AttemptCount { get; set; } = 0;

    [Required]
    public int MaxAttempts { get; set; } = 3;

    [Required]
    public bool IsBlocked { get; set; } = false;

    public DateTime? BlockedAt { get; set; }

    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";
}
