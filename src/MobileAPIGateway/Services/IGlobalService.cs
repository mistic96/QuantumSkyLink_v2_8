using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Services;

/// <summary>
/// Interface for the Global service
/// </summary>
public interface IGlobalService
{
    /// <summary>
    /// Gets the system status
    /// </summary>
    /// <returns>The system status</returns>
    Task<SystemStatus> GetSystemStatusAsync();
    
    /// <summary>
    /// Gets the application configuration
    /// </summary>
    /// <returns>The application configuration</returns>
    Task<AppConfig> GetAppConfigAsync();
    
    /// <summary>
    /// Gets the application version
    /// </summary>
    /// <returns>The application version</returns>
    Task<string> GetAppVersionAsync();
    
    /// <summary>
    /// Gets the feature flags
    /// </summary>
    /// <returns>The feature flags</returns>
    Task<Dictionary<string, bool>> GetFeatureFlagsAsync();
    
    /// <summary>
    /// Gets the application settings
    /// </summary>
    /// <returns>The application settings</returns>
    Task<Dictionary<string, string>> GetAppSettingsAsync();
    
    /// <summary>
    /// Gets the list of genders
    /// </summary>
    /// <returns>The list of genders</returns>
    Task<List<string>> GetGendersAsync();
    
    /// <summary>
    /// Gets the list of languages
    /// </summary>
    /// <returns>The list of languages</returns>
    Task<List<string>> GetLanguagesAsync();
    
    /// <summary>
    /// Gets the list of cryptocurrencies
    /// </summary>
    /// <returns>The list of cryptocurrencies</returns>
    Task<List<string>> GetCryptoCurrenciesAsync();
    
    /// <summary>
    /// Gets the list of search types
    /// </summary>
    /// <returns>The list of search types</returns>
    Task<List<string>> GetSearchTypesAsync();
    
    /// <summary>
    /// Gets the list of search language types
    /// </summary>
    /// <returns>The list of search language types</returns>
    Task<List<string>> GetSearchLanguageTypesAsync();
    
    /// <summary>
    /// Gets the user limits
    /// </summary>
    /// <returns>The user limits</returns>
    Task<LimitResponse> GetUserLimitsAsync();
    
    /// <summary>
    /// Gets the list of user levels
    /// </summary>
    /// <returns>The list of user levels</returns>
    Task<List<string>> GetUserLevelsAsync();
    
    /// <summary>
    /// Gets the list of limit types
    /// </summary>
    /// <returns>The list of limit types</returns>
    Task<List<string>> GetLimitTypesAsync();
    
    /// <summary>
    /// Gets the list of limit frequencies
    /// </summary>
    /// <returns>The list of limit frequencies</returns>
    Task<List<string>> GetLimitFrequenciesAsync();
    
    /// <summary>
    /// Gets the list of payment sources
    /// </summary>
    /// <returns>The list of payment sources</returns>
    Task<List<string>> GetPaymentSourcesAsync();
    
    /// <summary>
    /// Gets the list of bank account types
    /// </summary>
    /// <returns>The list of bank account types</returns>
    Task<List<string>> GetBankAccountTypesAsync();
    
    /// <summary>
    /// Gets the list of quick action cart types
    /// </summary>
    /// <returns>The list of quick action cart types</returns>
    Task<List<string>> GetQuickActionCartTypesAsync();
    
    /// <summary>
    /// Gets the list of quick action service types
    /// </summary>
    /// <returns>The list of quick action service types</returns>
    Task<List<string>> GetQuickActionServiceTypesAsync();
    
    /// <summary>
    /// Gets the list of funding options
    /// </summary>
    /// <returns>The list of funding options</returns>
    Task<List<string>> GetFundingOptionsAsync();
}
