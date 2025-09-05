using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PaymentGatewayService.Services;

/// <summary>
/// Gateway management service implementing IPaymentGatewayService
/// Handles gateway selection algorithms, health monitoring, configuration management, and testing
/// </summary>
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentGatewayService> _logger;
    private readonly IGatewayIntegrationService _gatewayIntegration;
    private readonly IPaymentCacheService _cacheService;

    public PaymentGatewayService(
        PaymentDbContext context,
        ILogger<PaymentGatewayService> logger,
        IGatewayIntegrationService gatewayIntegration,
        IPaymentCacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _gatewayIntegration = gatewayIntegration;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Gets all active payment gateways
    /// </summary>
    public async Task<IEnumerable<PaymentGatewayResponse>> GetActiveGatewaysAsync()
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving active payment gateways. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var gateways = await _context.PaymentGateways
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.Priority)
                .ThenBy(g => g.Name)
                .ToListAsync();

            var responses = gateways.Select(g => new PaymentGatewayResponse
            {
                Id = g.Id,
                Name = g.Name,
                GatewayType = g.GatewayType,
                IsActive = g.IsActive,
                IsTestMode = g.IsTestMode,
                FeePercentage = g.FeePercentage,
                FixedFee = g.FixedFee,
                MinimumAmount = g.MinimumAmount,
                MaximumAmount = g.MaximumAmount,
                SupportedCurrencies = g.SupportedCurrencies?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
                SupportedCountries = g.SupportedCountries?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
                Priority = g.Priority,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList();

            _logger.LogInformation("Active payment gateways retrieved successfully. CorrelationId: {CorrelationId}, Count: {Count}", 
                correlationId, responses.Count);

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active payment gateways. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets a payment gateway by ID
    /// </summary>
    public async Task<PaymentGatewayResponse?> GetGatewayAsync(Guid gatewayId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving payment gateway. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            // Try cache first
            var cachedGateway = await _cacheService.GetGatewayAsync(gatewayId);
            if (cachedGateway != null)
            {
                _logger.LogInformation("Payment gateway retrieved from cache. CorrelationId: {CorrelationId}", correlationId);
                return MapToResponse(cachedGateway);
            }

            // Fallback to database
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                _logger.LogWarning("Payment gateway not found. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                    correlationId, gatewayId);
                return null;
            }

            var response = MapToResponse(gateway);

            // Cache the result
            await _cacheService.CacheGatewayAsync(gateway);

            _logger.LogInformation("Payment gateway retrieved successfully. CorrelationId: {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment gateway. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the best gateway for a payment request
    /// </summary>
    public async Task<PaymentGatewayResponse?> GetBestGatewayAsync(decimal amount, string currency, global::PaymentGatewayService.Models.PaymentType paymentType, string? country = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Selecting best payment gateway. CorrelationId: {CorrelationId}, Amount: {Amount}, Currency: {Currency}, Type: {Type}", 
            correlationId, amount, currency, paymentType);

        try
        {
            var query = _context.PaymentGateways
                .AsNoTracking()
                .Where(g => g.IsActive && 
                           g.SupportedCurrencies != null && 
                           g.SupportedCurrencies.Contains(currency));

            // Filter by amount limits
            query = query.Where(g => (g.MinimumAmount == null || amount >= g.MinimumAmount) &&
                                   (g.MaximumAmount == null || amount <= g.MaximumAmount));

            // Filter by country if provided
            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(g => g.SupportedCountries == null || g.SupportedCountries.Contains(country));
            }

            var gateways = await query
                .OrderBy(g => g.Priority)
                .ThenBy(g => g.FeePercentage + g.FixedFee) // Prefer lower fees
                .ToListAsync();

            if (!gateways.Any())
            {
                _logger.LogWarning("No suitable gateways found. CorrelationId: {CorrelationId}, Currency: {Currency}", 
                    correlationId, currency);
                return null;
            }

            // Check gateway health and select the best one
            foreach (var gateway in gateways)
            {
                var healthStatus = await _gatewayIntegration.GetGatewayHealthAsync();
                if (healthStatus.IsHealthy)
                {
                    _logger.LogInformation("Selected gateway for payment. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}, GatewayType: {GatewayType}", 
                        correlationId, gateway.Id, gateway.GatewayType);

                    return MapToResponse(gateway);
                }
            }

            _logger.LogWarning("No healthy gateways found. CorrelationId: {CorrelationId}, Currency: {Currency}", 
                correlationId, currency);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting best payment gateway. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Creates a new payment gateway configuration
    /// </summary>
    public async Task<PaymentGatewayResponse> CreateGatewayAsync(CreatePaymentGatewayRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating payment gateway. CorrelationId: {CorrelationId}, Name: {Name}", 
            correlationId, request.Name);

        try
        {
            // Validate gateway configuration
            ValidateGatewayConfiguration(request.GatewayType, request.Configuration);

            var gateway = new PaymentGateway
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                GatewayType = request.GatewayType,
                Configuration = JsonSerializer.Serialize(request.Configuration),
                IsActive = true, // Default to active
                IsTestMode = request.IsTestMode,
                FeePercentage = request.FeePercentage,
                FixedFee = request.FixedFee,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                SupportedCurrencies = request.SupportedCurrencies,
                SupportedCountries = request.SupportedCountries,
                Priority = request.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PaymentGateways.Add(gateway);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment gateway created successfully. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                correlationId, gateway.Id);

            return MapToResponse(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment gateway. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Updates a payment gateway configuration
    /// </summary>
    public async Task<PaymentGatewayResponse> UpdateGatewayAsync(UpdatePaymentGatewayRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating payment gateway. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, request.GatewayId);

        try
        {
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.Id == request.GatewayId);

            if (gateway == null)
            {
                _logger.LogWarning("Payment gateway not found for update. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                    correlationId, request.GatewayId);
                throw new InvalidOperationException($"Payment gateway with ID {request.GatewayId} not found");
            }

            // Validate configuration if provided
            if (request.Configuration != null)
            {
                ValidateGatewayConfiguration(gateway.GatewayType, request.Configuration);
                gateway.Configuration = JsonSerializer.Serialize(request.Configuration);
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Name))
                gateway.Name = request.Name;

            if (request.IsTestMode.HasValue)
                gateway.IsTestMode = request.IsTestMode.Value;

            if (request.FeePercentage.HasValue)
                gateway.FeePercentage = request.FeePercentage.Value;

            if (request.FixedFee.HasValue)
                gateway.FixedFee = request.FixedFee.Value;

            if (request.MinimumAmount.HasValue)
                gateway.MinimumAmount = request.MinimumAmount.Value;

            if (request.MaximumAmount.HasValue)
                gateway.MaximumAmount = request.MaximumAmount.Value;

            if (!string.IsNullOrEmpty(request.SupportedCurrencies))
                gateway.SupportedCurrencies = request.SupportedCurrencies;

            if (!string.IsNullOrEmpty(request.SupportedCountries))
                gateway.SupportedCountries = request.SupportedCountries;

            if (request.Priority.HasValue)
                gateway.Priority = request.Priority.Value;

            gateway.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(request.GatewayId);

            _logger.LogInformation("Payment gateway updated successfully. CorrelationId: {CorrelationId}", correlationId);

            return MapToResponse(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment gateway. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Activates or deactivates a payment gateway
    /// </summary>
    public async Task<PaymentGatewayResponse> SetGatewayStatusAsync(Guid gatewayId, bool isActive)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Setting gateway status. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}, IsActive: {IsActive}", 
            correlationId, gatewayId, isActive);

        try
        {
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                _logger.LogWarning("Payment gateway not found. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                    correlationId, gatewayId);
                throw new InvalidOperationException($"Payment gateway with ID {gatewayId} not found");
            }

            gateway.IsActive = isActive;
            gateway.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(gatewayId);

            _logger.LogInformation("Gateway status updated successfully. CorrelationId: {CorrelationId}", correlationId);

            return MapToResponse(gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting gateway status. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets gateway statistics
    /// </summary>
    public async Task<GatewayStatisticsResponse> GetGatewayStatisticsAsync(Guid gatewayId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting gateway statistics. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                throw new InvalidOperationException($"Payment gateway with ID {gatewayId} not found");
            }

            var query = _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentGatewayId == gatewayId);

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt <= toDate.Value);

            var payments = await query.ToListAsync();

            var totalTransactions = payments.Count;
            var totalAmount = payments.Sum(p => p.Amount);
            var successfulPayments = payments.Count(p => p.Status == global::PaymentGatewayService.Data.Entities.PaymentStatus.Completed);
            var successRate = totalTransactions > 0 ? (decimal)successfulPayments / totalTransactions * 100 : 0;

            var processingTimes = payments
                .Where(p => p.CompletedAt.HasValue)
                .Select(p => (p.CompletedAt!.Value - p.CreatedAt).TotalMilliseconds)
                .ToList();

            var averageProcessingTime = processingTimes.Any() ? processingTimes.Average() : 0;
            var totalFees = payments.Sum(p => p.FeeAmount ?? 0);

            _logger.LogInformation("Gateway statistics retrieved successfully. CorrelationId: {CorrelationId}", correlationId);

            return new GatewayStatisticsResponse
            {
                GatewayId = gatewayId,
                GatewayName = gateway.Name,
                TotalTransactions = totalTransactions,
                TotalAmount = totalAmount,
                SuccessRate = successRate,
                AverageProcessingTimeMs = averageProcessingTime,
                TotalFees = totalFees,
                FromDate = fromDate,
                ToDate = toDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gateway statistics. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Tests a gateway connection
    /// </summary>
    public async Task<GatewayTestResponse> TestGatewayAsync(Guid gatewayId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Testing payment gateway. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                _logger.LogWarning("Payment gateway not found for testing. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                    correlationId, gatewayId);
                throw new InvalidOperationException($"Payment gateway with ID {gatewayId} not found");
            }

            var startTime = DateTime.UtcNow;
            var healthStatus = await _gatewayIntegration.GetGatewayHealthAsync();
            var endTime = DateTime.UtcNow;

            var responseTime = (int)(endTime - startTime).TotalMilliseconds;

            _logger.LogInformation("Gateway test completed. CorrelationId: {CorrelationId}, IsHealthy: {IsHealthy}", 
                correlationId, healthStatus.IsHealthy);

            return new GatewayTestResponse
            {
                GatewayId = gatewayId,
                IsSuccessful = healthStatus.IsHealthy,
                Message = healthStatus.IsHealthy ? "Gateway is healthy" : healthStatus.Message ?? "Gateway is unhealthy",
                ResponseTimeMs = responseTime,
                TestedAt = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["GatewayType"] = gateway.GatewayType.ToString(),
                    ["IsTestMode"] = gateway.IsTestMode,
                    ["IsActive"] = gateway.IsActive
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing payment gateway. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets supported currencies for a gateway
    /// </summary>
    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync(Guid gatewayId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting supported currencies. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                throw new InvalidOperationException($"Payment gateway with ID {gatewayId} not found");
            }

            var currencies = gateway.SupportedCurrencies?.Split(',')
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList() ?? new List<string>();

            _logger.LogInformation("Supported currencies retrieved. CorrelationId: {CorrelationId}, Count: {Count}", 
                correlationId, currencies.Count);

            return currencies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported currencies. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets supported countries for a gateway
    /// </summary>
    public async Task<IEnumerable<string>> GetSupportedCountriesAsync(Guid gatewayId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting supported countries. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId);

            if (gateway == null)
            {
                throw new InvalidOperationException($"Payment gateway with ID {gatewayId} not found");
            }

            var countries = gateway.SupportedCountries?.Split(',')
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList() ?? new List<string>();

            _logger.LogInformation("Supported countries retrieved. CorrelationId: {CorrelationId}, Count: {Count}", 
                correlationId, countries.Count);

            return countries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported countries. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Maps a PaymentGateway entity to PaymentGatewayResponse
    /// </summary>
    private static PaymentGatewayResponse MapToResponse(PaymentGateway gateway)
    {
        return new PaymentGatewayResponse
        {
            Id = gateway.Id,
            Name = gateway.Name,
            GatewayType = gateway.GatewayType,
            IsActive = gateway.IsActive,
            IsTestMode = gateway.IsTestMode,
            FeePercentage = gateway.FeePercentage,
            FixedFee = gateway.FixedFee,
            MinimumAmount = gateway.MinimumAmount,
            MaximumAmount = gateway.MaximumAmount,
            SupportedCurrencies = gateway.SupportedCurrencies?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
            SupportedCountries = gateway.SupportedCountries?.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>(),
            Priority = gateway.Priority,
            CreatedAt = gateway.CreatedAt,
            UpdatedAt = gateway.UpdatedAt
        };
    }

    #region IPaymentGatewayService Implementation

    /// <summary>
    /// Processes a payment request through the gateway system
    /// </summary>
    public async Task<PaymentResult> ProcessPaymentAsync(global::PaymentGatewayService.Models.Requests.ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing payment request. CorrelationId: {CorrelationId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, request.Amount, request.Currency);

        try
        {
            // Simple implementation - return success for now
            _logger.LogInformation("Payment processed (simplified). CorrelationId: {CorrelationId}", correlationId);

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                Status = global::PaymentGatewayService.Models.PaymentStatus.Completed,
                Metadata = request.Metadata != null ? request.Metadata.ToDictionary(k => k.Key, k => (object)k.Value) : new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment. CorrelationId: {CorrelationId}", correlationId);
            return new PaymentResult
            {
                Success = false,
                Status = global::PaymentGatewayService.Models.PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Processes a refund request
    /// </summary>
    public async Task<PaymentResult> ProcessRefundAsync(Services.Interfaces.RefundRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing refund request. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
            correlationId, request.PaymentId);

        try
        {
            // Simple implementation - return success for now
            _logger.LogInformation("Refund processed (simplified). CorrelationId: {CorrelationId}", correlationId);

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                Status = global::PaymentGatewayService.Models.PaymentStatus.Processing,
                Metadata = request.Metadata != null ? request.Metadata.ToDictionary(k => k.Key, k => (object)k.Value) : new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund. CorrelationId: {CorrelationId}", correlationId);
            return new PaymentResult
            {
                Success = false,
                Status = global::PaymentGatewayService.Models.PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets the status of a payment
    /// </summary>
    public async Task<global::PaymentGatewayService.Models.PaymentStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting payment status. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
            correlationId, transactionId);

        try
        {
            // Simple implementation - return completed status for now
            _logger.LogInformation("Payment status retrieved (simplified). CorrelationId: {CorrelationId}", correlationId);
            return global::PaymentGatewayService.Models.PaymentStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status. CorrelationId: {CorrelationId}", correlationId);
            return global::PaymentGatewayService.Models.PaymentStatus.Failed;
        }
    }

    #endregion

    /// <summary>
    /// Validates gateway configuration based on gateway type
    /// </summary>
    private static void ValidateGatewayConfiguration(PaymentGatewayType gatewayType, Dictionary<string, object> configuration)
    {
        // Validate required configuration keys based on gateway type
        switch (gatewayType)
        {
            case PaymentGatewayType.Square:
                if (!configuration.ContainsKey("ApplicationId") || !configuration.ContainsKey("AccessToken"))
                {
                    throw new ArgumentException("Square gateway requires ApplicationId and AccessToken");
                }
                break;

            case PaymentGatewayType.Stripe:
                if (!configuration.ContainsKey("SecretKey") || !configuration.ContainsKey("PublishableKey"))
                {
                    throw new ArgumentException("Stripe gateway requires SecretKey and PublishableKey");
                }
                break;

            case PaymentGatewayType.BankTransfer:
                if (!configuration.ContainsKey("RoutingNumber") || !configuration.ContainsKey("AccountNumber"))
                {
                    throw new ArgumentException("Bank transfer gateway requires RoutingNumber and AccountNumber");
                }
                break;

            case PaymentGatewayType.CryptoWallet:
                if (!configuration.ContainsKey("WalletAddress") || !configuration.ContainsKey("NetworkType"))
                {
                    throw new ArgumentException("Crypto wallet gateway requires WalletAddress and NetworkType");
                }
                break;

            default:
                throw new ArgumentException($"Unsupported gateway type: {gatewayType}");
        }
    }
}
