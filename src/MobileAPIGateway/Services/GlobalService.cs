using Microsoft.Extensions.Logging;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Global;

namespace MobileAPIGateway.Services;

/// <summary>
/// Implementation of the Global service
/// </summary>
public class GlobalService : IGlobalService
{
    private readonly IGlobalClient _globalClient;
    private readonly ILogger<GlobalService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalService"/> class.
    /// </summary>
    /// <param name="globalClient">The global client</param>
    /// <param name="logger">The logger</param>
    public GlobalService(IGlobalClient globalClient, ILogger<GlobalService> logger)
    {
        _globalClient = globalClient ?? throw new ArgumentNullException(nameof(globalClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SystemStatus> GetSystemStatusAsync()
    {
        _logger.LogInformation("Getting system status");
        return await _globalClient.GetSystemStatusAsync();
    }

    /// <inheritdoc />
    public async Task<AppConfig> GetAppConfigAsync()
    {
        _logger.LogInformation("Getting application configuration");
        return await _globalClient.GetAppConfigAsync();
    }

    /// <inheritdoc />
    public async Task<string> GetAppVersionAsync()
    {
        _logger.LogInformation("Getting application version");
        return await _globalClient.GetAppVersionAsync();
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, bool>> GetFeatureFlagsAsync()
    {
        _logger.LogInformation("Getting feature flags");
        return await _globalClient.GetFeatureFlagsAsync();
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetAppSettingsAsync()
    {
        _logger.LogInformation("Getting application settings");
        return await _globalClient.GetAppSettingsAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetGendersAsync()
    {
        _logger.LogInformation("Getting genders");
        return await _globalClient.GetGendersAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetLanguagesAsync()
    {
        _logger.LogInformation("Getting languages");
        return await _globalClient.GetLanguagesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetCryptoCurrenciesAsync()
    {
        _logger.LogInformation("Getting cryptocurrencies");
        return await _globalClient.GetCryptoCurrenciesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetSearchTypesAsync()
    {
        _logger.LogInformation("Getting search types");
        return await _globalClient.GetSearchTypesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetSearchLanguageTypesAsync()
    {
        _logger.LogInformation("Getting search language types");
        return await _globalClient.GetSearchLanguageTypesAsync();
    }

    /// <inheritdoc />
    public async Task<LimitResponse> GetUserLimitsAsync()
    {
        _logger.LogInformation("Getting user limits");
        return await _globalClient.GetUserLimitsAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetUserLevelsAsync()
    {
        _logger.LogInformation("Getting user levels");
        return await _globalClient.GetUserLevelsAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetLimitTypesAsync()
    {
        _logger.LogInformation("Getting limit types");
        return await _globalClient.GetLimitTypesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetLimitFrequenciesAsync()
    {
        _logger.LogInformation("Getting limit frequencies");
        return await _globalClient.GetLimitFrequenciesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetPaymentSourcesAsync()
    {
        _logger.LogInformation("Getting payment sources");
        return await _globalClient.GetPaymentSourcesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetBankAccountTypesAsync()
    {
        _logger.LogInformation("Getting bank account types");
        return await _globalClient.GetBankAccountTypesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetQuickActionCartTypesAsync()
    {
        _logger.LogInformation("Getting quick action cart types");
        return await _globalClient.GetQuickActionCartTypesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetQuickActionServiceTypesAsync()
    {
        _logger.LogInformation("Getting quick action service types");
        return await _globalClient.GetQuickActionServiceTypesAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetFundingOptionsAsync()
    {
        _logger.LogInformation("Getting funding options");
        return await _globalClient.GetFundingOptionsAsync();
    }
}
