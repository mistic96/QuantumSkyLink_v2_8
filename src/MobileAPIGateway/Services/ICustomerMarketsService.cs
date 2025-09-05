using MobileAPIGateway.Models.CustomerMarkets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Customer markets service interface
/// </summary>
public interface ICustomerMarketsService
{
    /// <summary>
    /// Gets all customer markets
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer markets</returns>
    Task<IEnumerable<CustomerMarket>> GetCustomerMarketsAsync(string customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a customer market by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer market</returns>
    Task<CustomerMarket> GetCustomerMarketAsync(string customerId, string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all customer trading pairs for a market
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer trading pairs</returns>
    Task<IEnumerable<CustomerTradingPair>> GetCustomerTradingPairsAsync(string customerId, string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a customer trading pair by symbol
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer trading pair</returns>
    Task<CustomerTradingPair> GetCustomerTradingPairAsync(string customerId, string symbol, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all customer trading pair alerts
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer trading pair alerts</returns>
    Task<IEnumerable<CustomerTradingPairAlert>> GetCustomerTradingPairAlertsAsync(string customerId, string tradingPairId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a customer trading pair alert
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alert">Customer trading pair alert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer trading pair alert</returns>
    Task<CustomerTradingPairAlert> CreateCustomerTradingPairAlertAsync(string customerId, string tradingPairId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a customer trading pair alert
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="alert">Customer trading pair alert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated customer trading pair alert</returns>
    Task<CustomerTradingPairAlert> UpdateCustomerTradingPairAlertAsync(string customerId, string tradingPairId, string alertId, CustomerTradingPairAlert alert, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a customer trading pair alert
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="tradingPairId">Trading pair ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteCustomerTradingPairAlertAsync(string customerId, string tradingPairId, string alertId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all customer market subscriptions
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer market subscriptions</returns>
    Task<IEnumerable<CustomerMarketSubscription>> GetCustomerMarketSubscriptionsAsync(string customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a customer market subscription by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer market subscription</returns>
    Task<CustomerMarketSubscription> GetCustomerMarketSubscriptionAsync(string customerId, string subscriptionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all subscription plans for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of subscription plans</returns>
    Task<IEnumerable<CustomerMarketSubscriptionPlan>> GetSubscriptionPlansAsync(string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a subscription plan by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="planId">Subscription plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription plan</returns>
    Task<CustomerMarketSubscriptionPlan> GetSubscriptionPlanAsync(string marketId, string planId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a customer market subscription
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="subscription">Customer market subscription</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer market subscription</returns>
    Task<CustomerMarketSubscription> CreateCustomerMarketSubscriptionAsync(string customerId, CustomerMarketSubscription subscription, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels a customer market subscription
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled customer market subscription</returns>
    Task<CustomerMarketSubscription> CancelCustomerMarketSubscriptionAsync(string customerId, string subscriptionId, string reason, CancellationToken cancellationToken = default);
}
