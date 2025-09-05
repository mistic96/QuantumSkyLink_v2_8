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

public class ExchangeRateService : IExchangeRateService
{
    private readonly FeeDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly IEnumerable<IExternalRateProviderService> _rateProviders;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly IConfiguration _configuration;

    private readonly TimeSpan _cacheExpiry;
    private readonly string[] _supportedCurrencies = {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "SEK", "NZD",
        "BTC", "ETH", "BNB", "ADA", "XRP", "SOL", "DOT", "AVAX", "MATIC", "LINK"
    };

    public ExchangeRateService(
        FeeDbContext context,
        IDistributedCache cache,
        IEnumerable<IExternalRateProviderService> rateProviders,
        ILogger<ExchangeRateService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _rateProviders = rateProviders.OrderBy(p => p.Priority);
        _logger = logger;
        _configuration = configuration;
        _cacheExpiry = TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:ExchangeRateCacheDurationMinutes", 5));
    }

    public async Task<ExchangeRateResponse> GetCurrentRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            if (!IsValidCurrency(fromCurrency) || !IsValidCurrency(toCurrency))
            {
                throw new ArgumentException("Invalid currency code provided");
            }

            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return new ExchangeRateResponse
                {
                    FromCurrency = fromCurrency.ToUpper(),
                    ToCurrency = toCurrency.ToUpper(),
                    Rate = 1.0m,
                    Provider = "Internal",
                    Timestamp = DateTime.UtcNow
                };
            }

            // Try cache first
            var cacheKey = $"exchange_rate:{fromCurrency.ToUpper()}:{toCurrency.ToUpper()}";
            var cachedRate = await GetFromCacheAsync<ExchangeRateResponse>(cacheKey);
            if (cachedRate != null && cachedRate.Timestamp > DateTime.UtcNow.AddMinutes(-_cacheExpiry.TotalMinutes))
            {
                _logger.LogDebug("Retrieved exchange rate from cache: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                return cachedRate;
            }

            // Try database for recent rates
            var dbRate = await _context.ExchangeRates
                .Where(r => r.FromCurrency == fromCurrency.ToUpper() && 
                           r.ToCurrency == toCurrency.ToUpper() && 
                           r.IsActive &&
                           r.Timestamp > DateTime.UtcNow.AddMinutes(-_cacheExpiry.TotalMinutes))
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();

            if (dbRate != null)
            {
                var response = dbRate.Adapt<ExchangeRateResponse>();
                await SetCacheAsync(cacheKey, response, _cacheExpiry);
                _logger.LogDebug("Retrieved exchange rate from database: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                return response;
            }

            // Fetch from external providers
            return await GetRateWithFallbackAsync(fromCurrency, toCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current exchange rate for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw;
        }
    }

    public async Task<ExchangeRateResponse> GetRateWithFallbackAsync(string fromCurrency, string toCurrency)
    {
        foreach (var provider in _rateProviders)
        {
            try
            {
                if (!await provider.IsAvailableAsync())
                {
                    _logger.LogWarning("Provider {ProviderName} is not available", provider.ProviderName);
                    continue;
                }

                if (!await provider.SupportsCurrencyPairAsync(fromCurrency, toCurrency))
                {
                    _logger.LogDebug("Provider {ProviderName} does not support {FromCurrency}/{ToCurrency}", 
                        provider.ProviderName, fromCurrency, toCurrency);
                    continue;
                }

                var rate = await provider.GetRateAsync(fromCurrency, toCurrency);
                if (rate != null)
                {
                    // Store in database
                    await StoreRateInDatabaseAsync(rate);

                    // Cache the result
                    var cacheKey = $"exchange_rate:{fromCurrency.ToUpper()}:{toCurrency.ToUpper()}";
                    await SetCacheAsync(cacheKey, rate, _cacheExpiry);

                    _logger.LogInformation("Successfully retrieved rate from {ProviderName}: {FromCurrency} to {ToCurrency} = {Rate}", 
                        provider.ProviderName, fromCurrency, toCurrency, rate.Rate);
                    return rate;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get rate from provider {ProviderName}", provider.ProviderName);
                continue;
            }
        }

        throw new InvalidOperationException($"Unable to retrieve exchange rate for {fromCurrency} to {toCurrency} from any provider");
    }

    public async Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(
        string fromCurrency, 
        string toCurrency, 
        DateTime fromDate, 
        DateTime toDate)
    {
        try
        {
            if (!IsValidCurrency(fromCurrency) || !IsValidCurrency(toCurrency))
            {
                throw new ArgumentException("Invalid currency code provided");
            }

            if (fromDate > toDate)
            {
                throw new ArgumentException("From date cannot be greater than to date");
            }

            var rates = await _context.ExchangeRates
                .Where(r => r.FromCurrency == fromCurrency.ToUpper() && 
                           r.ToCurrency == toCurrency.ToUpper() && 
                           r.IsActive &&
                           r.Timestamp >= fromDate && 
                           r.Timestamp <= toDate)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();

            return rates.Adapt<IEnumerable<ExchangeRateResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical rates for {FromCurrency} to {ToCurrency} from {FromDate} to {ToDate}", 
                fromCurrency, toCurrency, fromDate, toDate);
            throw;
        }
    }

    public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(ConvertCurrencyRequest request)
    {
        try
        {
            var rate = await GetCurrentRateAsync(request.FromCurrency, request.ToCurrency);
            var convertedAmount = request.Amount * rate.Rate;

            return new CurrencyConversionResponse
            {
                FromCurrency = request.FromCurrency.ToUpper(),
                ToCurrency = request.ToCurrency.ToUpper(),
                OriginalAmount = request.Amount,
                ConvertedAmount = convertedAmount,
                ExchangeRate = rate.Rate,
                Provider = rate.Provider,
                Timestamp = rate.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency from {FromCurrency} to {ToCurrency}, amount: {Amount}", 
                request.FromCurrency, request.ToCurrency, request.Amount);
            throw;
        }
    }

    public async Task UpdateRatesAsync()
    {
        try
        {
            _logger.LogInformation("Starting exchange rate update process");

            var currencyPairs = GenerateCurrencyPairs();
            var updateTasks = new List<Task>();

            foreach (var (from, to) in currencyPairs)
            {
                updateTasks.Add(UpdateCurrencyPairAsync(from, to));
            }

            await Task.WhenAll(updateTasks);
            _logger.LogInformation("Completed exchange rate update process for {PairCount} currency pairs", currencyPairs.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during exchange rate update process");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync()
    {
        return await Task.FromResult(_supportedCurrencies);
    }

    public bool IsValidCurrency(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) && 
               _supportedCurrencies.Contains(currency.ToUpper()) &&
               currency.Length >= 3 && currency.Length <= 10;
    }

    private async Task UpdateCurrencyPairAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            var rate = await GetRateWithFallbackAsync(fromCurrency, toCurrency);
            _logger.LogDebug("Updated rate for {FromCurrency}/{ToCurrency}: {Rate}", fromCurrency, toCurrency, rate.Rate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update rate for {FromCurrency}/{ToCurrency}", fromCurrency, toCurrency);
        }
    }

    private IEnumerable<(string from, string to)> GenerateCurrencyPairs()
    {
        var pairs = new List<(string, string)>();
        var baseCurrencies = new[] { "USD", "EUR", "BTC", "ETH" };

        foreach (var baseCurrency in baseCurrencies)
        {
            foreach (var targetCurrency in _supportedCurrencies)
            {
                if (baseCurrency != targetCurrency)
                {
                    pairs.Add((baseCurrency, targetCurrency));
                }
            }
        }

        return pairs;
    }

    private async Task StoreRateInDatabaseAsync(ExchangeRateResponse rateResponse)
    {
        try
        {
            var exchangeRate = new ExchangeRate
            {
                Id = Guid.NewGuid(),
                FromCurrency = rateResponse.FromCurrency,
                ToCurrency = rateResponse.ToCurrency,
                Rate = rateResponse.Rate,
                Provider = rateResponse.Provider,
                Timestamp = rateResponse.Timestamp,
                IsActive = true,
                Bid = rateResponse.Bid,
                Ask = rateResponse.Ask,
                Volume24h = rateResponse.Volume24h,
                Change24h = rateResponse.Change24h,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExchangeRates.Add(exchangeRate);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing exchange rate in database");
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
}
