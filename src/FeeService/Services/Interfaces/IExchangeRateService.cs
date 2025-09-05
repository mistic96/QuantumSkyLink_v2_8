using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Services.Interfaces;

public interface IExchangeRateService
{
    /// <summary>
    /// Get current exchange rate between two currencies
    /// </summary>
    Task<ExchangeRateResponse> GetCurrentRateAsync(string fromCurrency, string toCurrency);

    /// <summary>
    /// Get historical exchange rates for a currency pair
    /// </summary>
    Task<IEnumerable<ExchangeRateResponse>> GetHistoricalRatesAsync(
        string fromCurrency, 
        string toCurrency, 
        DateTime fromDate, 
        DateTime toDate);

    /// <summary>
    /// Convert amount from one currency to another
    /// </summary>
    Task<CurrencyConversionResponse> ConvertCurrencyAsync(ConvertCurrencyRequest request);

    /// <summary>
    /// Update exchange rates from external providers
    /// </summary>
    Task UpdateRatesAsync();

    /// <summary>
    /// Get supported currency pairs
    /// </summary>
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync();

    /// <summary>
    /// Get rate with fallback providers
    /// </summary>
    Task<ExchangeRateResponse> GetRateWithFallbackAsync(string fromCurrency, string toCurrency);

    /// <summary>
    /// Validate currency code
    /// </summary>
    bool IsValidCurrency(string currency);
}
