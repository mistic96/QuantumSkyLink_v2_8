using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.CustomerMarkets;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Customer markets controller
/// </summary>
[ApiController]
[Route("api/customer-markets")]
[Authorize]
public class CustomerMarketsController : BaseController
{
    private readonly ICustomerMarketsService _customerMarketsService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketsController"/> class
    /// </summary>
    /// <param name="customerMarketsService">Customer markets service</param>
    public CustomerMarketsController(ICustomerMarketsService customerMarketsService)
    {
        _customerMarketsService = customerMarketsService;
    }
    
    /// <summary>
    /// Gets all customer markets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer markets</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerMarket>>> GetCustomerMarketsAsync(CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var markets = await _customerMarketsService.GetCustomerMarketsAsync(customerId, cancellationToken);
        return Ok(markets);
    }
    
    /// <summary>
    /// Gets a customer market by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer market</returns>
    [HttpGet("{marketId}")]
    public async Task<ActionResult<CustomerMarket>> GetCustomerMarketAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var market = await _customerMarketsService.GetCustomerMarketAsync(customerId, marketId, cancellationToken);
        return Ok(market);
    }
    
    /// <summary>
    /// Gets all customer trading pairs for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer trading pairs</returns>
    [HttpGet("{marketId}/trading-pairs")]
    public async Task<ActionResult<IEnumerable<CustomerTradingPair>>> GetCustomerTradingPairsAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var tradingPairs = await _customerMarketsService.GetCustomerTradingPairsAsync(customerId, marketId, cancellationToken);
        return Ok(tradingPairs);
    }
    
    /// <summary>
    /// Gets a customer trading pair by symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer trading pair</returns>
    [HttpGet("trading-pairs/{symbol}")]
    public async Task<ActionResult<CustomerTradingPair>> GetCustomerTradingPairAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var tradingPair = await _customerMarketsService.GetCustomerTradingPairAsync(customerId, symbol, cancellationToken);
        return Ok(tradingPair);
    }
    
    /// <summary>
    /// Gets all customer trading pair alerts
    /// </summary>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer trading pair alerts</returns>
    [HttpGet("trading-pairs/{tradingPairId}/alerts")]
    public async Task<ActionResult<IEnumerable<CustomerTradingPairAlert>>> GetCustomerTradingPairAlertsAsync(string tradingPairId, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var alerts = await _customerMarketsService.GetCustomerTradingPairAlertsAsync(customerId, tradingPairId, cancellationToken);
        return Ok(alerts);
    }
    
    /// <summary>
    /// Creates a customer trading pair alert
    /// </summary>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alert">Customer trading pair alert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer trading pair alert</returns>
    [HttpPost("trading-pairs/{tradingPairId}/alerts")]
    public async Task<ActionResult<CustomerTradingPairAlert>> CreateCustomerTradingPairAlertAsync(string tradingPairId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var createdAlert = await _customerMarketsService.CreateCustomerTradingPairAlertAsync(customerId, tradingPairId, alert, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerTradingPairAlertsAsync), new { tradingPairId }, createdAlert);
    }
    
    /// <summary>
    /// Updates a customer trading pair alert
    /// </summary>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="alert">Customer trading pair alert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated customer trading pair alert</returns>
    [HttpPut("trading-pairs/{tradingPairId}/alerts/{alertId}")]
    public async Task<ActionResult<CustomerTradingPairAlert>> UpdateCustomerTradingPairAlertAsync(string tradingPairId, string alertId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var updatedAlert = await _customerMarketsService.UpdateCustomerTradingPairAlertAsync(customerId, tradingPairId, alertId, alert, cancellationToken);
        return Ok(updatedAlert);
    }
    
    /// <summary>
    /// Deletes a customer trading pair alert
    /// </summary>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("trading-pairs/{tradingPairId}/alerts/{alertId}")]
    public async Task<ActionResult> DeleteCustomerTradingPairAlertAsync(string tradingPairId, string alertId, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        await _customerMarketsService.DeleteCustomerTradingPairAlertAsync(customerId, tradingPairId, alertId, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Gets all customer market subscriptions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer market subscriptions</returns>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<IEnumerable<CustomerMarketSubscription>>> GetCustomerMarketSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var subscriptions = await _customerMarketsService.GetCustomerMarketSubscriptionsAsync(customerId, cancellationToken);
        return Ok(subscriptions);
    }
    
    /// <summary>
    /// Gets a customer market subscription by ID
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer market subscription</returns>
    [HttpGet("subscriptions/{subscriptionId}")]
    public async Task<ActionResult<CustomerMarketSubscription>> GetCustomerMarketSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var subscription = await _customerMarketsService.GetCustomerMarketSubscriptionAsync(customerId, subscriptionId, cancellationToken);
        return Ok(subscription);
    }
    
    /// <summary>
    /// Gets all subscription plans for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of subscription plans</returns>
    [HttpGet("markets/{marketId}/subscription-plans")]
    public async Task<ActionResult<IEnumerable<CustomerMarketSubscriptionPlan>>> GetSubscriptionPlansAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var plans = await _customerMarketsService.GetSubscriptionPlansAsync(marketId, cancellationToken);
        return Ok(plans);
    }
    
    /// <summary>
    /// Gets a subscription plan by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="planId">Subscription plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription plan</returns>
    [HttpGet("markets/{marketId}/subscription-plans/{planId}")]
    public async Task<ActionResult<CustomerMarketSubscriptionPlan>> GetSubscriptionPlanAsync(string marketId, string planId, CancellationToken cancellationToken = default)
    {
        var plan = await _customerMarketsService.GetSubscriptionPlanAsync(marketId, planId, cancellationToken);
        return Ok(plan);
    }
    
    /// <summary>
    /// Creates a customer market subscription
    /// </summary>
    /// <param name="subscription">Customer market subscription</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer market subscription</returns>
    [HttpPost("subscriptions")]
    public async Task<ActionResult<CustomerMarketSubscription>> CreateCustomerMarketSubscriptionAsync(CustomerMarketSubscription subscription, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var createdSubscription = await _customerMarketsService.CreateCustomerMarketSubscriptionAsync(customerId, subscription, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerMarketSubscriptionAsync), new { subscriptionId = createdSubscription.Id }, createdSubscription);
    }
    
    /// <summary>
    /// Cancels a customer market subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled customer market subscription</returns>
    [HttpPut("subscriptions/{subscriptionId}/cancel")]
    public async Task<ActionResult<CustomerMarketSubscription>> CancelCustomerMarketSubscriptionAsync(string subscriptionId, [FromBody] string reason, CancellationToken cancellationToken = default)
    {
        var customerId = CurrentUserId;
        var cancelledSubscription = await _customerMarketsService.CancelCustomerMarketSubscriptionAsync(customerId, subscriptionId, reason, cancellationToken);
        return Ok(cancelledSubscription);
    }
}
