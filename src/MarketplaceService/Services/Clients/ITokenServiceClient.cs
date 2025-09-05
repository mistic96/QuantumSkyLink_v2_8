using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for TokenService integration
/// </summary>
public interface ITokenServiceClient
{
    /// <summary>
    /// Validate token ownership for marketplace listing
    /// </summary>
    [Get("/api/tokens/{tokenId}/ownership/{userId}")]
    Task<TokenOwnershipValidationResponse> ValidateTokenOwnershipAsync(
        Guid tokenId, 
        Guid userId, 
        [Query] decimal quantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get token details for marketplace display
    /// </summary>
    [Get("/api/tokens/{tokenId}")]
    Task<TokenDetailsResponse> GetTokenDetailsAsync(
        Guid tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if token has completed full lifecycle
    /// </summary>
    [Get("/api/tokens/{tokenId}/lifecycle-status")]
    Task<TokenLifecycleStatusResponse> GetTokenLifecycleStatusAsync(
        Guid tokenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's token balance
    /// </summary>
    [Get("/api/tokens/{tokenId}/balance/{userId}")]
    Task<TokenBalanceResponse> GetUserTokenBalanceAsync(
        Guid tokenId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserve tokens for marketplace transaction
    /// </summary>
    [Post("/api/tokens/{tokenId}/reserve")]
    Task<TokenReservationResponse> ReserveTokensAsync(
        Guid tokenId,
        [Body] TokenReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release reserved tokens
    /// </summary>
    [Post("/api/tokens/{tokenId}/release-reservation")]
    Task<ApiResponse> ReleaseTokenReservationAsync(
        Guid tokenId,
        [Body] ReleaseReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer tokens for completed marketplace transaction
    /// </summary>
    [Post("/api/tokens/{tokenId}/transfer")]
    Task<TokenTransferResponse> TransferTokensAsync(
        Guid tokenId,
        [Body] TokenTransferRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response models for TokenService integration
/// </summary>
public class TokenOwnershipValidationResponse
{
    public bool IsValid { get; set; }
    public bool HasSufficientBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TokenDetailsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalSupply { get; set; }
    public decimal CirculatingSupply { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class TokenLifecycleStatusResponse
{
    public bool IsLifecycleComplete { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public bool CanBeListedInMarketplace { get; set; }
    public string? BlockingReason { get; set; }
}

public class TokenBalanceResponse
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class TokenReservationRequest
{
    public Guid UserId { get; set; }
    public decimal Quantity { get; set; }
    public string ReservationReason { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class TokenReservationResponse
{
    public Guid ReservationId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class ReleaseReservationRequest
{
    public Guid ReservationId { get; set; }
    public string ReleaseReason { get; set; } = string.Empty;
}

public class TokenTransferRequest
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public decimal Quantity { get; set; }
    public string TransferReason { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? ReservationId { get; set; }
}

public class TokenTransferResponse
{
    public Guid TransferId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionHash { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class ApiResponse
{
    public bool IsSuccessful { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
