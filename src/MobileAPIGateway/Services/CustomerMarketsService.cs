using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Customer markets service
/// </summary>
public class CustomerMarketsService : ICustomerMarketsService
{
    private readonly ICustomerMarketsClient _customerMarketsClient;
    private readonly ILogger<CustomerMarketsService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketsService"/> class
    /// </summary>
    /// <param name="customerMarketsClient">Customer markets client</param>
    /// <param name="logger">Logger</param>
    public CustomerMarketsService(
        ICustomerMarketsClient customerMarketsClient,
        ILogger<CustomerMarketsService> logger)
    {
        _customerMarketsClient = customerMarketsClient;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CustomerMarket>> GetCustomerMarketsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer markets for customer {CustomerId}", customerId);
            
            var markets = await _customerMarketsClient.GetCustomerMarketsAsync(customerId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} customer markets for customer {CustomerId}", markets.Count(), customerId);
            
            return markets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer markets for customer {CustomerId}", customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerMarket> GetCustomerMarketAsync(string customerId, string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer market {MarketId} for customer {CustomerId}", marketId, customerId);
            
            var market = await _customerMarketsClient.GetCustomerMarketAsync(customerId, marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved customer market {MarketId} for customer {CustomerId}", marketId, customerId);
            
            return market;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer market {MarketId} for customer {CustomerId}", marketId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CustomerTradingPair>> GetCustomerTradingPairsAsync(string customerId, string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer trading pairs for market {MarketId} and customer {CustomerId}", marketId, customerId);
            
            var tradingPairs = await _customerMarketsClient.GetCustomerTradingPairsAsync(customerId, marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} customer trading pairs for market {MarketId} and customer {CustomerId}", tradingPairs.Count(), marketId, customerId);
            
            return tradingPairs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer trading pairs for market {MarketId} and customer {CustomerId}", marketId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerTradingPair> GetCustomerTradingPairAsync(string customerId, string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer trading pair {Symbol} for customer {CustomerId}", symbol, customerId);
            
            var tradingPair = await _customerMarketsClient.GetCustomerTradingPairAsync(customerId, symbol, cancellationToken);
            
            _logger.LogInformation("Retrieved customer trading pair {Symbol} for customer {CustomerId}", symbol, customerId);
            
            return tradingPair;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer trading pair {Symbol} for customer {CustomerId}", symbol, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CustomerTradingPairAlert>> GetCustomerTradingPairAlertsAsync(string customerId, string tradingPairId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer trading pair alerts for trading pair {TradingPairId} and customer {CustomerId}", tradingPairId, customerId);
            
            var alerts = await _customerMarketsClient.GetCustomerTradingPairAlertsAsync(customerId, tradingPairId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} customer trading pair alerts for trading pair {TradingPairId} and customer {CustomerId}", alerts.Count(), tradingPairId, customerId);
            
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer trading pair alerts for trading pair {TradingPairId} and customer {CustomerId}", tradingPairId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerTradingPairAlert> CreateCustomerTradingPairAlertAsync(string customerId, string tradingPairId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating customer trading pair alert for trading pair {TradingPairId} and customer {CustomerId}", tradingPairId, customerId);
            
            var createdAlert = await _customerMarketsClient.CreateCustomerTradingPairAlertAsync(customerId, tradingPairId, alert, cancellationToken);
            
            _logger.LogInformation("Created customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", createdAlert.Id, tradingPairId, customerId);
            
            return createdAlert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer trading pair alert for trading pair {TradingPairId} and customer {CustomerId}", tradingPairId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerTradingPairAlert> UpdateCustomerTradingPairAlertAsync(string customerId, string tradingPairId, string alertId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
            
            var updatedAlert = await _customerMarketsClient.UpdateCustomerTradingPairAlertAsync(customerId, tradingPairId, alertId, alert, cancellationToken);
            
            _logger.LogInformation("Updated customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
            
            return updatedAlert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task DeleteCustomerTradingPairAlertAsync(string customerId, string tradingPairId, string alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
            
            await _customerMarketsClient.DeleteCustomerTradingPairAlertAsync(customerId, tradingPairId, alertId, cancellationToken);
            
            _logger.LogInformation("Deleted customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer trading pair alert {AlertId} for trading pair {TradingPairId} and customer {CustomerId}", alertId, tradingPairId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CustomerMarketSubscription>> GetCustomerMarketSubscriptionsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer market subscriptions for customer {CustomerId}", customerId);
            
            var subscriptions = await _customerMarketsClient.GetCustomerMarketSubscriptionsAsync(customerId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} customer market subscriptions for customer {CustomerId}", subscriptions.Count(), customerId);
            
            return subscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer market subscriptions for customer {CustomerId}", customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerMarketSubscription> GetCustomerMarketSubscriptionAsync(string customerId, string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            
            var subscription = await _customerMarketsClient.GetCustomerMarketSubscriptionAsync(customerId, subscriptionId, cancellationToken);
            
            _logger.LogInformation("Retrieved customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<CustomerMarketSubscriptionPlan>> GetSubscriptionPlansAsync(string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subscription plans for market {MarketId}", marketId);
            
            var plans = await _customerMarketsClient.GetSubscriptionPlansAsync(marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} subscription plans for market {MarketId}", plans.Count(), marketId);
            
            return plans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans for market {MarketId}", marketId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerMarketSubscriptionPlan> GetSubscriptionPlanAsync(string marketId, string planId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subscription plan {PlanId} for market {MarketId}", planId, marketId);
            
            var plan = await _customerMarketsClient.GetSubscriptionPlanAsync(marketId, planId, cancellationToken);
            
            _logger.LogInformation("Retrieved subscription plan {PlanId} for market {MarketId}", planId, marketId);
            
            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plan {PlanId} for market {MarketId}", planId, marketId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerMarketSubscription> CreateCustomerMarketSubscriptionAsync(string customerId, CustomerMarketSubscription subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating customer market subscription for customer {CustomerId} and market {MarketId}", customerId, subscription.MarketId);
            
            var createdSubscription = await _customerMarketsClient.CreateCustomerMarketSubscriptionAsync(customerId, subscription, cancellationToken);
            
            _logger.LogInformation("Created customer market subscription {SubscriptionId} for customer {CustomerId} and market {MarketId}", createdSubscription.Id, customerId, subscription.MarketId);
            
            return createdSubscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer market subscription for customer {CustomerId} and market {MarketId}", customerId, subscription.MarketId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<CustomerMarketSubscription> CancelCustomerMarketSubscriptionAsync(string customerId, string subscriptionId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            
            var cancelledSubscription = await _customerMarketsClient.CancelCustomerMarketSubscriptionAsync(customerId, subscriptionId, reason, cancellationToken);
            
            _logger.LogInformation("Cancelled customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            
            return cancelledSubscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling customer market subscription {SubscriptionId} for customer {CustomerId}", subscriptionId, customerId);
            throw;
        }
    }
}
