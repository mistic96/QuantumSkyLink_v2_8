using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PaymentGatewayService.Services;

/// <summary>
/// Payment method service implementing IPaymentMethodService
/// Handles secure payment method storage, verification, default method management, and validation
/// </summary>
public class PaymentMethodService : IPaymentMethodService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentMethodService> _logger;
    private readonly IGatewayIntegrationService _gatewayIntegration;
    private readonly IPaymentCacheService _cacheService;
    private readonly IPaymentValidationService _validationService;

    public PaymentMethodService(
        PaymentDbContext context,
        ILogger<PaymentMethodService> logger,
        IGatewayIntegrationService gatewayIntegration,
        IPaymentCacheService cacheService,
        IPaymentValidationService validationService)
    {
        _context = context;
        _logger = logger;
        _gatewayIntegration = gatewayIntegration;
        _cacheService = cacheService;
        _validationService = validationService;
    }

    /// <summary>
    /// Creates a new payment method for a user
    /// </summary>
    public async Task<PaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating payment method. CorrelationId: {CorrelationId}, UserId: {UserId}, Type: {Type}", 
            correlationId, request.UserId, request.MethodType);

        try
        {
            // Get the payment gateway
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.Id == request.PaymentGatewayId && g.IsActive);

            if (gateway == null)
            {
                throw new InvalidOperationException($"Payment gateway with ID {request.PaymentGatewayId} not found or inactive");
            }

            var paymentMethod = new PaymentMethod
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PaymentGatewayId = request.PaymentGatewayId,
                MethodType = request.MethodType,
                DisplayName = request.DisplayName,
                GatewayMethodId = request.GatewayMethodId,
                IsActive = true,
                IsVerified = false, // Will be verified separately
                IsDefault = request.IsDefault,
                ExpiryDate = request.ExpiryDate,
                Last4Digits = request.Last4Digits,
                Brand = request.Brand,
                Country = request.Country,
                Currency = request.Currency,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : "{}",
                BillingAddress = request.BillingAddress != null ? JsonSerializer.Serialize(request.BillingAddress) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Check if this should be the default method (first method for user or explicitly requested)
            var existingMethodsCount = await _context.PaymentMethods
                .CountAsync(pm => pm.UserId == request.UserId && pm.IsActive);

            if (existingMethodsCount == 0 || request.IsDefault)
            {
                // If this is the first method or explicitly requested as default, make it default
                if (request.IsDefault && existingMethodsCount > 0)
                {
                    // Remove default flag from other methods
                    var otherMethods = await _context.PaymentMethods
                        .Where(pm => pm.UserId == request.UserId && pm.IsActive && pm.IsDefault)
                        .ToListAsync();

                    foreach (var method in otherMethods)
                    {
                        method.IsDefault = false;
                        method.UpdatedAt = DateTime.UtcNow;
                    }
                }
                paymentMethod.IsDefault = true;
            }

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            // Verify the payment method with the gateway
            var verificationResult = await _gatewayIntegration.VerifyPaymentMethodAsync(paymentMethod);
            if (verificationResult.IsValid)
            {
                paymentMethod.IsVerified = true;
                paymentMethod.VerifiedAt = DateTime.UtcNow;
                paymentMethod.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Cache the payment method
            await _cacheService.CachePaymentMethodAsync(paymentMethod);

            _logger.LogInformation("Payment method created successfully. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                correlationId, paymentMethod.Id);

            return await MapToResponseAsync(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets a payment method by ID
    /// </summary>
    public async Task<PaymentMethodResponse?> GetPaymentMethodAsync(Guid methodId, Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}, UserId: {UserId}", 
            correlationId, methodId, userId);

        try
        {
            // Try cache first
            var cachedMethod = await _cacheService.GetPaymentMethodAsync(methodId);
            if (cachedMethod != null && cachedMethod.UserId == userId)
            {
                _logger.LogInformation("Payment method retrieved from cache. CorrelationId: {CorrelationId}", correlationId);
                return await MapToResponseAsync(cachedMethod);
            }

            // Fallback to database
            var paymentMethod = await _context.PaymentMethods
                .AsNoTracking()
                .Include(pm => pm.PaymentGateway)
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId && pm.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                return null;
            }

            var response = await MapToResponseAsync(paymentMethod);

            // Cache the result
            await _cacheService.CachePaymentMethodAsync(paymentMethod);

            _logger.LogInformation("Payment method retrieved successfully. CorrelationId: {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets all payment methods for a user
    /// </summary>
    public async Task<IEnumerable<PaymentMethodResponse>> GetUserPaymentMethodsAsync(Guid userId, bool? isActive = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving user payment methods. CorrelationId: {CorrelationId}, UserId: {UserId}, IsActive: {IsActive}", 
            correlationId, userId, isActive);

        try
        {
            var query = _context.PaymentMethods
                .AsNoTracking()
                .Include(pm => pm.PaymentGateway)
                .Where(pm => pm.UserId == userId);

            if (isActive.HasValue)
            {
                query = query.Where(pm => pm.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(pm => pm.IsActive); // Default to active only
            }

            var paymentMethods = await query
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.LastUsedAt)
                .ThenBy(pm => pm.CreatedAt)
                .ToListAsync();

            var responses = new List<PaymentMethodResponse>();
            foreach (var method in paymentMethods)
            {
                responses.Add(await MapToResponseAsync(method));
            }

            _logger.LogInformation("User payment methods retrieved successfully. CorrelationId: {CorrelationId}, Count: {Count}", 
                correlationId, responses.Count);

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user payment methods. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Updates a payment method
    /// </summary>
    public async Task<PaymentMethodResponse> UpdatePaymentMethodAsync(UpdatePaymentMethodRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, request.MethodId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .Include(pm => pm.PaymentGateway)
                .FirstOrDefaultAsync(pm => pm.Id == request.MethodId && pm.UserId == request.UserId && pm.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found for update. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, request.MethodId);
                throw new InvalidOperationException($"Payment method with ID {request.MethodId} not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.DisplayName))
                paymentMethod.DisplayName = request.DisplayName;

            if (request.IsActive.HasValue)
                paymentMethod.IsActive = request.IsActive.Value;

            if (request.ExpiryDate.HasValue)
                paymentMethod.ExpiryDate = request.ExpiryDate.Value;

            if (request.Metadata != null)
                paymentMethod.Metadata = JsonSerializer.Serialize(request.Metadata);

            if (request.BillingAddress != null)
                paymentMethod.BillingAddress = JsonSerializer.Serialize(request.BillingAddress);

            // Handle default flag
            if (request.IsDefault.HasValue && request.IsDefault.Value)
            {
                // Remove default flag from other methods
                var otherMethods = await _context.PaymentMethods
                    .Where(pm => pm.UserId == request.UserId && pm.IsActive && pm.Id != request.MethodId)
                    .ToListAsync();

                foreach (var method in otherMethods)
                {
                    method.IsDefault = false;
                    method.UpdatedAt = DateTime.UtcNow;
                }

                paymentMethod.IsDefault = true;
            }
            else if (request.IsDefault.HasValue && !request.IsDefault.Value)
            {
                paymentMethod.IsDefault = false;
            }

            paymentMethod.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(request.MethodId);

            _logger.LogInformation("Payment method updated successfully. CorrelationId: {CorrelationId}", correlationId);

            return await MapToResponseAsync(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a payment method
    /// </summary>
    public async Task<bool> DeletePaymentMethodAsync(Guid methodId, Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Deleting payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}, UserId: {UserId}", 
            correlationId, methodId, userId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId && pm.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found for deletion. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                return false;
            }

            // Check if this method has pending payments
            var hasPendingPayments = await _context.Payments
                .AnyAsync(p => p.PaymentMethodId == methodId && 
                              (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing));

            if (hasPendingPayments)
            {
                _logger.LogWarning("Cannot delete payment method with pending payments. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                throw new InvalidOperationException("Cannot delete payment method with pending payments");
            }

            // Soft delete
            paymentMethod.IsActive = false;
            paymentMethod.UpdatedAt = DateTime.UtcNow;

            // If this was the default method, set another method as default
            if (paymentMethod.IsDefault)
            {
                var nextDefaultMethod = await _context.PaymentMethods
                    .Where(pm => pm.UserId == userId && pm.IsActive && pm.Id != methodId)
                    .OrderByDescending(pm => pm.LastUsedAt)
                    .ThenBy(pm => pm.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextDefaultMethod != null)
                {
                    nextDefaultMethod.IsDefault = true;
                    nextDefaultMethod.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(methodId);

            _logger.LogInformation("Payment method deleted successfully. CorrelationId: {CorrelationId}", correlationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Sets a payment method as the default for a user
    /// </summary>
    public async Task<PaymentMethodResponse> SetDefaultPaymentMethodAsync(Guid methodId, Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Setting default payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}, UserId: {UserId}", 
            correlationId, methodId, userId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .Include(pm => pm.PaymentGateway)
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId && pm.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                throw new InvalidOperationException($"Payment method with ID {methodId} not found");
            }

            if (!paymentMethod.IsVerified)
            {
                _logger.LogWarning("Cannot set unverified payment method as default. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                throw new InvalidOperationException("Cannot set unverified payment method as default");
            }

            // Remove default flag from all other methods
            var otherMethods = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsActive && pm.Id != methodId)
                .ToListAsync();

            foreach (var method in otherMethods)
            {
                method.IsDefault = false;
                method.UpdatedAt = DateTime.UtcNow;
            }

            // Set this method as default
            paymentMethod.IsDefault = true;
            paymentMethod.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear user cache
            await _cacheService.InvalidateUserCacheAsync(userId);

            _logger.LogInformation("Default payment method set successfully. CorrelationId: {CorrelationId}", correlationId);

            return await MapToResponseAsync(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with the gateway
    /// </summary>
    public async Task<PaymentMethodResponse> VerifyPaymentMethodAsync(Guid methodId, Guid userId, Dictionary<string, object> verificationData)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Verifying payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}, UserId: {UserId}", 
            correlationId, methodId, userId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .Include(pm => pm.PaymentGateway)
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId && pm.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found for verification. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                throw new InvalidOperationException($"Payment method with ID {methodId} not found");
            }

            if (paymentMethod.IsVerified)
            {
                _logger.LogInformation("Payment method already verified. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
                    correlationId, methodId);
                return await MapToResponseAsync(paymentMethod);
            }

            // Verify with gateway
            var verificationResult = await _gatewayIntegration.VerifyPaymentMethodAsync(paymentMethod);

            if (verificationResult.IsValid)
            {
                paymentMethod.IsVerified = true;
                paymentMethod.VerifiedAt = DateTime.UtcNow;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                // Update metadata with verification data
                var existingMetadata = string.IsNullOrEmpty(paymentMethod.Metadata) || paymentMethod.Metadata == "{}" 
                    ? new Dictionary<string, object>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(paymentMethod.Metadata) ?? new Dictionary<string, object>();

                // Add verification data
                foreach (var kvp in verificationData)
                {
                    existingMetadata[kvp.Key] = kvp.Value;
                }

                // Add gateway verification metadata
                foreach (var kvp in verificationResult.Metadata)
                {
                    existingMetadata[kvp.Key] = kvp.Value;
                }

                paymentMethod.Metadata = JsonSerializer.Serialize(existingMetadata);

                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemovePaymentAsync(methodId);

                _logger.LogInformation("Payment method verified successfully. CorrelationId: {CorrelationId}", correlationId);
            }
            else
            {
                _logger.LogWarning("Payment method verification failed. CorrelationId: {CorrelationId}, Error: {Error}", 
                    correlationId, verificationResult.ErrorMessage);
                throw new InvalidOperationException($"Payment method verification failed: {verificationResult.ErrorMessage}");
            }

            return await MapToResponseAsync(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the default payment method for a user
    /// </summary>
    public async Task<PaymentMethodResponse?> GetDefaultPaymentMethodAsync(Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting default payment method. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            var defaultMethod = await _context.PaymentMethods
                .AsNoTracking()
                .Include(pm => pm.PaymentGateway)
                .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsActive && pm.IsDefault);

            if (defaultMethod == null)
            {
                _logger.LogInformation("No default payment method found. CorrelationId: {CorrelationId}, UserId: {UserId}", 
                    correlationId, userId);
                return null;
            }

            _logger.LogInformation("Default payment method retrieved successfully. CorrelationId: {CorrelationId}", correlationId);
            return await MapToResponseAsync(defaultMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default payment method. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Updates the last used timestamp for a payment method
    /// </summary>
    public async Task UpdateLastUsedAsync(Guid paymentMethodId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating last used timestamp. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, paymentMethodId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.IsActive);

            if (paymentMethod != null)
            {
                paymentMethod.LastUsedAt = DateTime.UtcNow;
                paymentMethod.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemovePaymentAsync(paymentMethodId);

                _logger.LogInformation("Last used timestamp updated successfully. CorrelationId: {CorrelationId}", correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used timestamp. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw here as this is not critical
        }
    }

    /// <summary>
    /// Maps a PaymentMethod entity to PaymentMethodResponse
    /// </summary>
    private async Task<PaymentMethodResponse> MapToResponseAsync(PaymentMethod paymentMethod)
    {
        PaymentGatewayResponse? gatewayResponse = null;
        
        if (paymentMethod.PaymentGateway != null)
        {
            gatewayResponse = new PaymentGatewayResponse
            {
                Id = paymentMethod.PaymentGateway.Id,
                Name = paymentMethod.PaymentGateway.Name,
                GatewayType = paymentMethod.PaymentGateway.GatewayType,
                IsActive = paymentMethod.PaymentGateway.IsActive,
                IsTestMode = paymentMethod.PaymentGateway.IsTestMode,
                FeePercentage = paymentMethod.PaymentGateway.FeePercentage,
                FixedFee = paymentMethod.PaymentGateway.FixedFee,
                MinimumAmount = paymentMethod.PaymentGateway.MinimumAmount,
                MaximumAmount = paymentMethod.PaymentGateway.MaximumAmount,
                SupportedCurrencies = paymentMethod.PaymentGateway.SupportedCurrencies?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
                SupportedCountries = paymentMethod.PaymentGateway.SupportedCountries?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
                Priority = paymentMethod.PaymentGateway.Priority,
                CreatedAt = paymentMethod.PaymentGateway.CreatedAt,
                UpdatedAt = paymentMethod.PaymentGateway.UpdatedAt
            };
        }

        return new PaymentMethodResponse
        {
            Id = paymentMethod.Id,
            UserId = paymentMethod.UserId,
            PaymentGateway = gatewayResponse,
            MethodType = paymentMethod.MethodType,
            DisplayName = paymentMethod.DisplayName,
            IsActive = paymentMethod.IsActive,
            IsVerified = paymentMethod.IsVerified,
            IsDefault = paymentMethod.IsDefault,
            ExpiryDate = paymentMethod.ExpiryDate,
            Last4Digits = paymentMethod.Last4Digits,
            Brand = paymentMethod.Brand,
            Country = paymentMethod.Country,
            LastUsedAt = paymentMethod.LastUsedAt,
            CreatedAt = paymentMethod.CreatedAt
        };
    }
}
