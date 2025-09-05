using FeeService.Models.Responses;

namespace FeeService.Services.Interfaces;

public interface IExternalRateProviderService
{
    /// <summary>
    /// Get exchange rate from external provider
    /// </summary>
    Task<ExchangeRateResponse?> GetRateAsync(string fromCurrency, string toCurrency);

    /// <summary>
    /// Get multiple rates from external provider
    /// </summary>
    Task<IEnumerable<ExchangeRateResponse>> GetMultipleRatesAsync(IEnumerable<(string from, string to)> currencyPairs);

    /// <summary>
    /// Get supported currencies from provider
    /// </summary>
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync();

    /// <summary>
    /// Check if provider is available
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Get provider name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Get provider priority (lower number = higher priority)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Check if currency pair is supported
    /// </summary>
    Task<bool> SupportsCurrencyPairAsync(string fromCurrency, string toCurrency);
}
