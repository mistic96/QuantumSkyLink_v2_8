using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeeService.Services.Interfaces;
using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Controllers;

/// <summary>
/// Controller for managing exchange rates and currency conversions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<ExchangeRateController> _logger;

    public ExchangeRateController(
        IExchangeRateService exchangeRateService,
        ILogger<ExchangeRateController> logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    /// <summary>
    /// Get current exchange rate between two currencies
    /// </summary>
    /// <param name="fromCurrency">Source currency code (e.g., USD)</param>
    /// <param name="toCurrency">Target currency code (e.g., EUR)</param>
    /// <returns>Current exchange rate</returns>
    [HttpGet("current/{fromCurrency}/{toCurrency}")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExchangeRateResponse>> GetCurrentExchangeRate(
        [FromRoute] string fromCurrency,
        [FromRoute] string toCurrency)
    {
        try
        {
            _logger.LogInformation("Getting current exchange rate from {FromCurrency} to {ToCurrency}", 
                fromCurrency, toCurrency);

            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            {
                return BadRequest("Currency codes cannot be empty");
            }

            var exchangeRate = await _exchangeRateService.GetCurrentRateAsync(fromCurrency, toCurrency);
            
            if (exchangeRate == null)
            {
                return NotFound($"Exchange rate not found for {fromCurrency} to {toCurrency}");
            }

            return Ok(exchangeRate);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid currency codes provided: {FromCurrency} to {ToCurrency}", 
                fromCurrency, toCurrency);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate from {FromCurrency} to {ToCurrency}", 
                fromCurrency, toCurrency);
            return StatusCode(500, "An error occurred while retrieving the exchange rate");
        }
    }

    /// <summary>
    /// Convert currency amount using current exchange rates
    /// </summary>
    /// <param name="request">Currency conversion request</param>
    /// <returns>Converted amount with exchange rate details</returns>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(CurrencyConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyConversionResponse>> ConvertCurrency(
        [FromBody] ConvertCurrencyRequest request)
    {
        try
        {
            _logger.LogInformation("Converting {Amount} {FromCurrency} to {ToCurrency}", 
                request.Amount, request.FromCurrency, request.ToCurrency);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _exchangeRateService.ConvertCurrencyAsync(request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid conversion request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency: {Request}", request);
            return StatusCode(500, "An error occurred while converting currency");
        }
    }

    /// <summary>
    /// Get historical exchange rates for a currency pair
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="startDate">Start date for historical data (optional)</param>
    /// <param name="endDate">End date for historical data (optional)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50, max: 100)</param>
    /// <returns>List of historical exchange rates</returns>
    [HttpGet("historical")]
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ExchangeRateResponse>>> GetHistoricalExchangeRates(
        [FromQuery] string fromCurrency,
        [FromQuery] string toCurrency,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Getting historical exchange rates from {FromCurrency} to {ToCurrency} " +
                "from {StartDate} to {EndDate}, page {Page}, size {PageSize}", 
                fromCurrency, toCurrency, startDate, endDate, page, pageSize);

            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            {
                return BadRequest("Currency codes cannot be empty");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date");
            }

            // Set default dates if not provided
            var fromDate = startDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = endDate ?? DateTime.UtcNow;

            var historicalRates = await _exchangeRateService.GetHistoricalRatesAsync(
                fromCurrency, 
                toCurrency, 
                fromDate, 
                toDate);

            // Apply pagination manually since the interface doesn't support it
            var paginatedRates = historicalRates
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return Ok(paginatedRates);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid historical exchange rate request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical exchange rates");
            return StatusCode(500, "An error occurred while retrieving historical exchange rates");
        }
    }

    /// <summary>
    /// Get list of supported currencies
    /// </summary>
    /// <returns>List of supported currency codes</returns>
    [HttpGet("supported-currencies")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedCurrencies()
    {
        try
        {
            _logger.LogInformation("Getting supported currencies");

            var supportedCurrencies = await _exchangeRateService.GetSupportedCurrenciesAsync();
            return Ok(supportedCurrencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported currencies");
            return StatusCode(500, "An error occurred while retrieving supported currencies");
        }
    }

}
