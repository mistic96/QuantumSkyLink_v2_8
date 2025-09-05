using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Data.Entities;

[Table("UserSecuritySettings")]
public class UserSecuritySettings
{
    [Key]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public bool IsMfaEnabled { get; set; } = false;

    [MaxLength(50)]
    public string? MfaMethod { get; set; }

    [MaxLength(255)]
    public string? MfaSecret { get; set; }

    public bool IsEmailNotificationEnabled { get; set; } = true;

    public bool IsSmsNotificationEnabled { get; set; } = false;

    public bool IsLoginNotificationEnabled { get; set; } = true;

    public bool IsTransactionNotificationEnabled { get; set; } = true;

    public bool IsSecurityAlertEnabled { get; set; } = true;

    public int MaxLoginAttempts { get; set; } = 5;

    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    public bool IsAccountLocked { get; set; } = false;

    public DateTime? LockedUntil { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LastFailedLoginAt { get; set; }

    [MaxLength(255)]
    public string? LastKnownIpAddress { get; set; }

    [MaxLength(500)]
    public string? LastKnownUserAgent { get; set; }

    public bool RequirePasswordChange { get; set; } = false;

    public DateTime? PasswordChangedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual User User { get; set; } = null!;
}
