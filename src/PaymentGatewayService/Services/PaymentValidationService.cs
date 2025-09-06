using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Ent = PaymentGatewayService.Data.Entities;
using Req = global::PaymentGatewayService.Models.Requests;
using Mod = global::PaymentGatewayService.Models;

namespace PaymentGatewayService.Services;

/// <summary>
/// Payment validation service implementing IPaymentValidationService
/// Handles payment request validation, business rules, amount validation, and user limits
/// </summary>
public class PaymentValidationService : IPaymentValidationService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentValidationService> _logger;
    private readonly IQuantumLedgerHubClient? _ledgerClient;

    // Configuration constants
    private const decimal MaxSinglePaymentAmount = 100000m; // $100,000
    private const decimal MaxDailyPaymentAmount = 500000m; // $500,000
    private const int MaxDailyPaymentCount = 50;
    private const decimal MinPaymentAmount = 0.01m; // 1 cent

    public PaymentValidationService(
        PaymentDbContext context,
        ILogger<PaymentValidationService> logger,
        IQuantumLedgerHubClient? ledgerClient = null)
    {
        _context = context;
        _logger = logger;
        _ledgerClient = ledgerClient;
    }

    /// <summary>
    /// Validates a payment request
    /// </summary>
    public async Task ValidatePaymentRequestAsync(Req.ProcessPaymentRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Validating payment request. CorrelationId: {CorrelationId}, UserId: {UserId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, request.UserId, request.Amount, request.Currency);

        try
        {
            // Basic validation
            ValidateBasicPaymentRequest(request);

            // Validate amount and currency
            await ValidateAmountAndCurrencyAsync(request.Amount, request.Currency);

            // Validate user limits (only if UserId is provided)
            if (!string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var parsedUserIdForLimits))
            {
                await ValidateUserLimitsAsync(parsedUserIdForLimits, request.Amount, request.Currency);
            }

            // Validate deposit code for deposit transactions
            if (request.Type == Mod.PaymentType.Deposit && !string.IsNullOrWhiteSpace(request.DepositCode))
            {
                Guid? depositUserId = null;
                if (!string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var parsedForDeposit))
                    depositUserId = parsedForDeposit;

                await ValidateDepositCodeAsync(depositUserId, request.DepositCode, request.Amount, request.Currency);
            }

            // Validate payment method if specified (requires a valid userId)
            if (request.PaymentMethodId.HasValue && !string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var parsedUserIdForMethod))
            {
                await ValidatePaymentMethodAsync(parsedUserIdForMethod, request.PaymentMethodId.Value);
            }

            // Validate gateway if specified
            if (request.PreferredGatewayId.HasValue)
            {
                await ValidateGatewayAsync(request.PreferredGatewayId.Value, request.Amount, request.Currency);
            }

            _logger.LogInformation("Payment request validation completed successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment request validation failed. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates a payment method for a user
    /// </summary>
    public async Task ValidatePaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Validating payment method. CorrelationId: {CorrelationId}, UserId: {UserId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, userId, paymentMethodId);

        try
        {
            var paymentMethod = await _context.PaymentMethods
                .AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.UserId == userId);

            if (paymentMethod == null)
            {
                throw new ValidationException($"Payment method with ID {paymentMethodId} not found for user {userId}");
            }

            if (!paymentMethod.IsActive)
            {
                throw new ValidationException($"Payment method {paymentMethodId} is not active");
            }

            if (!paymentMethod.IsVerified)
            {
                throw new ValidationException($"Payment method {paymentMethodId} is not verified");
            }

            // Check if payment method has expired
            if (paymentMethod.ExpiryDate.HasValue && paymentMethod.ExpiryDate.Value < DateTime.UtcNow)
            {
                throw new ValidationException($"Payment method {paymentMethodId} has expired");
            }

            _logger.LogInformation("Payment method validation completed successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment method validation failed. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates payment amount and currency
    /// </summary>
    public async Task ValidateAmountAndCurrencyAsync(decimal amount, string currency)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Validating amount and currency. CorrelationId: {CorrelationId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, amount, currency);

        try
        {
            // Validate amount
            if (amount <= 0)
            {
                throw new ValidationException("Payment amount must be greater than zero");
            }

            if (amount < MinPaymentAmount)
            {
                throw new ValidationException($"Payment amount must be at least {MinPaymentAmount:C}");
            }

            if (amount > MaxSinglePaymentAmount)
            {
                throw new ValidationException($"Payment amount cannot exceed {MaxSinglePaymentAmount:C}");
            }

            // Validate currency
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ValidationException("Currency is required");
            }

            if (currency.Length != 3)
            {
                throw new ValidationException("Currency must be a valid 3-letter ISO 4217 code");
            }

            // Check if currency is supported
            var supportedCurrencies = await GetSupportedCurrenciesAsync();
            if (!supportedCurrencies.Contains(currency.ToUpperInvariant()))
            {
                throw new ValidationException($"Currency {currency} is not supported");
            }

            _logger.LogInformation("Amount and currency validation completed successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Amount and currency validation failed. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates user payment limits
    /// </summary>
    public async Task ValidateUserLimitsAsync(Guid userId, decimal amount, string currency)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Validating user limits. CorrelationId: {CorrelationId}, UserId: {UserId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, userId, amount, currency);

        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Get today's payments for the user
            var todaysPayments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId && 
                           p.CreatedAt >= today && 
                           p.CreatedAt < tomorrow &&
                           p.Currency == currency &&
                           (p.Status == Ent.PaymentStatus.Completed || p.Status == Ent.PaymentStatus.Processing || p.Status == Ent.PaymentStatus.Pending))
                .ToListAsync();

            // Check daily payment count limit
            if (todaysPayments.Count >= MaxDailyPaymentCount)
            {
                throw new ValidationException($"Daily payment limit of {MaxDailyPaymentCount} transactions exceeded");
            }

            // Check daily payment amount limit
            var todaysTotalAmount = todaysPayments.Sum(p => p.Amount);
            if (todaysTotalAmount + amount > MaxDailyPaymentAmount)
            {
                var remainingLimit = MaxDailyPaymentAmount - todaysTotalAmount;
                throw new ValidationException($"Daily payment amount limit exceeded. Remaining limit: {remainingLimit:C}");
            }

            // Check for suspicious patterns
            await ValidateSuspiciousActivityAsync(userId, amount, todaysPayments);

            _logger.LogInformation("User limits validation completed successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User limits validation failed. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates basic payment request fields
    /// </summary>
    private static void ValidateBasicPaymentRequest(Req.ProcessPaymentRequest request)
    {
        // UserId is now nullable for system-generated deposits
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            if (!Guid.TryParse(request.UserId, out var parsedUserId))
            {
                throw new ValidationException("User ID must be a valid GUID when provided");
            }

            if (parsedUserId == Guid.Empty)
            {
                throw new ValidationException("User ID cannot be empty when provided");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ValidationException("Currency is required");
        }

        if (!Enum.IsDefined(typeof(Mod.PaymentType), request.Type))
        {
            throw new ValidationException("Payment type is required");
        }

        // Validate deposit code is provided for deposit transactions
        if (request.Type == Mod.PaymentType.Deposit && string.IsNullOrWhiteSpace(request.DepositCode))
        {
            throw new ValidationException("Deposit code is required for deposit transactions");
        }

        // Validate description length
        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500)
        {
            throw new ValidationException("Description cannot exceed 500 characters");
        }

        // Validate metadata size and content security
        if (request.Metadata != null)
        {
            ValidateMetadataSecurity(request.Metadata);
            var metadataJson = JsonSerializer.Serialize(request.Metadata);
            if (metadataJson.Length > 10000) // 10KB limit
            {
                throw new ValidationException("Metadata cannot exceed 10KB");
            }
        }
    }

    /// <summary>
    /// Validates gateway compatibility with payment
    /// </summary>
    private async Task ValidateGatewayAsync(Guid gatewayId, decimal amount, string currency)
    {
        var gateway = await _context.PaymentGateways
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gatewayId);

        if (gateway == null)
        {
            throw new ValidationException($"Payment gateway with ID {gatewayId} not found");
        }

        if (!gateway.IsActive)
        {
            throw new ValidationException($"Payment gateway {gateway.Name} is not active");
        }

        // Check currency support
        if (!string.IsNullOrEmpty(gateway.SupportedCurrencies) && 
            !gateway.SupportedCurrencies.Split(',').Contains(currency))
        {
            throw new ValidationException($"Gateway {gateway.Name} does not support currency {currency}");
        }

        // Check amount limits
        if (gateway.MinimumAmount.HasValue && amount < gateway.MinimumAmount.Value)
        {
            throw new ValidationException($"Amount {amount:C} is below gateway minimum of {gateway.MinimumAmount.Value:C}");
        }

        if (gateway.MaximumAmount.HasValue && amount > gateway.MaximumAmount.Value)
        {
            throw new ValidationException($"Amount {amount:C} exceeds gateway maximum of {gateway.MaximumAmount.Value:C}");
        }
    }

    /// <summary>
    /// Validates for suspicious activity patterns
    /// </summary>
    private async Task ValidateSuspiciousActivityAsync(Guid userId, decimal amount, List<Payment> todaysPayments)
    {
        // Check for rapid successive payments (more than 5 in the last hour)
        var lastHour = DateTime.UtcNow.AddHours(-1);
        var recentPayments = todaysPayments.Where(p => p.CreatedAt >= lastHour).ToList();
        
        if (recentPayments.Count >= 5)
        {
            _logger.LogWarning("Suspicious activity detected: rapid successive payments. UserId: {UserId}, Count: {Count}", 
                userId, recentPayments.Count);
            throw new ValidationException("Too many payments in a short time period. Please wait before making another payment.");
        }

        // Check for unusual amount patterns (same amount repeated multiple times)
        var sameAmountCount = todaysPayments.Count(p => p.Amount == amount);
        if (sameAmountCount >= 3)
        {
            _logger.LogWarning("Suspicious activity detected: repeated same amounts. UserId: {UserId}, Amount: {Amount}, Count: {Count}", 
                userId, amount, sameAmountCount);
            throw new ValidationException("Multiple payments with the same amount detected. Please contact support if this is legitimate.");
        }

        // Check for failed payment attempts in the last hour
        var failedPayments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId && 
                       p.CreatedAt >= lastHour &&
                       p.Status == Ent.PaymentStatus.Failed)
            .CountAsync();

        if (failedPayments >= 3)
        {
            _logger.LogWarning("Suspicious activity detected: multiple failed payments. UserId: {UserId}, FailedCount: {FailedCount}", 
                userId, failedPayments);
            throw new ValidationException("Multiple failed payment attempts detected. Please wait before trying again.");
        }
    }

    /// <summary>
    /// Gets list of supported currencies
    /// </summary>
    private async Task<List<string>> GetSupportedCurrenciesAsync()
    {
        var gateways = await _context.PaymentGateways
            .AsNoTracking()
            .Where(g => g.IsActive && !string.IsNullOrEmpty(g.SupportedCurrencies))
            .Select(g => g.SupportedCurrencies)
            .ToListAsync();

        var supportedCurrencies = new HashSet<string>();
        
        foreach (var currencyList in gateways)
        {
            if (!string.IsNullOrEmpty(currencyList))
            {
                var currencies = currencyList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var currency in currencies)
                {
                    supportedCurrencies.Add(currency.Trim().ToUpperInvariant());
                }
            }
        }

        // Add default supported currencies if no gateways are configured
        if (!supportedCurrencies.Any())
        {
            supportedCurrencies.Add("USD");
            supportedCurrencies.Add("EUR");
            supportedCurrencies.Add("GBP");
            supportedCurrencies.Add("CAD");
            supportedCurrencies.Add("AUD");
        }

        return supportedCurrencies.ToList();
    }

    /// <summary>
    /// Validates deposit code for deposits
    /// </summary>
    public async Task ValidateDepositCodeAsync(Guid? userId, string depositCode, decimal amount, string currency)
    {
        var correlationId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation("Validating deposit code. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, userId, depositCode, amount, currency);

        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(depositCode))
            {
                throw new ValidationException("Deposit code is required for deposit transactions");
            }

            if (depositCode.Length != 8)
            {
                throw new ValidationException("Deposit code must be exactly 8 characters");
            }

            // Check if deposit code format is valid (alphanumeric only)
            if (!IsValidDepositCodeFormat(depositCode))
            {
                throw new ValidationException("Deposit code contains invalid characters. Only alphanumeric characters are allowed");
            }

            // First check with QuantumLedger.Hub if available
            if (_ledgerClient != null)
            {
                var ledgerValidation = await _ledgerClient.ValidateDepositCodeAsync(depositCode);
                if (ledgerValidation != null && !ledgerValidation.Exists)
                {
                    _logger.LogWarning("Deposit code not found in ledger. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}", 
                        correlationId, depositCode);
                    throw new ValidationException("Invalid deposit code");
                }
            }

            // Case-insensitive lookup
            var depositCodeEntity = await _context.DepositCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(dc => dc.Code.ToUpper() == depositCode.ToUpper());

            if (depositCodeEntity == null)
            {
                _logger.LogWarning("Invalid deposit code attempted. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}", 
                    correlationId, depositCode);
                throw new ValidationException("Invalid deposit code");
            }

            // Check if deposit code is active
            if (depositCodeEntity.Status != DepositCodeStatus.Active)
            {
                _logger.LogWarning("Inactive deposit code attempted. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}, Status: {Status}", 
                    correlationId, depositCode, depositCodeEntity.Status);
                throw new ValidationException($"Deposit code is {depositCodeEntity.Status.ToString().ToLower()} and cannot be used");
            }

            // Check if deposit code has expired
            if (depositCodeEntity.ExpiresAt.HasValue && depositCodeEntity.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired deposit code attempted. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}, ExpiresAt: {ExpiresAt}", 
                    correlationId, depositCode, depositCodeEntity.ExpiresAt);
                throw new ValidationException("Deposit code has expired");
            }

            // Validate amount matches (if specified in the deposit code)
            if (depositCodeEntity.Amount.HasValue && depositCodeEntity.Amount.Value > 0 && Math.Abs(depositCodeEntity.Amount.Value - amount) > 0.01m)
            {
                _logger.LogWarning("Amount mismatch for deposit code. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}, Expected: {ExpectedAmount}, Actual: {ActualAmount}", 
                    correlationId, depositCode, depositCodeEntity.Amount, amount);
                throw new ValidationException($"Deposit amount {amount:C} does not match the expected amount {depositCodeEntity.Amount:C} for this deposit code");
            }

            // Validate currency matches (if specified in the deposit code)
            if (!string.IsNullOrEmpty(depositCodeEntity.Currency) && 
                !depositCodeEntity.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Currency mismatch for deposit code. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}, Expected: {ExpectedCurrency}, Actual: {ActualCurrency}", 
                    correlationId, depositCode, depositCodeEntity.Currency, currency);
                throw new ValidationException($"Currency {currency} does not match the expected currency {depositCodeEntity.Currency} for this deposit code");
            }

            // Validate user authorization (if userId is provided and deposit code has a specific user)
            if (userId.HasValue && depositCodeEntity.UserId != Guid.Empty && depositCodeEntity.UserId != userId.Value)
            {
                _logger.LogWarning("Unauthorized deposit code usage attempted. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}, RequestingUserId: {RequestingUserId}, AuthorizedUserId: {AuthorizedUserId}", 
                    correlationId, depositCode, userId.Value, depositCodeEntity.UserId);
                throw new ValidationException("You are not authorized to use this deposit code");
            }

            _logger.LogInformation("Deposit code validation completed successfully. CorrelationId: {CorrelationId}", correlationId);
            
            // Record successful validation metrics (monitoring removed)
            var duration = DateTime.UtcNow - startTime;
            _logger.LogDebug("Deposit code validation succeeded (monitoring removed). Duration: {Duration}, Amount: {Amount}, Currency: {Currency}", duration, amount, currency);
        }
        catch (ValidationException ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Deposit code validation failed. CorrelationId: {CorrelationId}", correlationId);
            
            // Monitoring removed: log failure details
            _logger.LogDebug("Deposit code validation failed (monitoring removed). Reason: {Reason}, Duration: {Duration}", ex.Message, duration);
            
            // Log security event placeholder if suspicious
            if (IsSuspiciousValidationFailure(ex.Message))
            {
                _logger.LogWarning("Suspicious validation failure detected (monitoring removed): User: {UserId}, Code: {Code}, Reason: {Reason}", userId, depositCode, ex.Message);
            }
            
            throw;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Deposit code validation failed with unexpected error. CorrelationId: {CorrelationId}", correlationId);
            
            // Monitoring removed: log system error
            _logger.LogDebug("Deposit code validation system error (monitoring removed). Duration: {Duration}", duration);
            
            throw;
        }
    }

    /// <summary>
    /// Generates a unique 8-character cryptographic deposit code
    /// </summary>
    public async Task<string> GenerateDepositCodeAsync(Guid? userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation("Generating deposit code. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            string depositCode;
            int attempts = 0;
            const int maxAttempts = 10;

            // Generate unique deposit code with collision detection
            do
            {
                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("Failed to generate unique deposit code after maximum attempts");
                }

                depositCode = GenerateCryptographicCode();
                attempts++;

                // Check for case-insensitive duplicates
                var existingCode = await _context.DepositCodes
                    .AsNoTracking()
                    .AnyAsync(dc => dc.Code.ToUpper() == depositCode.ToUpper());

                if (!existingCode)
                {
                    break;
                }

                _logger.LogDebug("Deposit code collision detected, regenerating. CorrelationId: {CorrelationId}, Attempt: {Attempt}", 
                    correlationId, attempts);

            } while (true);

            // Create deposit code entity
            var depositCodeEntity = new DepositCode
            {
                // Id left to DB (int)
                Code = depositCode.ToUpper(), // Store in uppercase for consistency
                UserId = userId ?? Guid.Empty,
                Status = DepositCodeStatus.Active,
                Amount = 0, // Amount will be validated when used
                Currency = "USD", // Default currency, can be overridden
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 24-hour expiry
                Metadata = JsonSerializer.Serialize(new
                {
                    GeneratedBy = userId?.ToString() ?? "System",
                    GeneratedAt = DateTime.UtcNow.ToString("O"),
                    GenerationCorrelationId = correlationId
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DepositCodes.Add(depositCodeEntity);
            await _context.SaveChangesAsync();

            // Record in QuantumLedger.Hub if available
            if (_ledgerClient != null)
            {
                var ledgerEntry = new DepositCodeLedgerEntry
                {
                    DepositCode = depositCode,
                    UserId = userId,
                    Amount = 0, // Will be set when used
                    Currency = "USD",
                    CreatedAt = depositCodeEntity.CreatedAt,
                    ExpiresAt = depositCodeEntity.ExpiresAt ?? DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["correlationId"] = correlationId,
                        ["generatedBy"] = userId?.ToString() ?? "System"
                    }
                };

                var ledgerSuccess = await _ledgerClient.RecordDepositCodeCreationAsync(ledgerEntry);
                if (!ledgerSuccess)
                {
                    _logger.LogWarning("Failed to record deposit code in ledger, but local record created. DepositCode: {DepositCode}", depositCode);
                }
            }

            _logger.LogInformation("Deposit code generated successfully. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}", 
                correlationId, depositCode);

            // Record successful generation metrics (monitoring removed)
            var duration = DateTime.UtcNow - startTime;
            _logger.LogDebug("Deposit code generated (monitoring removed). GeneratedBy: {GeneratedBy}, Duration: {Duration}", userId?.ToString() ?? "System", duration);

            return depositCode;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error generating deposit code. CorrelationId: {CorrelationId}", correlationId);
            
            // Monitoring removed: log failed generation
            _logger.LogDebug("Deposit code generation failed (monitoring removed). GeneratedBy: {GeneratedBy}, Duration: {Duration}", userId?.ToString() ?? "System", duration);
            
            throw;
        }
    }

    /// <summary>
    /// Validates deposit code format (alphanumeric only)
    /// </summary>
    private static bool IsValidDepositCodeFormat(string depositCode)
    {
        return depositCode.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Generates a cryptographically secure 8-character code with enhanced entropy
    /// Uses NIST-approved character set excluding ambiguous characters for security
    /// </summary>
    private static string GenerateCryptographicCode()
    {
        // Enhanced character set excluding ambiguous characters (0, O, 1, I, l)
        // Total entropy: 32^8 = ~1.1×10^12 combinations (acceptable for 8-char codes)
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var result = new char[8];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            // Generate extra entropy to avoid modulo bias
            var bytes = new byte[16]; // Double the required bytes for better entropy
            rng.GetBytes(bytes);
            
            for (int i = 0; i < 8; i++)
            {
                // Use two bytes per character to reduce modulo bias
                var randomValue = (bytes[i * 2] << 8) | bytes[i * 2 + 1];
                result[i] = chars[randomValue % chars.Length];
            }
        }
        
        return new string(result);
    }

    /// <summary>
    /// Determines if a validation failure reason indicates suspicious activity
    /// </summary>
    private static bool IsSuspiciousValidationFailure(string? rejectionReason)
    {
        if (string.IsNullOrEmpty(rejectionReason))
            return false;

        var suspiciousReasons = new[]
        {
            "Invalid deposit code",
            "Deposit code contains invalid characters",
            "You are not authorized to use this deposit code"
        };

        return suspiciousReasons.Any(reason => 
            rejectionReason.Contains(reason, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates metadata content for security threats and injection attacks
    /// </summary>
    private static void ValidateMetadataSecurity(Dictionary<string, object> metadata)
    {
        if (metadata == null) return;

        const int maxKeys = 50;
        const int maxKeyLength = 100;
        const int maxValueLength = 1000;

        if (metadata.Count > maxKeys)
        {
            throw new ValidationException($"Metadata cannot contain more than {maxKeys} keys");
        }

        foreach (var kvp in metadata)
        {
            // Validate key security
            if (string.IsNullOrEmpty(kvp.Key))
            {
                throw new ValidationException("Metadata keys cannot be null or empty");
            }

            if (kvp.Key.Length > maxKeyLength)
            {
                throw new ValidationException($"Metadata key '{kvp.Key}' exceeds maximum length of {maxKeyLength} characters");
            }

            if (ContainsSuspiciousContent(kvp.Key))
            {
                throw new ValidationException($"Metadata key '{kvp.Key}' contains potentially malicious content");
            }

            // Validate value security
            if (kvp.Value != null)
            {
                var valueString = kvp.Value.ToString() ?? string.Empty;
                
                if (valueString.Length > maxValueLength)
                {
                    throw new ValidationException($"Metadata value for key '{kvp.Key}' exceeds maximum length of {maxValueLength} characters");
                }

                if (ContainsSuspiciousContent(valueString))
                {
                    throw new ValidationException($"Metadata value for key '{kvp.Key}' contains potentially malicious content");
                }
            }
        }
    }

    /// <summary>
    /// Checks for suspicious content patterns that could indicate injection attacks
    /// </summary>
    private static bool ContainsSuspiciousContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return false;

        var suspiciousPatterns = new[]
        {
            "<script", "javascript:", "data:", "vbscript:",
            "onload=", "onerror=", "onclick=", "onmouseover=",
            "eval(", "expression(", "url(", "import(",
            "{{", "}}", "${", "<%", "%>", "<?", "?>",
            "DROP TABLE", "DELETE FROM", "INSERT INTO", "UPDATE SET",
            "UNION SELECT", "OR 1=1", "AND 1=1", "'; --",
            "\0", "\x00", "\r\n\r\n", "\n\n"
        };

        return suspiciousPatterns.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Custom validation exception
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
