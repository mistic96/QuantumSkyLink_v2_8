using FeeService.Models.Responses;
using FeeService.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FeeService.Services;

public class ExternalRateProviderService : IExternalRateProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ExternalRateProviderService> _logger;
    private readonly ExternalApiSettings _settings;

    public string ProviderName => "Multi-Provider Rate Service";
    public int Priority => 1;

    public ExternalRateProviderService(
        HttpClient httpClient,
        IDistributedCache cache,
        ILogger<ExternalRateProviderService> logger,
        IOptions<ExternalApiSettings> settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<ExchangeRateResponse?> GetRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            _logger.LogInformation("Getting exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);

            var cacheKey = $"rate_{fromCurrency}_{toCurrency}";
            var cachedRate = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedRate))
            {
                var cached = JsonSerializer.Deserialize<ExchangeRateResponse>(cachedRate);
                if (cached != null && cached.Timestamp > DateTime.UtcNow.AddMinutes(-5))
                {
                    _logger.LogDebug("Returning cached rate for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                    return cached;
                }
            }

            // Try multiple providers in order of preference
            var rate = await TryGetRateFromCoinGecko(fromCurrency, toCurrency) ??
                      await TryGetRateFromBinance(fromCurrency, toCurrency) ??
                      await TryGetRateFromFixer(fromCurrency, toCurrency);

            if (rate != null)
            {
                // Cache the result
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rate), cacheOptions);
            }

            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangeRateResponse>> GetMultipleRatesAsync(IEnumerable<(string from, string to)> currencyPairs)
    {
        var tasks = currencyPairs.Select(pair => GetRateAsync(pair.from, pair.to));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).Cast<ExchangeRateResponse>();
    }

    public async Task<IEnumerable<string>> GetSupportedCurrenciesAsync()
    {
        try
        {
            // Return a basic set of supported currencies
            // In a real implementation, this would query the external providers
            return await Task.FromResult(new[]
            {
                "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD",
                "BTC", "ETH", "BNB", "ADA", "XRP", "SOL", "DOT", "DOGE", "AVAX", "MATIC"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported currencies");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Simple health check - try to get a basic rate
            var testRate = await GetRateAsync("USD", "EUR");
            return testRate != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SupportsCurrencyPairAsync(string fromCurrency, string toCurrency)
    {
        var supportedCurrencies = await GetSupportedCurrenciesAsync();
        return supportedCurrencies.Contains(fromCurrency, StringComparer.OrdinalIgnoreCase) &&
               supportedCurrencies.Contains(toCurrency, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<ExchangeRateResponse?> TryGetRateFromCoinGecko(string fromCurrency, string toCurrency)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.CoinGecko?.BaseUrl))
                return null;

            // Mock implementation for now - in real scenario, make actual API call
            _logger.LogDebug("Attempting to get rate from CoinGecko: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            
            // Simulate API call delay
            await Task.Delay(100);
            
            // Return mock data for demonstration
            return new ExchangeRateResponse
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = GetMockRate(fromCurrency, toCurrency),
                Provider = "CoinGecko",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get rate from CoinGecko");
            return null;
        }
    }

    private async Task<ExchangeRateResponse?> TryGetRateFromBinance(string fromCurrency, string toCurrency)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.Binance?.BaseUrl))
                return null;

            _logger.LogDebug("Attempting to get rate from Binance: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            
            // Simulate API call delay
            await Task.Delay(100);
            
            // Return mock data for demonstration
            return new ExchangeRateResponse
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = GetMockRate(fromCurrency, toCurrency),
                Provider = "Binance",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get rate from Binance");
            return null;
        }
    }

    private async Task<ExchangeRateResponse?> TryGetRateFromFixer(string fromCurrency, string toCurrency)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.Fixer?.BaseUrl))
                return null;

            _logger.LogDebug("Attempting to get rate from Fixer: {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            
            // Simulate API call delay
            await Task.Delay(100);
            
            // Return mock data for demonstration
            return new ExchangeRateResponse
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = GetMockRate(fromCurrency, toCurrency),
                Provider = "Fixer",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get rate from Fixer");
            return null;
        }
    }

    private static decimal GetMockRate(string fromCurrency, string toCurrency)
    {
        // Simple mock rate calculation for demonstration
        // In real implementation, this would come from actual API responses
        var hash = (fromCurrency + toCurrency).GetHashCode();
        var random = new Random(Math.Abs(hash));
        return (decimal)(random.NextDouble() * 100 + 0.1);
    }
}

// Configuration classes for external API settings
public class ExternalApiSettings
{
    public CoinGeckoSettings? CoinGecko { get; set; }
    public BinanceSettings? Binance { get; set; }
    public CoinbaseSettings? Coinbase { get; set; }
    public FixerSettings? Fixer { get; set; }
}

public class CoinGeckoSettings
{
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public int RateLimitPerMinute { get; set; }
}

public class BinanceSettings
{
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public int RateLimitPerMinute { get; set; }
}

public class CoinbaseSettings
{
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public int RateLimitPerMinute { get; set; }
}

public class FixerSettings
{
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public int RateLimitPerMinute { get; set; }
}
