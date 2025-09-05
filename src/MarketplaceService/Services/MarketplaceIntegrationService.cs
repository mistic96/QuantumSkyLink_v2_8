using Microsoft.Extensions.Logging;
using MarketplaceService.Services.Interfaces;
using MarketplaceService.Services.Clients;

namespace MarketplaceService.Services;

public class MarketplaceIntegrationService : IMarketplaceIntegrationService
{
    private readonly ILogger<MarketplaceIntegrationService> _logger;
    private readonly Clients.ITokenServiceClient _tokenServiceClient;
    private readonly Clients.IPaymentGatewayServiceClient _paymentGatewayServiceClient;
    private readonly Clients.IFeeServiceClient _feeServiceClient;
    private readonly Clients.IUserServiceClient _userServiceClient;
    private readonly Clients.INotificationServiceClient _notificationServiceClient;
    private readonly Clients.IComplianceServiceClient _complianceServiceClient;

    public MarketplaceIntegrationService(
        ILogger<MarketplaceIntegrationService> logger,
        Clients.ITokenServiceClient tokenServiceClient,
        Clients.IPaymentGatewayServiceClient paymentGatewayServiceClient,
        Clients.IFeeServiceClient feeServiceClient,
        Clients.IUserServiceClient userServiceClient,
        Clients.INotificationServiceClient notificationServiceClient,
        Clients.IComplianceServiceClient complianceServiceClient)
    {
        _logger = logger;
        _tokenServiceClient = tokenServiceClient;
        _paymentGatewayServiceClient = paymentGatewayServiceClient;
        _feeServiceClient = feeServiceClient;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _complianceServiceClient = complianceServiceClient;
    }

    public async Task<TokenOwnershipResult> ValidateTokenOwnershipAsync(Guid userId, Guid tokenId, decimal quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating token ownership for user {UserId}, token {TokenId}, quantity {Quantity}", userId, tokenId, quantity);
            
            var result = await _tokenServiceClient.ValidateTokenOwnershipAsync(tokenId, userId, quantity, cancellationToken);
            
            _logger.LogInformation("Token ownership validation completed for user {UserId}: {IsValid}", userId, result.IsValid);
            
            // Map from Clients response to Interfaces response
            return new TokenOwnershipResult
            {
                IsValid = result.IsValid,
                UserId = userId,
                TokenId = tokenId,
                OwnedQuantity = result.AvailableBalance,
                RequestedQuantity = quantity,
                HasSufficientBalance = result.HasSufficientBalance,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token ownership for user {UserId}, token {TokenId}", userId, tokenId);
            return new TokenOwnershipResult
            {
                IsValid = false,
                UserId = userId,
                TokenId = tokenId,
                RequestedQuantity = quantity,
                HasSufficientBalance = false,
                ErrorMessage = $"Token ownership validation failed: {ex.Message}"
            };
        }
    }

    public async Task<TokenLifecycleStatusResult> GetTokenLifecycleStatusAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying TokenService lifecycle status for token {TokenId}", tokenId);

            // Calls TokenService via the Refit client and maps the response
            var resp = await _tokenServiceClient.GetTokenLifecycleStatusAsync(tokenId, cancellationToken);

            return new TokenLifecycleStatusResult
            {
                IsLifecycleComplete = resp.IsLifecycleComplete,
                CurrentStage = resp.CurrentStage,
                CanBeListedInMarketplace = resp.CanBeListedInMarketplace,
                BlockingReason = resp.BlockingReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying lifecycle status for token {TokenId}", tokenId);
            return new TokenLifecycleStatusResult
            {
                IsLifecycleComplete = false,
                CurrentStage = "Unknown",
                CanBeListedInMarketplace = false,
                BlockingReason = "Lifecycle check failed"
            };
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing payment for user {UserId}, amount {Amount} {Currency}", 
                paymentRequest.UserId, paymentRequest.Amount, paymentRequest.Currency);
            
            // Map from Interfaces request to Clients request
            var clientRequest = new Clients.ProcessPaymentRequest
            {
                PayerId = paymentRequest.UserId,
                PayeeId = Guid.Empty, // Will be set by the marketplace logic
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency,
                PaymentMethodId = paymentRequest.PaymentMethod,
                Description = paymentRequest.Description ?? "",
                Metadata = paymentRequest.Metadata != null ? new Dictionary<string, string> { { "original", paymentRequest.Metadata } } : new Dictionary<string, string>()
            };

            // Propagate method hint for PaymentRouter (if provided)
            if (!string.IsNullOrWhiteSpace(paymentRequest.PaymentMethod))
            {
                clientRequest.Metadata["method"] = paymentRequest.PaymentMethod;
            }
            
            var result = await _paymentGatewayServiceClient.ProcessPaymentAsync(clientRequest, cancellationToken);
            
            _logger.LogInformation("Payment processing completed for user {UserId}: {IsSuccessful}", 
                paymentRequest.UserId, result.IsSuccessful);
            
            // Map from Clients response to Interfaces response
            return new PaymentResult
            {
                IsSuccessful = result.IsSuccessful,
                PaymentId = result.PaymentId,
                TransactionId = result.TransactionId,
                Status = result.Status,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for user {UserId}", paymentRequest.UserId);
            return new PaymentResult
            {
                IsSuccessful = false,
                Status = "Failed",
                ErrorMessage = $"Payment processing failed: {ex.Message}"
            };
        }
    }

    public async Task<FeeCalculationResult> CalculateFeesAsync(FeeCalculationRequest feeRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating fees for transaction type {TransactionType}, amount {Amount} {Currency}", 
                feeRequest.TransactionType, feeRequest.Amount, feeRequest.Currency);
            
            // Map from Interfaces request to Clients request
            var clientRequest = new Clients.CalculateTransactionFeeRequest
            {
                BuyerId = feeRequest.UserId ?? Guid.Empty,
                SellerId = Guid.Empty, // Will be set by marketplace logic
                TransactionAmount = feeRequest.Amount,
                Currency = feeRequest.Currency,
                AssetType = "PlatformToken", // Default, can be overridden
                MarketType = feeRequest.TransactionType,
                PaymentMethod = "Default"
            };
            
            var result = await _feeServiceClient.CalculateTransactionFeeAsync(clientRequest, cancellationToken);
            
            _logger.LogInformation("Fee calculation completed: Total fee {TotalFee}", result.TotalFee);
            
            // Map from Clients response to Interfaces response
            return new FeeCalculationResult
            {
                PlatformFee = result.TotalFee * 0.7m, // Assume 70% platform fee
                TransactionFee = result.TotalFee * 0.3m, // Assume 30% transaction fee
                TotalFees = result.TotalFee,
                Currency = result.Currency,
                FeeBreakdown = string.Join(", ", result.FeeBreakdown.Select(f => $"{f.FeeType}: {f.Amount}")),
                IsSuccessful = result.IsSuccessful,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fees for transaction type {TransactionType}", feeRequest.TransactionType);
            return new FeeCalculationResult
            {
                IsSuccessful = false,
                Currency = feeRequest.Currency,
                ErrorMessage = $"Fee calculation failed: {ex.Message}"
            };
        }
    }

    public async Task<UserVerificationResult> VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying user {UserId}", userId);
            
            var result = await _userServiceClient.GetUserProfileAsync(userId, cancellationToken);
            
            _logger.LogInformation("User verification completed for {UserId}: {IsActive}", userId, result.IsActive);
            
            // Map from Clients response to Interfaces response
            return new UserVerificationResult
            {
                IsValid = result.IsActive,
                UserId = result.Id,
                Username = result.DisplayName,
                Email = result.Email,
                IsVerified = result.IsVerified,
                IsActive = result.IsActive,
                VerificationLevel = "Basic", // Default level
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying user {UserId}", userId);
            return new UserVerificationResult
            {
                IsValid = false,
                UserId = userId,
                IsVerified = false,
                IsActive = false,
                ErrorMessage = $"User verification failed: {ex.Message}"
            };
        }
    }

    public async Task<MarketplacePermissionsResponse> GetMarketplacePermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching marketplace permissions for user {UserId}", userId);

            var resp = await _userServiceClient.GetMarketplacePermissionsAsync(userId, cancellationToken);

            // Return the client response directly (Clients.MarketplacePermissionsResponse)
            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching marketplace permissions for user {UserId}", userId);
            return new MarketplacePermissionsResponse
            {
                UserId = userId,
                CanCreateListings = false,
                CanPurchaseTokens = false,
                CanSellTokens = false,
                CanTradeExternalCrypto = false,
                CanCreatePrimaryMarketListings = false,
                CanCreateSecondaryMarketListings = false,
                MaxListingValue = 0,
                MaxTransactionValue = 0,
                RestrictedAssets = new List<string>(),
                AllowedPaymentMethods = new List<string>(),
                RestrictionReason = "Permission check failed"
            };
        }
    }

    public async Task<Interfaces.NotificationResult> SendNotificationAsync(NotificationRequest notificationRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending notification to user {UserId}, type {Type}: {Title}", 
                notificationRequest.UserId, notificationRequest.Type, notificationRequest.Title);
            
            // Map from Interfaces request to Clients request
            var clientRequest = new Clients.ListingNotificationRequest
            {
                UserId = notificationRequest.UserId,
                NotificationType = notificationRequest.Type,
                ListingId = Guid.Empty, // Will be set by marketplace logic
                ListingTitle = notificationRequest.Title,
                AssetSymbol = "Unknown",
                NotificationChannels = new List<string> { "Email", "InApp" },
                Metadata = notificationRequest.Metadata != null ? new Dictionary<string, object> { { "original", notificationRequest.Metadata } } : null
            };
            
            var result = await _notificationServiceClient.SendListingNotificationAsync(clientRequest, cancellationToken);
            
            _logger.LogInformation("Notification sent to user {UserId}: {IsSuccessful}", 
                notificationRequest.UserId, result.IsSuccessful);
            
            // Map from Clients response to Interfaces response
            return new Interfaces.NotificationResult
            {
                IsSuccessful = result.IsSuccessful,
                NotificationId = result.NotificationId,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", notificationRequest.UserId);
            return new Interfaces.NotificationResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Notification sending failed: {ex.Message}"
            };
        }
    }

    public async Task<ComplianceResult> PerformComplianceCheckAsync(ComplianceRequest complianceRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing compliance check for user {UserId}, type {CheckType}", 
                complianceRequest.UserId, complianceRequest.CheckType);
            
            // Map from Interfaces request to Clients request
            var clientRequest = new Clients.MarketplaceComplianceRequest
            {
                UserId = complianceRequest.UserId,
                OperationType = complianceRequest.CheckType,
                AssetType = "PlatformToken", // Default
                TransactionAmount = complianceRequest.TransactionAmount,
                Currency = complianceRequest.Currency,
                Metadata = complianceRequest.Metadata != null ? new Dictionary<string, object> { { "original", complianceRequest.Metadata } } : null
            };
            
            var result = await _complianceServiceClient.VerifyMarketplaceComplianceAsync(clientRequest, cancellationToken);
            
            _logger.LogInformation("Compliance check completed for user {UserId}: {IsCompliant}", 
                complianceRequest.UserId, result.IsCompliant);
            
            // Map from Clients response to Interfaces response
            return new ComplianceResult
            {
                IsCompliant = result.IsCompliant,
                Status = result.ComplianceStatus,
                RequiredActions = result.RequiredActions,
                Warnings = new List<string>(), // Default empty warnings
                ErrorMessage = result.RejectionReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing compliance check for user {UserId}", complianceRequest.UserId);
            return new ComplianceResult
            {
                IsCompliant = false,
                Status = "Failed",
                RequiredActions = new[] { "Manual review required due to system error" },
                ErrorMessage = $"Compliance check failed: {ex.Message}"
            };
        }
    }
}
