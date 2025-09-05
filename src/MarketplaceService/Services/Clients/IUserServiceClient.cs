using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for UserService integration
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Get user profile information
    /// </summary>
    [Get("/api/users/{userId}")]
    Task<UserProfileResponse> GetUserProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user KYC/AML status
    /// </summary>
    [Get("/api/users/{userId}/verification-status")]
    Task<UserVerificationStatusResponse> GetUserVerificationStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user can perform marketplace operations
    /// </summary>
    [Get("/api/users/{userId}/marketplace-permissions")]
    Task<MarketplacePermissionsResponse> GetMarketplacePermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's trading statistics
    /// </summary>
    [Get("/api/users/{userId}/trading-stats")]
    Task<UserTradingStatsResponse> GetUserTradingStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user's marketplace activity
    /// </summary>
    [Post("/api/users/{userId}/marketplace-activity")]
    Task<ApiResponse> UpdateMarketplaceActivityAsync(
        Guid userId,
        [Body] UpdateMarketplaceActivityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's reputation score
    /// </summary>
    [Get("/api/users/{userId}/reputation")]
    Task<UserReputationResponse> GetUserReputationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate user exists and is active
    /// </summary>
    [Get("/api/users/{userId}/validate")]
    Task<UserValidationResponse> ValidateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request models for UserService integration
/// </summary>
public class UpdateMarketplaceActivityRequest
{
    public string ActivityType { get; set; } = string.Empty; // "ListingCreated", "OrderPlaced", "TransactionCompleted"
    public decimal? TransactionAmount { get; set; }
    public string? Currency { get; set; }
    public Guid? RelatedEntityId { get; set; } // ListingId, OrderId, etc.
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response models for UserService integration
/// </summary>
public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserVerificationStatusResponse
{
    public Guid UserId { get; set; }
    public bool IsKycVerified { get; set; }
    public bool IsAmlVerified { get; set; }
    public bool IsIdentityVerified { get; set; }
    public string VerificationLevel { get; set; } = string.Empty; // "Basic", "Enhanced", "Premium"
    public List<string> CompletedVerifications { get; set; } = new();
    public List<string> PendingVerifications { get; set; } = new();
    public DateTime? LastVerificationUpdate { get; set; }
    public decimal DailyTransactionLimit { get; set; }
    public decimal MonthlyTransactionLimit { get; set; }
}

public class MarketplacePermissionsResponse
{
    public Guid UserId { get; set; }
    public bool CanCreateListings { get; set; }
    public bool CanPurchaseTokens { get; set; }
    public bool CanSellTokens { get; set; }
    public bool CanTradeExternalCrypto { get; set; }
    public bool CanCreatePrimaryMarketListings { get; set; }
    public bool CanCreateSecondaryMarketListings { get; set; }
    public decimal MaxListingValue { get; set; }
    public decimal MaxTransactionValue { get; set; }
    public List<string> RestrictedAssets { get; set; } = new();
    public List<string> AllowedPaymentMethods { get; set; } = new();
    public string? RestrictionReason { get; set; }
}

public class UserTradingStatsResponse
{
    public Guid UserId { get; set; }
    public int TotalListingsCreated { get; set; }
    public int ActiveListings { get; set; }
    public int CompletedTransactions { get; set; }
    public decimal TotalVolumeTraded { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageTransactionSize { get; set; }
    public decimal SuccessfulTransactionRate { get; set; }
    public int BuyerRating { get; set; }
    public int SellerRating { get; set; }
    public DateTime FirstTransactionDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public Dictionary<string, decimal> VolumeByAssetType { get; set; } = new();
}

public class UserReputationResponse
{
    public Guid UserId { get; set; }
    public decimal ReputationScore { get; set; }
    public string ReputationLevel { get; set; } = string.Empty; // "New", "Bronze", "Silver", "Gold", "Platinum"
    public int PositiveReviews { get; set; }
    public int NegativeReviews { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public List<ReputationFactor> ReputationFactors { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class ReputationFactor
{
    public string FactorType { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Score { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UserValidationResponse
{
    public bool IsValid { get; set; }
    public bool IsActive { get; set; }
    public bool Exists { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastActiveAt { get; set; }
}
