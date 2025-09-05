using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Global;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Controller for global operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class GlobalController : BaseController
{
    private readonly IGlobalService _globalService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalController"/> class.
    /// </summary>
    /// <param name="globalService">The global service</param>
    public GlobalController(IGlobalService globalService)
    {
        _globalService = globalService ?? throw new ArgumentNullException(nameof(globalService));
    }

    /// <summary>
    /// Gets the list of genders
    /// </summary>
    /// <returns>The list of genders</returns>
    [HttpGet("GetGenders")]
    public async Task<ActionResult<List<string>>> GetGenders()
    {
        var result = await _globalService.GetGendersAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of languages
    /// </summary>
    /// <returns>The list of languages</returns>
    [HttpGet("GetLanguages")]
    public async Task<ActionResult<List<string>>> GetLanguages()
    {
        var result = await _globalService.GetLanguagesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of cryptocurrencies
    /// </summary>
    /// <returns>The list of cryptocurrencies</returns>
    [HttpGet("GetCryptoCurrencies")]
    public async Task<ActionResult<List<string>>> GetCryptoCurrencies()
    {
        var result = await _globalService.GetCryptoCurrenciesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of search types
    /// </summary>
    /// <returns>The list of search types</returns>
    [HttpGet("GetSearchTypes")]
    public async Task<ActionResult<List<string>>> GetSearchTypes()
    {
        var result = await _globalService.GetSearchTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of search language types
    /// </summary>
    /// <returns>The list of search language types</returns>
    [HttpGet("GetSearchLanguageTypes")]
    public async Task<ActionResult<List<string>>> GetSearchLanguageTypes()
    {
        var result = await _globalService.GetSearchLanguageTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the user limits
    /// </summary>
    /// <returns>The user limits</returns>
    [HttpGet("GetUserLimits")]
    public async Task<ActionResult<LimitResponse>> GetUserLimits()
    {
        var result = await _globalService.GetUserLimitsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of user levels
    /// </summary>
    /// <returns>The list of user levels</returns>
    [HttpGet("GetUserLevels")]
    public async Task<ActionResult<List<string>>> GetUserLevels()
    {
        var result = await _globalService.GetUserLevelsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of limit types
    /// </summary>
    /// <returns>The list of limit types</returns>
    [HttpGet("GetLimitTypes")]
    public async Task<ActionResult<List<string>>> GetLimitTypes()
    {
        var result = await _globalService.GetLimitTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of limit frequencies
    /// </summary>
    /// <returns>The list of limit frequencies</returns>
    [HttpGet("GetLimitFrequencies")]
    public async Task<ActionResult<List<string>>> GetLimitFrequencies()
    {
        var result = await _globalService.GetLimitFrequenciesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of payment sources
    /// </summary>
    /// <returns>The list of payment sources</returns>
    [HttpGet("GetPaymentSources")]
    public async Task<ActionResult<List<string>>> GetPaymentSources()
    {
        var result = await _globalService.GetPaymentSourcesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of bank account types
    /// </summary>
    /// <returns>The list of bank account types</returns>
    [HttpGet("GetBankAccountTypes")]
    public async Task<ActionResult<List<string>>> GetBankAccountTypes()
    {
        var result = await _globalService.GetBankAccountTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of quick action cart types
    /// </summary>
    /// <returns>The list of quick action cart types</returns>
    [HttpGet("GetQuickActionCartTypes")]
    public async Task<ActionResult<List<string>>> GetQuickActionCartTypes()
    {
        var result = await _globalService.GetQuickActionCartTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of quick action service types
    /// </summary>
    /// <returns>The list of quick action service types</returns>
    [HttpGet("GetQuickActionServiceTypes")]
    public async Task<ActionResult<List<string>>> GetQuickActionServiceTypes()
    {
        var result = await _globalService.GetQuickActionServiceTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of funding options
    /// </summary>
    /// <returns>The list of funding options</returns>
    [HttpGet("GetFundingOptions")]
    public async Task<ActionResult<List<string>>> GetFundingOptions()
    {
        var result = await _globalService.GetFundingOptionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the system status
    /// </summary>
    /// <returns>The system status</returns>
    [HttpGet("GetSystemStatus")]
    public async Task<ActionResult<SystemStatus>> GetSystemStatus()
    {
        var result = await _globalService.GetSystemStatusAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the application configuration
    /// </summary>
    /// <returns>The application configuration</returns>
    [HttpGet("GetAppConfig")]
    public async Task<ActionResult<AppConfig>> GetAppConfig()
    {
        var result = await _globalService.GetAppConfigAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets the application version
    /// </summary>
    /// <returns>The application version</returns>
    [HttpGet("GetAppVersion")]
    public async Task<ActionResult<string>> GetAppVersion()
    {
        var result = await _globalService.GetAppVersionAsync();
        return Ok(result);
    }
}
