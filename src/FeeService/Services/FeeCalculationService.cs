using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using FeeService.Data;
using FeeService.Data.Entities;
using FeeService.Models.Requests;
using FeeService.Models.Responses;
using FeeService.Services.Interfaces;
using Mapster;

namespace FeeService.Services;

public class FeeCalculationService : IFeeCalculationService
{
    private readonly FeeDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<FeeCalculationService> _logger;
    private readonly IConfiguration _configuration;

    private readonly TimeSpan _configCacheExpiry;
    private readonly string _defaultCurrency;

    public FeeCalculationService(
        FeeDbContext context,
        IDistributedCache cache,
        IExchangeRateService exchangeRateService,
        ILogger<FeeCalculationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _exchangeRateService = exchangeRateService;
        _logger = logger;
        _configuration = configuration;
        _configCacheExpiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:FeeConfigurationCacheDurationMinutes", 30));
        _defaultCurrency = _configuration.GetValue<string>("FeeService:DefaultCurrency", "USD") ?? "USD";
    }

    public async Task<FeeCalculationResponse> CalculateFeeAsync(CalculateFeeRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating fee for user {UserId}, type {FeeType}, amount {Amount} {Currency}", 
                request.UserId, request.FeeType, request.Amount, request.Currency);

            // Get fee configuration
            var feeConfig = await GetFeeConfigurationAsync(request.FeeType, request.EntityType, request.EntityId);
            if (feeConfig == null)
            {
                throw new InvalidOperationException($"No fee configuration found for fee type: {request.FeeType}");
            }

            // Calculate base fee
            var calculatedFee = await CalculateBaseFeeAsync(feeConfig, request.Amount, request.Currency);

            // Apply discounts
            var discountAmount = await ApplyDiscountsAsync(request.UserId, request.FeeType, calculatedFee);
            var finalFeeAmount = Math.Max(0, calculatedFee - discountAmount);

            // Handle currency conversion if needed
            decimal? exchangeRate = null;
            if (!request.Currency.Equals(feeConfig.Currency, StringComparison.OrdinalIgnoreCase))
            {
                var conversionResponse = await _exchangeRateService.ConvertCurrencyAsync(new ConvertCurrencyRequest
                {
                    FromCurrency = feeConfig.Currency,
                    ToCurrency = request.Currency,
                    Amount = finalFeeAmount
                });
                finalFeeAmount = conversionResponse.ConvertedAmount;
                exchangeRate = conversionResponse.ExchangeRate;
            }

            // Create calculation result
            var calculationResult = new FeeCalculationResult
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FeeType = request.FeeType,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                BaseAmount = request.Amount,
                BaseCurrency = request.Currency,
                CalculatedFee = calculatedFee,
                FeeCurrency = feeConfig.Currency,
                DiscountAmount = discountAmount,
                DiscountPercentage = discountAmount > 0 ? (discountAmount / calculatedFee) * 100 : null,
                FinalFeeAmount = finalFeeAmount,
                UsedExchangeRate = exchangeRate,
                FeeConfigurationId = Guid.Parse(feeConfig.Id.ToString()),
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    CalculationType = feeConfig.CalculationType,
                    AppliedRate = GetAppliedRate(feeConfig, request.Amount),
                    MinFeeApplied = finalFeeAmount == feeConfig.MinimumFee,
                    MaxFeeApplied = finalFeeAmount == feeConfig.MaximumFee,
                    AdditionalParameters = request.AdditionalParameters
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FeeCalculationResults.Add(calculationResult);
            await _context.SaveChangesAsync();

            var response = calculationResult.Adapt<FeeCalculationResponse>();
            response.FeeConfiguration = feeConfig;

            _logger.LogInformation("Fee calculation completed for user {UserId}: {FinalAmount} {Currency}", 
                request.UserId, finalFeeAmount, request.Currency);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fee for user {UserId}, type {FeeType}", request.UserId, request.FeeType);
            throw;
        }
    }

    public async Task<FeeEstimationResponse> EstimateFeeAsync(EstimateFeeRequest request)
    {
        try
        {
            _logger.LogDebug("Estimating fee for type {FeeType}, amount {Amount} {Currency}", 
                request.FeeType, request.Amount, request.Currency);

            // Get fee configuration
            var feeConfig = await GetFeeConfigurationAsync(request.FeeType, request.EntityType, request.EntityId);
            if (feeConfig == null)
            {
                throw new InvalidOperationException($"No fee configuration found for fee type: {request.FeeType}");
            }

            // Calculate base fee
            var estimatedFee = await CalculateBaseFeeAsync(feeConfig, request.Amount, request.Currency);

            // Estimate potential discount if user ID provided
            decimal? potentialDiscount = null;
            decimal? potentialDiscountPercentage = null;
            string? discountReason = null;

            if (request.UserId.HasValue)
            {
                potentialDiscount = await ApplyDiscountsAsync(request.UserId.Value, request.FeeType, estimatedFee);
                if (potentialDiscount > 0)
                {
                    potentialDiscountPercentage = (potentialDiscount / estimatedFee) * 100;
                    discountReason = "User-specific discount applied";
                }
            }

            var finalEstimatedFee = Math.Max(0, estimatedFee - (potentialDiscount ?? 0));

            // Handle currency conversion if needed
            decimal? exchangeRate = null;
            if (!request.Currency.Equals(feeConfig.Currency, StringComparison.OrdinalIgnoreCase))
            {
                var conversionResponse = await _exchangeRateService.ConvertCurrencyAsync(new ConvertCurrencyRequest
                {
                    FromCurrency = feeConfig.Currency,
                    ToCurrency = request.Currency,
                    Amount = finalEstimatedFee
                });
                finalEstimatedFee = conversionResponse.ConvertedAmount;
                exchangeRate = conversionResponse.ExchangeRate;
            }

            return new FeeEstimationResponse
            {
                FeeType = request.FeeType,
                BaseAmount = request.Amount,
                BaseCurrency = request.Currency,
                EstimatedFee = estimatedFee,
                FeeCurrency = feeConfig.Currency,
                PotentialDiscount = potentialDiscount,
                PotentialDiscountPercentage = potentialDiscountPercentage,
                DiscountReason = discountReason,
                FinalEstimatedFee = finalEstimatedFee,
                ExchangeRate = exchangeRate,
                EstimatedAt = DateTime.UtcNow,
                AppliedConfiguration = feeConfig,
                IsExactCalculation = request.UserId.HasValue,
                EstimationDetails = new
                {
                    CalculationType = feeConfig.CalculationType,
                    AppliedRate = GetAppliedRate(feeConfig, request.Amount),
                    MinFeeWouldApply = finalEstimatedFee == feeConfig.MinimumFee,
                    MaxFeeWouldApply = finalEstimatedFee == feeConfig.MaximumFee
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating fee for type {FeeType}", request.FeeType);
            throw;
        }
    }

    public async Task<FeeConfigurationResponse> GetFeeConfigurationAsync(string feeType, string? entityType = null, string? entityId = null)
    {
        try
        {
            // Try cache first
            var cacheKey = $"fee_config:{feeType}:{entityType ?? "default"}:{entityId ?? "default"}";
            var cachedConfig = await GetFromCacheAsync<FeeConfigurationResponse>(cacheKey);
            if (cachedConfig != null)
            {
                _logger.LogDebug("Retrieved fee configuration from cache for {FeeType}", feeType);
                return cachedConfig;
            }

            // Query database with priority: specific entity > entity type > general
            var query = _context.FeeConfigurations
                .Where(fc => fc.FeeType == feeType && 
                            fc.IsActive && 
                            fc.EffectiveFrom <= DateTime.UtcNow &&
                            (fc.EffectiveUntil == null || fc.EffectiveUntil > DateTime.UtcNow));

            FeeConfiguration? feeConfig = null;

            // Try specific entity first
            if (!string.IsNullOrEmpty(entityType) && !string.IsNullOrEmpty(entityId))
            {
                feeConfig = await query
                    .Where(fc => fc.EntityType == entityType && fc.EntityId == entityId)
                    .OrderByDescending(fc => fc.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            // Try entity type if no specific entity config found
            if (feeConfig == null && !string.IsNullOrEmpty(entityType))
            {
                feeConfig = await query
                    .Where(fc => fc.EntityType == entityType && fc.EntityId == null)
                    .OrderByDescending(fc => fc.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            // Try general config if no entity-specific config found
            if (feeConfig == null)
            {
                feeConfig = await query
                    .Where(fc => fc.EntityType == "General" || fc.EntityType == "Default")
                    .OrderByDescending(fc => fc.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            if (feeConfig == null)
            {
                _logger.LogWarning("No fee configuration found for {FeeType}", feeType);
                return null;
            }

            var response = feeConfig.Adapt<FeeConfigurationResponse>();
            
            // Cache the result
            await SetCacheAsync(cacheKey, response, _configCacheExpiry);

            _logger.LogDebug("Retrieved fee configuration from database for {FeeType}", feeType);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fee configuration for {FeeType}", feeType);
            throw;
        }
    }

    public async Task<FeeConfigurationResponse> CreateOrUpdateFeeConfigurationAsync(CreateFeeConfigurationRequest request)
    {
        try
        {
            _logger.LogInformation("Creating/updating fee configuration for {FeeType}", request.FeeType);

            // Validate configuration
            await ValidateFeeConfigurationAsync(request);

            // Check if configuration already exists
            var existingConfig = await _context.FeeConfigurations
                .Where(fc => fc.FeeType == request.FeeType && 
                            fc.EntityType == request.EntityType &&
                            fc.EntityId == request.EntityId &&
                            fc.IsActive)
                .FirstOrDefaultAsync();

            FeeConfiguration feeConfig;

            if (existingConfig != null)
            {
                // Update existing configuration
                existingConfig.CalculationType = request.CalculationType;
                existingConfig.FlatFeeAmount = request.FlatFeeAmount;
                existingConfig.PercentageRate = request.PercentageRate;
                existingConfig.MinimumFee = request.MinimumFee;
                existingConfig.MaximumFee = request.MaximumFee;
                existingConfig.Currency = request.Currency;
                existingConfig.EffectiveFrom = request.EffectiveFrom;
                existingConfig.EffectiveUntil = request.EffectiveUntil;
                existingConfig.Description = request.Description;
                existingConfig.TieredStructure = request.TieredStructure != null ? JsonSerializer.Serialize(request.TieredStructure) : null;
                existingConfig.DiscountRules = request.DiscountRules != null ? JsonSerializer.Serialize(request.DiscountRules) : null;
                existingConfig.UpdatedBy = request.CreatedBy;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                feeConfig = existingConfig;
            }
            else
            {
                // Create new configuration
                feeConfig = new FeeConfiguration
                {
                    Id = Guid.NewGuid(),
                    FeeType = request.FeeType,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    CalculationType = request.CalculationType,
                    FlatFeeAmount = request.FlatFeeAmount,
                    PercentageRate = request.PercentageRate,
                    MinimumFee = request.MinimumFee,
                    MaximumFee = request.MaximumFee,
                    Currency = request.Currency,
                    IsActive = true,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveUntil = request.EffectiveUntil,
                    Description = request.Description,
                    TieredStructure = request.TieredStructure != null ? JsonSerializer.Serialize(request.TieredStructure) : null,
                    DiscountRules = request.DiscountRules != null ? JsonSerializer.Serialize(request.DiscountRules) : null,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.FeeConfigurations.Add(feeConfig);
            }

            await _context.SaveChangesAsync();

            // Clear cache
            var cacheKey = $"fee_config:{request.FeeType}:{request.EntityType ?? "default"}:{request.EntityId ?? "default"}";
            await _cache.RemoveAsync(cacheKey);

            var response = feeConfig.Adapt<FeeConfigurationResponse>();
            _logger.LogInformation("Fee configuration created/updated for {FeeType} with ID {Id}", request.FeeType, feeConfig.Id);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating fee configuration for {FeeType}", request.FeeType);
            throw;
        }
    }

    public async Task<IEnumerable<FeeConfigurationResponse>> GetActiveFeeConfigurationsAsync()
    {
        try
        {
            var configurations = await _context.FeeConfigurations
                .Where(fc => fc.IsActive && 
                            fc.EffectiveFrom <= DateTime.UtcNow &&
                            (fc.EffectiveUntil == null || fc.EffectiveUntil > DateTime.UtcNow))
                .OrderBy(fc => fc.FeeType)
                .ThenBy(fc => fc.EntityType)
                .ToListAsync();

            return configurations.Adapt<IEnumerable<FeeConfigurationResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active fee configurations");
            throw;
        }
    }

    public async Task<bool> ValidateFeeParametersAsync(string feeType, decimal amount, string currency)
    {
        try
        {
            if (amount <= 0)
            {
                return false;
            }

            if (!_exchangeRateService.IsValidCurrency(currency))
            {
                return false;
            }

            var config = await GetFeeConfigurationAsync(feeType);
            return config != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating fee parameters for {FeeType}", feeType);
            return false;
        }
    }

    public async Task<decimal> ApplyDiscountsAsync(Guid userId, string feeType, decimal calculatedFee)
    {
        try
        {
            // This is a simplified discount calculation
            // In a real implementation, you would have complex discount rules
            
            // Example: Volume-based discount
            var userTransactionVolume = await GetUserTransactionVolumeAsync(userId);
            decimal discountPercentage = 0;

            if (userTransactionVolume > 1000000) // $1M+
            {
                discountPercentage = 0.15m; // 15% discount
            }
            else if (userTransactionVolume > 500000) // $500K+
            {
                discountPercentage = 0.10m; // 10% discount
            }
            else if (userTransactionVolume > 100000) // $100K+
            {
                discountPercentage = 0.05m; // 5% discount
            }

            var discountAmount = calculatedFee * discountPercentage;
            
            if (discountAmount > 0)
            {
                _logger.LogDebug("Applied {DiscountPercentage}% discount ({DiscountAmount}) for user {UserId}", 
                    discountPercentage * 100, discountAmount, userId);
            }

            return discountAmount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discounts for user {UserId}", userId);
            return 0; // No discount on error
        }
    }

    public async Task<IEnumerable<FeeCalculationResponse>> GetCalculationHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            
            var calculations = await _context.FeeCalculationResults
                .Where(fcr => fcr.UserId == userId)
                .OrderByDescending(fcr => fcr.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(fcr => fcr.FeeConfiguration)
                .ToListAsync();

            return calculations.Adapt<IEnumerable<FeeCalculationResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calculation history for user {UserId}", userId);
            throw;
        }
    }

    private async Task<decimal> CalculateBaseFeeAsync(FeeConfigurationResponse config, decimal amount, string currency)
    {
        decimal calculatedFee = 0;

        switch (config.CalculationType.ToLower())
        {
            case "flat":
                calculatedFee = config.FlatFeeAmount ?? 0;
                break;

            case "percentage":
                calculatedFee = amount * (config.PercentageRate ?? 0) / 100;
                break;

            case "tiered":
                calculatedFee = await CalculateTieredFeeAsync(config, amount);
                break;

            default:
                throw new InvalidOperationException($"Unknown calculation type: {config.CalculationType}");
        }

        // Apply min/max limits
        if (config.MinimumFee.HasValue && calculatedFee < config.MinimumFee.Value)
        {
            calculatedFee = config.MinimumFee.Value;
        }

        if (config.MaximumFee.HasValue && calculatedFee > config.MaximumFee.Value)
        {
            calculatedFee = config.MaximumFee.Value;
        }

        return calculatedFee;
    }

    private async Task<decimal> CalculateTieredFeeAsync(FeeConfigurationResponse config, decimal amount)
    {
        // This is a simplified tiered calculation
        // In a real implementation, you would parse the TieredStructure JSON
        
        if (amount <= 1000)
        {
            return amount * 0.01m; // 1%
        }
        else if (amount <= 10000)
        {
            return 10 + (amount - 1000) * 0.005m; // $10 + 0.5%
        }
        else
        {
            return 55 + (amount - 10000) * 0.002m; // $55 + 0.2%
        }
    }

    private decimal GetAppliedRate(FeeConfigurationResponse config, decimal amount)
    {
        return config.CalculationType.ToLower() switch
        {
            "flat" => config.FlatFeeAmount ?? 0,
            "percentage" => config.PercentageRate ?? 0,
            "tiered" => CalculateTieredRate(amount),
            _ => 0
        };
    }

    private decimal CalculateTieredRate(decimal amount)
    {
        // Simplified tiered rate calculation
        if (amount <= 1000) return 1.0m;
        if (amount <= 10000) return 0.5m;
        return 0.2m;
    }

    private async Task<decimal> GetUserTransactionVolumeAsync(Guid userId)
    {
        try
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var volume = await _context.FeeCalculationResults
                .Where(fcr => fcr.UserId == userId && fcr.CreatedAt >= thirtyDaysAgo)
                .SumAsync(fcr => fcr.BaseAmount);

            return volume;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user transaction volume for {UserId}", userId);
            return 0;
        }
    }

    private async Task ValidateFeeConfigurationAsync(CreateFeeConfigurationRequest request)
    {
        if (request.CalculationType.ToLower() == "flat" && !request.FlatFeeAmount.HasValue)
        {
            throw new ArgumentException("Flat fee amount is required for flat calculation type");
        }

        if (request.CalculationType.ToLower() == "percentage" && !request.PercentageRate.HasValue)
        {
            throw new ArgumentException("Percentage rate is required for percentage calculation type");
        }

        if (request.MinimumFee.HasValue && request.MaximumFee.HasValue && request.MinimumFee > request.MaximumFee)
        {
            throw new ArgumentException("Minimum fee cannot be greater than maximum fee");
        }

        if (!_exchangeRateService.IsValidCurrency(request.Currency))
        {
            throw new ArgumentException($"Invalid currency: {request.Currency}");
        }
    }

    private async Task<T?> GetFromCacheAsync<T>(string key) where T : class
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (cachedValue != null)
            {
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving from cache with key: {Key}", key);
        }
        return null;
    }

    private async Task SetCacheAsync<T>(string key, T value, TimeSpan expiry)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };
            await _cache.SetStringAsync(key, serializedValue, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache with key: {Key}", key);
        }
    }

    /// <summary>
    /// Calculates fiat rejection fees including wire fees, Square fees, and internal fees
    /// </summary>
    public async Task<FiatRejectionFeesResponse> CalculateFiatRejectionFeesAsync(FiatRejectionFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating fiat rejection fees for amount {Amount} {Currency}, gateway {GatewayType}", 
                request.Amount, request.Currency, request.GatewayType);

            decimal wireFees = 0;
            decimal squareFees = 0;
            decimal internalFees = 0;

            // Calculate wire fees (fixed cost for returning funds via wire)
            wireFees = await CalculateWireFeesAsync(request.Amount, request.Currency, request.PaymentMethodType);

            // Calculate Square fees (if applicable)
            if (request.GatewayType.Equals("Square", StringComparison.OrdinalIgnoreCase))
            {
                squareFees = await CalculateSquareRejectionFeesAsync(request.Amount, request.Currency);
            }

            // Calculate internal processing fees
            internalFees = await CalculateInternalRejectionFeesAsync(request.Amount, request.Currency, "fiat");

            var totalFees = wireFees + squareFees + internalFees;
            var netAmount = Math.Max(0, request.Amount - totalFees);

            var response = new FiatRejectionFeesResponse
            {
                WireFees = wireFees,
                SquareFees = squareFees,
                InternalFees = internalFees,
                TotalFees = totalFees,
                NetAmount = netAmount,
                Currency = request.Currency
            };

            _logger.LogInformation("Fiat rejection fees calculated: Wire={WireFees}, Square={SquareFees}, Internal={InternalFees}, Total={TotalFees}, Net={NetAmount}", 
                wireFees, squareFees, internalFees, totalFees, netAmount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fiat rejection fees for amount {Amount} {Currency}", request.Amount, request.Currency);
            throw;
        }
    }

    /// <summary>
    /// Calculates crypto rejection fees including network fees and internal fees
    /// </summary>
    public async Task<CryptoRejectionFeesResponse> CalculateCryptoRejectionFeesAsync(CryptoRejectionFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating crypto rejection fees for amount {Amount} {Cryptocurrency}, network {Network}", 
                request.Amount, request.Cryptocurrency, request.Network);

            // Calculate network fees based on cryptocurrency and network
            var networkFees = await CalculateNetworkFeesAsync(request.Amount, request.Cryptocurrency, request.Network);

            // Calculate internal processing fees
            var internalFees = await CalculateInternalRejectionFeesAsync(request.Amount, request.Cryptocurrency, "crypto");

            var totalFees = networkFees + internalFees;
            var netAmount = Math.Max(0, request.Amount - totalFees);

            var response = new CryptoRejectionFeesResponse
            {
                NetworkFees = networkFees,
                InternalFees = internalFees,
                TotalFees = totalFees,
                NetAmount = netAmount,
                Cryptocurrency = request.Cryptocurrency,
                Network = request.Network
            };

            _logger.LogInformation("Crypto rejection fees calculated: Network={NetworkFees}, Internal={InternalFees}, Total={TotalFees}, Net={NetAmount}", 
                networkFees, internalFees, totalFees, netAmount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating crypto rejection fees for amount {Amount} {Cryptocurrency}", request.Amount, request.Cryptocurrency);
            throw;
        }
    }

    /// <summary>
    /// Calculates wire transfer fees for returning fiat deposits
    /// </summary>
    private async Task<decimal> CalculateWireFeesAsync(decimal amount, string currency, string paymentMethodType)
    {
        try
        {
            // Get wire fee configuration
            var wireFeeConfig = await GetFeeConfigurationAsync("WireTransfer", "BankTransfer", null);
            
            if (wireFeeConfig != null)
            {
                return await CalculateBaseFeeAsync(wireFeeConfig, amount, currency);
            }

            // Default wire fees based on currency and region
            return currency.ToUpperInvariant() switch
            {
                "USD" => 25.00m,    // $25 for US domestic wires
                "EUR" => 30.00m,    // €30 for European wires
                "GBP" => 25.00m,    // £25 for UK wires
                "CAD" => 30.00m,    // $30 CAD for Canadian wires
                "AUD" => 35.00m,    // $35 AUD for Australian wires
                _ => 45.00m         // $45 for international wires
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating wire fees, using default");
            return 25.00m; // Default fallback
        }
    }

    /// <summary>
    /// Calculates Square processing fees for rejection
    /// </summary>
    private async Task<decimal> CalculateSquareRejectionFeesAsync(decimal amount, string currency)
    {
        try
        {
            // Get Square fee configuration
            var squareFeeConfig = await GetFeeConfigurationAsync("SquareProcessing", "Square", null);
            
            if (squareFeeConfig != null)
            {
                return await CalculateBaseFeeAsync(squareFeeConfig, amount, currency);
            }

            // Default Square fees: 2.9% + $0.30
            return (amount * 0.029m) + 0.30m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating Square fees, using default");
            return (amount * 0.029m) + 0.30m;
        }
    }

    /// <summary>
    /// Calculates network fees for cryptocurrency returns
    /// </summary>
    private async Task<decimal> CalculateNetworkFeesAsync(decimal amount, string cryptocurrency, string network)
    {
        try
        {
            // Get network fee configuration
            var networkFeeConfig = await GetFeeConfigurationAsync($"NetworkFee_{cryptocurrency}", "Crypto", network);
            
            if (networkFeeConfig != null)
            {
                return await CalculateBaseFeeAsync(networkFeeConfig, amount, cryptocurrency);
            }

            // Default network fees based on cryptocurrency
            return cryptocurrency.ToUpperInvariant() switch
            {
                "BTC" => 0.0005m,   // 0.0005 BTC (~$25 at $50k BTC)
                "ETH" => 0.01m,     // 0.01 ETH (~$25 at $2.5k ETH)
                "USDC" => 15.00m,   // $15 USDC
                "USDT" => 15.00m,   // $15 USDT
                "LTC" => 0.01m,     // 0.01 LTC
                "BCH" => 0.001m,    // 0.001 BCH
                "ADA" => 5.00m,     // 5 ADA
                "DOT" => 1.00m,     // 1 DOT
                "MATIC" => 10.00m,  // 10 MATIC
                _ => Math.Min(amount * 0.02m, 50.00m) // 2% of amount, max $50
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating network fees for {Cryptocurrency}, using default", cryptocurrency);
            return Math.Min(amount * 0.02m, 50.00m);
        }
    }

    /// <summary>
    /// Calculates internal processing fees for rejection handling
    /// </summary>
    private async Task<decimal> CalculateInternalRejectionFeesAsync(decimal amount, string currency, string type)
    {
        try
        {
            // Get internal rejection fee configuration
            var internalFeeConfig = await GetFeeConfigurationAsync($"InternalRejection_{type}", "Internal", null);
            
            if (internalFeeConfig != null)
            {
                return await CalculateBaseFeeAsync(internalFeeConfig, amount, currency);
            }

            // Default internal processing fees
            var baseFee = type.ToLowerInvariant() switch
            {
                "fiat" => 5.00m,   // $5 for fiat processing
                "crypto" => 2.00m, // $2 for crypto processing
                _ => 3.00m         // $3 default
            };

            // Add percentage-based fee (0.5% of amount, max $25)
            var percentageFee = Math.Min(amount * 0.005m, 25.00m);
            
            return baseFee + percentageFee;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating internal rejection fees, using default");
            return 5.00m; // Default fallback
        }
    }
}

/// <summary>
/// Request model for fiat rejection fees calculation
/// </summary>
public class FiatRejectionFeesRequest
{
    /// <summary>
    /// Gets or sets the deposit amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment gateway type
    /// </summary>
    public string GatewayType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string PaymentMethodType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for rejection
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for crypto rejection fees calculation
/// </summary>
public class CryptoRejectionFeesRequest
{
    /// <summary>
    /// Gets or sets the deposit amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the cryptocurrency symbol
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the network name
    /// </summary>
    public string Network { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the wallet address
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for rejection
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Response model for fiat rejection fees
/// </summary>
public class FiatRejectionFeesResponse
{
    /// <summary>
    /// Gets or sets the wire transfer fees
    /// </summary>
    public decimal WireFees { get; set; }

    /// <summary>
    /// Gets or sets the Square processing fees
    /// </summary>
    public decimal SquareFees { get; set; }

    /// <summary>
    /// Gets or sets the internal processing fees
    /// </summary>
    public decimal InternalFees { get; set; }

    /// <summary>
    /// Gets or sets the total rejection fees
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the net amount after fees
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Response model for crypto rejection fees
/// </summary>
public class CryptoRejectionFeesResponse
{
    /// <summary>
    /// Gets or sets the network transaction fees
    /// </summary>
    public decimal NetworkFees { get; set; }

    /// <summary>
    /// Gets or sets the internal processing fees
    /// </summary>
    public decimal InternalFees { get; set; }

    /// <summary>
    /// Gets or sets the total rejection fees
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the net amount after fees
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the cryptocurrency symbol
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the network name
    /// </summary>
    public string Network { get; set; } = string.Empty;
}
