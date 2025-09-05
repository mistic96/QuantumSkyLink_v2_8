using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for managing marketplace orders
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new marketplace order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="buyerId">User ID creating the order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order details</returns>
    Task<MarketplaceOrderDto> CreateOrderAsync(CreateOrderRequest request, Guid buyerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID requesting the order (for access control)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details or null if not found</returns>
    Task<MarketplaceOrderDto?> GetOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="status">New status</param>
    /// <param name="userId">User ID performing the update</param>
    /// <param name="notes">Optional notes about the status change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order details</returns>
    Task<MarketplaceOrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid userId, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="orderId">Order ID to cancel</param>
    /// <param name="userId">User ID performing the cancellation</param>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled order details</returns>
    Task<MarketplaceOrderDto> CancelOrderAsync(Guid orderId, Guid userId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated user orders</returns>
    Task<PaginatedOrderResponse> GetUserOrdersAsync(Guid userId, OrderStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders for a specific listing
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="sellerId">Seller ID (for access control)</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated listing orders</returns>
    Task<PaginatedOrderResponse> GetListingOrdersAsync(Guid listingId, Guid sellerId, OrderStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process order payment confirmation
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="paymentTransactionId">Payment transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order details</returns>
    Task<MarketplaceOrderDto> ProcessPaymentConfirmationAsync(Guid orderId, string paymentTransactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID performing the completion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed order details</returns>
    Task<MarketplaceOrderDto> CompleteOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order history
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID requesting the history (for access control)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order history records</returns>
    Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry a failed order
    /// </summary>
    /// <param name="orderId">Order ID to retry</param>
    /// <param name="userId">User ID performing the retry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retried order details</returns>
    Task<MarketplaceOrderDto> RetryOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for order service
/// </summary>
public class CreateOrderRequest
{
    public Guid ListingId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? MaxPricePerToken { get; set; }
    public string? BuyerNotes { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class MarketplaceOrderDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public OrderStatus Status { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerToken { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal PlatformFee { get; set; }
    public decimal TransactionFee { get; set; }
    public decimal TotalFees { get; set; }
    public decimal FinalAmount { get; set; }
    public Guid? EscrowAccountId { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? BlockchainTransactionHash { get; set; }
    public string? BuyerNotes { get; set; }
    public string? SellerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? EstimatedCompletionAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryAttempts { get; set; }
    public bool RequiresManualReview { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related data
    public MarketListingDto? Listing { get; set; }
    public EscrowAccountDto? EscrowAccount { get; set; }
}

public class PaginatedOrderResponse
{
    public IEnumerable<MarketplaceOrderDto> Orders { get; set; } = new List<MarketplaceOrderDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class OrderHistoryDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public OrderStatus? PreviousStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public bool IsSystemAction { get; set; }
    public string? SystemComponent { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EscrowAccountDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public EscrowStatus Status { get; set; }
    public decimal EscrowAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TokenQuantity { get; set; }
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public decimal EscrowFee { get; set; }
    public decimal NetReleaseAmount { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? FundedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsDisputed { get; set; }
    public string? DisputeReason { get; set; }
    public bool AutoReleaseEnabled { get; set; }
    public int AutoReleaseDelayHours { get; set; }
    public DateTime? AutoReleaseAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
