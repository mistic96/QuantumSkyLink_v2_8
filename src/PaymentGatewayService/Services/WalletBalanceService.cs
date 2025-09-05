using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PaymentGatewayService.Clients;
using PaymentGatewayService.Services.Interfaces;
using System.Text.Json;

namespace PaymentGatewayService.Services;

/// <summary>
/// Service for managing wallet balances with caching support
/// </summary>
public class WalletBalanceService : IWalletBalanceService
{
    private readonly ILogger<WalletBalanceService> _logger;
    private readonly ITreasuryServiceClient _treasuryServiceClient;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public WalletBalanceService(
        ILogger<WalletBalanceService> logger,
        ITreasuryServiceClient treasuryServiceClient,
        IDistributedCache cache)
    {
        _logger = logger;
        _treasuryServiceClient = treasuryServiceClient;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetBalanceAsync(string userId, string currency)
    {
        var balanceInfo = await GetBalanceInfoAsync(userId, currency);
        return balanceInfo.AvailableBalance;
    }

    /// <inheritdoc/>
    public async Task<WalletBalanceInfo> GetBalanceInfoAsync(string userId, string currency)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = $"wallet_balance:{userId}:{currency}";
            var cachedBalance = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedBalance))
            {
                var cached = JsonSerializer.Deserialize<WalletBalanceInfo>(cachedBalance);
                if (cached != null && cached.LastUpdated > DateTime.UtcNow.AddMinutes(-5))
                {
                    _logger.LogDebug("Returning cached balance for user {UserId}, currency {Currency}", 
                        userId, currency);
                    return cached;
                }
            }

            // Fetch from Treasury Service
            var response = await _treasuryServiceClient.GetWalletBalanceAsync(userId, currency);
            
            var balanceInfo = new WalletBalanceInfo
            {
                UserId = userId,
                Currency = currency,
                TotalBalance = response.Balance,
                AvailableBalance = response.AvailableBalance,
                PendingBalance = response.PendingBalance,
                LockedBalance = response.Balance - response.AvailableBalance - response.PendingBalance,
                LastUpdated = response.LastUpdated,
                IsActive = true
            };

            // Cache the result
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(balanceInfo),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiry
                });

            _logger.LogInformation("Retrieved balance for user {UserId}, currency {Currency}: {Balance}", 
                userId, currency, balanceInfo.AvailableBalance);

            return balanceInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for user {UserId}, currency {Currency}", 
                userId, currency);
            
            // Return a default balance info on error
            return new WalletBalanceInfo
            {
                UserId = userId,
                Currency = currency,
                TotalBalance = 0,
                AvailableBalance = 0,
                PendingBalance = 0,
                LockedBalance = 0,
                LastUpdated = DateTime.UtcNow,
                IsActive = false
            };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> HasSufficientBalanceAsync(string userId, string currency, decimal amount)
    {
        var balance = await GetBalanceAsync(userId, currency);
        return balance >= amount;
    }

    /// <inheritdoc/>
    public async Task<decimal> UpdateBalanceAsync(string userId, string currency, decimal amount, string reason)
    {
        try
        {
            _logger.LogInformation("Updating balance for user {UserId}, currency {Currency}, amount {Amount}, reason {Reason}", 
                userId, currency, amount, reason);

            // Create a transaction to update the balance
            var transactionRequest = new CreateTransactionRequest
            {
                UserId = userId,
                Currency = currency,
                Amount = Math.Abs(amount),
                Type = amount > 0 ? "deposit" : "withdrawal",
                Reference = $"BALANCE-UPDATE-{Guid.NewGuid()}",
                Metadata = new Dictionary<string, string>
                {
                    ["reason"] = reason,
                    ["updatedBy"] = "WalletBalanceService"
                }
            };

            var transaction = await _treasuryServiceClient.CreateTransactionAsync(transactionRequest);

            // Invalidate cache
            var cacheKey = $"wallet_balance:{userId}:{currency}";
            await _cache.RemoveAsync(cacheKey);

            // Get updated balance
            var updatedBalance = await GetBalanceAsync(userId, currency);
            
            _logger.LogInformation("Balance updated successfully for user {UserId}, new balance: {Balance}", 
                userId, updatedBalance);

            return updatedBalance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for user {UserId}, currency {Currency}", 
                userId, currency);
            throw;
        }
    }
}