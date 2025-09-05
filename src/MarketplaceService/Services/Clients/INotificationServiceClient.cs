using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for NotificationService integration
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// Send marketplace listing notification
    /// </summary>
    [Post("/api/notifications/marketplace/listing")]
    Task<NotificationResponse> SendListingNotificationAsync(
        [Body] ListingNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send marketplace order notification
    /// </summary>
    [Post("/api/notifications/marketplace/order")]
    Task<NotificationResponse> SendOrderNotificationAsync(
        [Body] OrderNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send marketplace transaction notification
    /// </summary>
    [Post("/api/notifications/marketplace/transaction")]
    Task<NotificationResponse> SendTransactionNotificationAsync(
        [Body] TransactionNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send price alert notification
    /// </summary>
    [Post("/api/notifications/marketplace/price-alert")]
    Task<NotificationResponse> SendPriceAlertNotificationAsync(
        [Body] PriceAlertNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk notifications for marketplace events
    /// </summary>
    [Post("/api/notifications/marketplace/bulk")]
    Task<BulkNotificationResponse> SendBulkMarketplaceNotificationsAsync(
        [Body] BulkMarketplaceNotificationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request models for NotificationService integration
/// </summary>
public class ListingNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // "ListingCreated", "ListingActivated", "ListingExpired", "ListingPurchased"
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string AssetSymbol { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public List<string> NotificationChannels { get; set; } = new(); // "Email", "SMS", "Push", "InApp"
    public Dictionary<string, object>? Metadata { get; set; }
}

public class OrderNotificationRequest
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // "OrderCreated", "OrderConfirmed", "OrderCompleted", "OrderCancelled"
    public Guid OrderId { get; set; }
    public Guid ListingId { get; set; }
    public string AssetSymbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public List<string> NotificationChannels { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TransactionNotificationRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // "PaymentReceived", "PaymentSent", "EscrowCreated", "EscrowReleased", "EscrowRefunded"
    public Guid TransactionId { get; set; }
    public Guid? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string TransactionType { get; set; } = string.Empty; // "Purchase", "Sale", "Escrow", "Fee"
    public string Status { get; set; } = string.Empty;
    public List<string> NotificationChannels { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class PriceAlertNotificationRequest
{
    public Guid UserId { get; set; }
    public string AssetSymbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal AlertPrice { get; set; }
    public string AlertType { get; set; } = string.Empty; // "PriceAbove", "PriceBelow", "PriceChange"
    public decimal PercentageChange { get; set; }
    public string Currency { get; set; } = "USD";
    public List<string> NotificationChannels { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class BulkMarketplaceNotificationRequest
{
    public List<MarketplaceNotificationItem> Notifications { get; set; } = new();
    public string BatchId { get; set; } = string.Empty;
    public int Priority { get; set; } = 1; // 1 = Low, 2 = Normal, 3 = High, 4 = Critical
    public DateTime? ScheduledAt { get; set; }
}

public class MarketplaceNotificationItem
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> NotificationChannels { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response models for NotificationService integration
/// </summary>
public class NotificationResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid NotificationId { get; set; }
    public string Status { get; set; } = string.Empty; // "Sent", "Queued", "Failed"
    public List<ChannelResult> ChannelResults { get; set; } = new();
    public DateTime SentAt { get; set; }
}

public class ChannelResult
{
    public string Channel { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}

public class BulkNotificationResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string BatchId { get; set; } = string.Empty;
    public int TotalNotifications { get; set; }
    public int SuccessfulNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public List<NotificationResult> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class NotificationResult
{
    public Guid UserId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? NotificationId { get; set; }
    public List<ChannelResult> ChannelResults { get; set; } = new();
}
