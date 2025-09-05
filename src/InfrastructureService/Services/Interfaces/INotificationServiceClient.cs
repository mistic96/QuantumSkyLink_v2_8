using Refit;

namespace InfrastructureService.Services.Interfaces;

public interface INotificationServiceClient
{
    [Post("/api/notifications/wallet")]
    Task<ApiResponse<NotificationResponse>> SendWalletNotificationAsync([Body] WalletNotificationRequest request);

    [Post("/api/notifications/transaction")]
    Task<ApiResponse<NotificationResponse>> SendTransactionNotificationAsync([Body] TransactionNotificationRequest request);

    [Post("/api/notifications/security")]
    Task<ApiResponse<NotificationResponse>> SendSecurityNotificationAsync([Body] SecurityNotificationRequest request);

    [Post("/api/notifications/system")]
    Task<ApiResponse<NotificationResponse>> SendSystemNotificationAsync([Body] SystemNotificationRequest request);
}

public class WalletNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // WalletCreated, BalanceUpdated, SignerAdded, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string WalletAddress { get; set; } = string.Empty;
    public string? Network { get; set; }
    public decimal? Amount { get; set; }
    public string? TokenSymbol { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TransactionNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // TransactionCreated, TransactionSigned, TransactionConfirmed, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid TransactionId { get; set; }
    public string? TransactionHash { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SecurityNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // SignatureRequired, UnauthorizedAccess, SecurityAlert, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string? WalletAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SystemNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // SystemMaintenance, NetworkUpdate, ServiceAlert, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public DateTime? ScheduledTime { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
