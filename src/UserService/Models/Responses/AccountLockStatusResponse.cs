using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class AccountLockStatusResponse
{
    public bool ShouldLock { get; set; }

    public bool IsCurrentlyLocked { get; set; }

    public int FailedAttempts { get; set; }

    public int MaxAllowedAttempts { get; set; }

    public DateTime? LockoutStartTime { get; set; }

    public DateTime? LockoutEndTime { get; set; }

    public int LockoutDurationMinutes { get; set; }

    public int RemainingLockoutMinutes { get; set; }

    [StringLength(500)]
    public string? LockoutReason { get; set; }

    [StringLength(100)]
    public string? LastFailedAttemptIp { get; set; }

    public DateTime? LastFailedAttemptTime { get; set; }

    public bool RequiresAdminUnlock { get; set; }

    public bool CanRetryAfterCooldown { get; set; }

    public DateTime? NextRetryAllowedTime { get; set; }

    [StringLength(50)]
    public string LockStatus { get; set; } = string.Empty; // None, Temporary, Permanent, AdminLocked

    public int TotalLockoutCount { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
