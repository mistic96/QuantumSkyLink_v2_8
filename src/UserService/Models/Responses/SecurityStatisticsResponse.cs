using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class SecurityStatisticsResponse
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalEvents { get; set; }

    public int SuccessfulLogins { get; set; }

    public int FailedLogins { get; set; }

    public int PasswordChanges { get; set; }

    public int AccountLockouts { get; set; }

    public int PermissionChanges { get; set; }

    public int HighRiskEvents { get; set; }

    public int CriticalEvents { get; set; }

    public int UniqueUsers { get; set; }

    public int UniqueIpAddresses { get; set; }

    public int SuspiciousActivities { get; set; }

    public decimal FailureRate { get; set; }

    public Dictionary<string, int> EventsByType { get; set; } = new();

    public Dictionary<string, int> EventsBySeverity { get; set; } = new();

    public Dictionary<string, int> EventsByHour { get; set; } = new();

    public List<TopRiskUser> TopRiskUsers { get; set; } = new();

    public List<TopIpAddress> TopIpAddresses { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TopRiskUser
{
    public Guid UserId { get; set; }

    [StringLength(100)]
    public string? UserEmail { get; set; }

    public int RiskScore { get; set; }

    public int EventCount { get; set; }

    public int FailedAttempts { get; set; }
}

public class TopIpAddress
{
    [StringLength(200)]
    public string IpAddress { get; set; } = string.Empty;

    public int EventCount { get; set; }

    public int FailedAttempts { get; set; }

    public int UniqueUsers { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public bool IsSuspicious { get; set; }
}
