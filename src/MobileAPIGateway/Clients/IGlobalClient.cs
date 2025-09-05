using Refit;
using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for the Global service
/// </summary>
public interface IGlobalClient
{
    /// <summary>
    /// Gets the system status
    /// </summary>
    /// <returns>The system status</returns>
    [Get("/api/global/status")]
    Task<SystemStatus> GetSystemStatusAsync();
    
    /// <summary>
    /// Gets the application configuration
    /// </summary>
    /// <returns>The application configuration</returns>
    [Get("/api/global/config")]
    Task<AppConfig> GetAppConfigAsync();
    
    /// <summary>
    /// Gets the application version
    /// </summary>
    /// <returns>The application version</returns>
    [Get("/api/global/version")]
    Task<string> GetAppVersionAsync();
    
    /// <summary>
    /// Gets the feature flags
    /// </summary>
    /// <returns>The feature flags</returns>
    [Get("/api/global/features")]
    Task<Dictionary<string, bool>> GetFeatureFlagsAsync();
    
    /// <summary>
    /// Gets the application settings
    /// </summary>
    /// <returns>The application settings</returns>
    [Get("/api/global/settings")]
    Task<Dictionary<string, string>> GetAppSettingsAsync();
    
    /// <summary>
    /// Gets the list of genders
    /// </summary>
    /// <returns>The list of genders</returns>
    [Get("/api/global/genders")]
    Task<List<string>> GetGendersAsync();
    
    /// <summary>
    /// Gets the list of languages
    /// </summary>
    /// <returns>The list of languages</returns>
    [Get("/api/global/languages")]
    Task<List<string>> GetLanguagesAsync();
    
    /// <summary>
    /// Gets the list of cryptocurrencies
    /// </summary>
    /// <returns>The list of cryptocurrencies</returns>
    [Get("/api/global/cryptocurrencies")]
    Task<List<string>> GetCryptoCurrenciesAsync();
    
    /// <summary>
    /// Gets the list of search types
    /// </summary>
    /// <returns>The list of search types</returns>
    [Get("/api/global/search-types")]
    Task<List<string>> GetSearchTypesAsync();
    
    /// <summary>
    /// Gets the list of search language types
    /// </summary>
    /// <returns>The list of search language types</returns>
    [Get("/api/global/search-language-types")]
    Task<List<string>> GetSearchLanguageTypesAsync();
    
    /// <summary>
    /// Gets the user limits
    /// </summary>
    /// <returns>The user limits</returns>
    [Get("/api/global/user-limits")]
    Task<LimitResponse> GetUserLimitsAsync();
    
    /// <summary>
    /// Gets the list of user levels
    /// </summary>
    /// <returns>The list of user levels</returns>
    [Get("/api/global/user-levels")]
    Task<List<string>> GetUserLevelsAsync();
    
    /// <summary>
    /// Gets the list of limit types
    /// </summary>
    /// <returns>The list of limit types</returns>
    [Get("/api/global/limit-types")]
    Task<List<string>> GetLimitTypesAsync();
    
    /// <summary>
    /// Gets the list of limit frequencies
    /// </summary>
    /// <returns>The list of limit frequencies</returns>
    [Get("/api/global/limit-frequencies")]
    Task<List<string>> GetLimitFrequenciesAsync();
    
    /// <summary>
    /// Gets the list of payment sources
    /// </summary>
    /// <returns>The list of payment sources</returns>
    [Get("/api/global/payment-sources")]
    Task<List<string>> GetPaymentSourcesAsync();
    
    /// <summary>
    /// Gets the list of bank account types
    /// </summary>
    /// <returns>The list of bank account types</returns>
    [Get("/api/global/bank-account-types")]
    Task<List<string>> GetBankAccountTypesAsync();
    
    /// <summary>
    /// Gets the list of quick action cart types
    /// </summary>
    /// <returns>The list of quick action cart types</returns>
    [Get("/api/global/quick-action-cart-types")]
    Task<List<string>> GetQuickActionCartTypesAsync();
    
    /// <summary>
    /// Gets the list of quick action service types
    /// </summary>
    /// <returns>The list of quick action service types</returns>
    [Get("/api/global/quick-action-service-types")]
    Task<List<string>> GetQuickActionServiceTypesAsync();
    
    /// <summary>
    /// Gets the list of funding options
    /// </summary>
    /// <returns>The list of funding options</returns>
    [Get("/api/global/funding-options")]
    Task<List<string>> GetFundingOptionsAsync();
}
