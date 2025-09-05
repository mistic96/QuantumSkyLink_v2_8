using Refit;
using MarketplaceService.Services.Clients;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for integrating with external services
/// </summary>
public interface IMarketplaceIntegrationService
{
    /// <summary>
    /// Validate token ownership with TokenService
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tokenId">Token ID</param>
    /// <param name="quantity">Quantity to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token ownership validation result</returns>
    Task<TokenOwnershipResult> ValidateTokenOwnershipAsync(Guid userId, Guid tokenId, decimal quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query TokenService for the token lifecycle status and whether it can be listed.
    /// </summary>
    Task<TokenLifecycleStatusResult> GetTokenLifecycleStatusAsync(Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process payment through PaymentGatewayService
    /// </summary>
    /// <param name="paymentRequest">Payment processing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment processing result</returns>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate fees through FeeService
    /// </summary>
    /// <param name="feeRequest">Fee calculation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee calculation result</returns>
    Task<FeeCalculationResult> CalculateFeesAsync(FeeCalculationRequest feeRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user credentials with UserService
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User verification result</returns>
    Task<UserVerificationResult> VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get marketplace permissions for a user (create listings, sell tokens, trade external crypto)
    /// </summary>
    Task<MarketplacePermissionsResponse> GetMarketplacePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification through NotificationService
    /// </summary>
    /// <param name="notificationRequest">Notification request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification sending result</returns>
    Task<NotificationResult> SendNotificationAsync(NotificationRequest notificationRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform compliance check through ComplianceService
    /// </summary>
    /// <param name="complianceRequest">Compliance check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check result</returns>
    Task<ComplianceResult> PerformComplianceCheckAsync(ComplianceRequest complianceRequest, CancellationToken cancellationToken = default);
}

/// <summary>
/// Refit client interfaces for external services
/// </summary>
public interface ITokenServiceClient
{
    [Get("/api/tokens/{tokenId}/ownership/{userId}")]
    Task<TokenOwnershipResult> ValidateOwnershipAsync(Guid tokenId, Guid userId, [Query] decimal quantity);

    [Get("/api/tokens/{tokenId}")]
    Task<TokenDetailsResult> GetTokenDetailsAsync(Guid tokenId);

    [Post("/api/tokens/{tokenId}/transfer")]
    Task<TokenTransferResult> TransferTokenAsync(Guid tokenId, [Body] TokenTransferRequest request);
}

public interface IPaymentGatewayServiceClient
{
    [Post("/api/payments")]
    Task<PaymentResult> ProcessPaymentAsync([Body] PaymentRequest request);

    [Get("/api/payments/{paymentId}")]
    Task<PaymentResult> GetPaymentAsync(Guid paymentId);

    [Post("/api/escrow")]
    Task<EscrowResult> CreateEscrowAsync([Body] EscrowRequest request);

    [Put("/api/escrow/{escrowId}/release")]
    Task<EscrowResult> ReleaseEscrowAsync(Guid escrowId, [Body] EscrowReleaseRequest request);
}

public interface IFeeServiceClient
{
    [Post("/api/fees/calculate")]
    Task<FeeCalculationResult> CalculateFeesAsync([Body] FeeCalculationRequest request);

    [Get("/api/fees/structure")]
    Task<FeeStructureResult> GetFeeStructureAsync();
}

public interface IUserServiceClient
{
    [Get("/api/users/{userId}")]
    Task<UserVerificationResult> GetUserAsync(Guid userId);

    [Get("/api/users/{userId}/verification-status")]
    Task<UserVerificationResult> GetVerificationStatusAsync(Guid userId);
}

public interface INotificationServiceClient
{
    [Post("/api/notifications")]
    Task<NotificationResult> SendNotificationAsync([Body] NotificationRequest request);

    [Post("/api/notifications/bulk")]
    Task<BulkNotificationResult> SendBulkNotificationAsync([Body] BulkNotificationRequest request);
}

public interface IComplianceServiceClient
{
    [Post("/api/compliance/check")]
    Task<ComplianceResult> PerformCheckAsync([Body] ComplianceRequest request);

    [Get("/api/compliance/user/{userId}/status")]
    Task<ComplianceResult> GetUserComplianceStatusAsync(Guid userId);
}

/// <summary>
/// Data transfer objects for external service integration
/// </summary>
public class TokenOwnershipResult
{
    public bool IsValid { get; set; }
    public Guid UserId { get; set; }
    public Guid TokenId { get; set; }
    public decimal OwnedQuantity { get; set; }
    public decimal RequestedQuantity { get; set; }
    public bool HasSufficientBalance { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TokenDetailsResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalSupply { get; set; }
    public decimal CirculatingSupply { get; set; }
    public bool IsActive { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TokenTransferRequest
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
}

public class TokenTransferResult
{
    public bool IsSuccessful { get; set; }
    public string? TransactionHash { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Metadata { get; set; }
}

public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public Guid? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class EscrowRequest
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Terms { get; set; }
}

public class EscrowReleaseRequest
{
    public Guid UserId { get; set; }
    public string? Reason { get; set; }
}

public class EscrowResult
{
    public bool IsSuccessful { get; set; }
    public Guid? EscrowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class FeeCalculationRequest
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid? UserId { get; set; }
    public string? Metadata { get; set; }
}

public class FeeCalculationResult
{
    public decimal PlatformFee { get; set; }
    public decimal TransactionFee { get; set; }
    public decimal TotalFees { get; set; }
    public string Currency { get; set; } = "USD";
    public string? FeeBreakdown { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}

public class FeeStructureResult
{
    public string? FeeStructure { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserVerificationResult
{
    public bool IsValid { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string? VerificationLevel { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NotificationRequest
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public bool IsUrgent { get; set; } = false;
}

public class NotificationResult
{
    public bool IsSuccessful { get; set; }
    public Guid? NotificationId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkNotificationRequest
{
    public IEnumerable<NotificationRequest> Notifications { get; set; } = new List<NotificationRequest>();
}

public class BulkNotificationResult
{
    public int TotalNotifications { get; set; }
    public int SuccessfulNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public IEnumerable<NotificationResult> Results { get; set; } = new List<NotificationResult>();
}

public class ComplianceRequest
{
    public Guid UserId { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public decimal? TransactionAmount { get; set; }
    public string? Currency { get; set; }
    public string? Metadata { get; set; }
}

public class ComplianceResult
{
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<string> RequiredActions { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
    public string? ErrorMessage { get; set; }
}

// Token lifecycle status used by marketplace to decide if a token can be listed.
// This mirrors the TokenService lifecycle response at the integration boundary.
public class TokenLifecycleStatusResult
{
    public bool IsLifecycleComplete { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public bool CanBeListedInMarketplace { get; set; }
    public string? BlockingReason { get; set; }
}
