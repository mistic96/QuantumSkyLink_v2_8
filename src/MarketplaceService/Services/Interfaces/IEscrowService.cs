using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for managing escrow accounts and secure transactions
/// </summary>
public interface IEscrowService
{
    /// <summary>
    /// Create a new escrow account for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created escrow account details</returns>
    Task<EscrowAccountDto> CreateEscrowAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get escrow account details
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="userId">User ID requesting the details (for access control)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Escrow account details or null if not found</returns>
    Task<EscrowAccountDto?> GetEscrowAsync(Guid escrowId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lock assets in escrow
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="lockTransactionHash">Blockchain transaction hash for asset locking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> LockAssetsAsync(Guid escrowId, string? lockTransactionHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fund escrow with payment
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="paymentTransactionId">Payment transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> FundEscrowAsync(Guid escrowId, string paymentTransactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release escrow assets and payment
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="userId">User ID performing the release</param>
    /// <param name="releaseTransactionHash">Blockchain transaction hash for asset release</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> ReleaseEscrowAsync(Guid escrowId, Guid userId, string? releaseTransactionHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel escrow and return assets
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="userId">User ID performing the cancellation</param>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> CancelEscrowAsync(Guid escrowId, Guid userId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiate dispute for escrow
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="userId">User ID initiating the dispute</param>
    /// <param name="reason">Dispute reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> InitiateDisputeAsync(Guid escrowId, Guid userId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve dispute for escrow
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="adminUserId">Admin user ID resolving the dispute</param>
    /// <param name="resolution">Dispute resolution details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> ResolveDisputeAsync(Guid escrowId, Guid adminUserId, string resolution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get escrow history
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="userId">User ID requesting the history (for access control)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Escrow history records</returns>
    Task<IEnumerable<EscrowHistoryDto>> GetEscrowHistoryAsync(Guid escrowId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get escrows ready for automatic release
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of escrow accounts ready for auto-release</returns>
    Task<IEnumerable<EscrowAccountDto>> GetEscrowsReadyForAutoReleaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Process automatic escrow release
    /// </summary>
    /// <param name="escrowId">Escrow account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated escrow account details</returns>
    Task<EscrowAccountDto> ProcessAutoReleaseAsync(Guid escrowId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for escrow service
/// </summary>
public class EscrowHistoryDto
{
    public Guid Id { get; set; }
    public EscrowStatus Status { get; set; }
    public EscrowStatus? PreviousStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public bool IsSystemAction { get; set; }
    public string? SystemComponent { get; set; }
    public string? TransactionHash { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
