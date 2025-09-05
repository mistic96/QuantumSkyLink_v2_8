//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.CustomerMarkets;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the customer markets compatibility service
///// </summary>
//public class CustomerMarketsCompatibilityService : ICustomerMarketsCompatibilityService
//{
//    private readonly ICustomerMarketsService _customerMarketsService;
//    private readonly ILogger<CustomerMarketsCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="CustomerMarketsCompatibilityService"/> class
//    /// </summary>
//    /// <param name="customerMarketsService">The customer markets service</param>
//    /// <param name="logger">The logger</param>
//    public CustomerMarketsCompatibilityService(ICustomerMarketsService customerMarketsService, ILogger<CustomerMarketsCompatibilityService> logger)
//    {
//        _customerMarketsService = customerMarketsService ?? throw new ArgumentNullException(nameof(customerMarketsService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    ///// <inheritdoc />
//    //public async Task<CustomerMarketListResponse> GetCustomerMarketListAsync(CustomerMarketListRequest request, CancellationToken cancellationToken = default)
//    //{
//    //    try
//    //    {
//    //        _logger.LogInformation("Getting customer market list for customer ID: {CustomerId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, market type: {MarketType}, currency code: {CurrencyCode}", 
//    //            request.CustomerId, request.PageNumber, request.PageSize, request.Status, request.MarketType, request.CurrencyCode);
            
//    //        // Map to the new model
//    //        var filter = new Models.CustomerMarkets.CustomerMarket
//    //        {
//    //            CustomerId = request.CustomerId,
//    //            Status = request.Status,
//    //            MarketType = request.MarketType,
//    //            BaseCurrencyCode = request.CurrencyCode
//    //        };
            
//    //        // Call the new service
//    //        var customerMarkets = await _customerMarketsService.GetCustomerMarketsAsync(request.CustomerId, filter, request.PageNumber, request.PageSize, cancellationToken);
            
//    //        // Map to the compatibility response
//    //        var response = new CustomerMarketListResponse
//    //        {
//    //            IsSuccessful = true,
//    //            Message = "Customer market list retrieved successfully",
//    //            CustomerId = request.CustomerId,
//    //            PageNumber = request.PageNumber,
//    //            PageSize = request.PageSize,
//    //            TotalCount = customerMarkets.Count,
//    //            Timestamp = DateTime.UtcNow
//    //        };
            
//    //        // Map each customer market
//    //        foreach (var customerMarket in customerMarkets)
//    //        {
//    //            response.CustomerMarkets.Add(new CustomerMarketItem
//    //            {
//    //                CustomerMarketId = customerMarket.Id,
//    //                MarketId = customerMarket.MarketId,
//    //                MarketName = customerMarket.MarketName,
//    //                Description = customerMarket.Description,
//    //                MarketType = customerMarket.MarketType,
//    //                BaseCurrencyCode = customerMarket.BaseCurrencyCode,
//    //                QuoteCurrencyCode = customerMarket.QuoteCurrencyCode,
//    //                Status = customerMarket.Status,
//    //                CurrentPrice = customerMarket.CurrentPrice,
//    //                High24h = customerMarket.High24h,
//    //                Low24h = customerMarket.Low24h,
//    //                Volume24h = customerMarket.Volume24h,
//    //                PriceChangePercentage24h = customerMarket.PriceChangePercentage24h,
//    //                MarketCap = customerMarket.MarketCap,
//    //                TradingPairs = customerMarket.TradingPairs?.Select(tp => tp.Symbol).ToList() ?? new List<string>(),
//    //                IsFavorite = customerMarket.IsFavorite,
//    //                SubscriptionId = customerMarket.Subscription?.Id,
//    //                SubscriptionPlanId = customerMarket.Subscription?.PlanId,
//    //                SubscriptionPlanName = customerMarket.Subscription?.PlanName,
//    //                SubscriptionStatus = customerMarket.Subscription?.Status,
//    //                SubscriptionStartDate = customerMarket.Subscription?.StartDate,
//    //                SubscriptionEndDate = customerMarket.Subscription?.EndDate,
//    //                CreatedDate = customerMarket.CreatedDate,
//    //                LastUpdatedDate = customerMarket.LastUpdatedDate
//    //            });
//    //        }
            
//    //        return response;
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Error getting customer market list for customer ID: {CustomerId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, market type: {MarketType}, currency code: {CurrencyCode}", 
//    //            request.CustomerId, request.PageNumber, request.PageSize, request.Status, request.MarketType, request.CurrencyCode);
            
//    //        return new CustomerMarketListResponse
//    //        {
//    //            IsSuccessful = false,
//    //            Message = "Failed to get customer market list: " + ex.Message,
//    //            CustomerId = request.CustomerId,
//    //            PageNumber = request.PageNumber,
//    //            PageSize = request.PageSize,
//    //            Timestamp = DateTime.UtcNow
//    //        };
//    //    }
//    //}
    
//    /// <inheritdoc />
//    //public async Task<CustomerTradingPairResponse> GetCustomerTradingPairsAsync(CustomerTradingPairRequest request, CancellationToken cancellationToken = default)
//    //{
//    //    try
//    //    {
//    //        _logger.LogInformation("Getting customer trading pairs for customer ID: {CustomerId}, customer market ID: {CustomerMarketId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, base currency code: {BaseCurrencyCode}, quote currency code: {QuoteCurrencyCode}", 
//    //            request.CustomerId, request.CustomerMarketId, request.PageNumber, request.PageSize, request.Status, request.BaseCurrencyCode, request.QuoteCurrencyCode);
            
//    //        // Map to the new model
//    //        var filter = new Models.CustomerMarkets.CustomerTradingPair
//    //        {
//    //            CustomerId = request.CustomerId,
//    //            CustomerMarketId = request.CustomerMarketId,
//    //            Status = request.Status,
//    //            BaseCurrencyCode = request.BaseCurrencyCode,
//    //            QuoteCurrencyCode = request.QuoteCurrencyCode
//    //        };
            
//    //        // Call the new service
//    //        var customerTradingPairs = await _customerMarketsService.GetCustomerTradingPairsAsync(request.CustomerId, request.CustomerMarketId, filter, request.PageNumber, request.PageSize, cancellationToken);
            
//    //        // Map to the compatibility response
//    //        var response = new CustomerTradingPairResponse
//    //        {
//    //            IsSuccessful = true,
//    //            Message = "Customer trading pairs retrieved successfully",
//    //            CustomerId = request.CustomerId,
//    //            CustomerMarketId = request.CustomerMarketId,
//    //            PageNumber = request.PageNumber,
//    //            PageSize = request.PageSize,
//    //            TotalCount = customerTradingPairs.Count,
//    //            Timestamp = DateTime.UtcNow
//    //        };
            
//    //        // Map each customer trading pair
//    //        foreach (var customerTradingPair in customerTradingPairs)
//    //        {
//    //            response.CustomerTradingPairs.Add(new CustomerTradingPairItem
//    //            {
//    //                CustomerTradingPairId = customerTradingPair.Id,
//    //                TradingPairId = customerTradingPair.TradingPairId,
//    //                Symbol = customerTradingPair.Symbol,
//    //                BaseCurrencyCode = customerTradingPair.BaseCurrencyCode,
//    //                QuoteCurrencyCode = customerTradingPair.QuoteCurrencyCode,
//    //                Status = customerTradingPair.Status,
//    //                CurrentPrice = customerTradingPair.CurrentPrice,
//    //                High24h = customerTradingPair.High24h,
//    //                Low24h = customerTradingPair.Low24h,
//    //                Volume24h = customerTradingPair.Volume24h,
//    //                PriceChangePercentage24h = customerTradingPair.PriceChangePercentage24h,
//    //                MinOrderSize = customerTradingPair.MinOrderSize,
//    //                MaxOrderSize = customerTradingPair.MaxOrderSize,
//    //                PricePrecision = customerTradingPair.PricePrecision,
//    //                QuantityPrecision = customerTradingPair.QuantityPrecision,
//    //                IsFavorite = customerTradingPair.IsFavorite,
//    //                //AlertId = customerTradingPair.Alert?.Id,
//    //                //AlertPrice = customerTradingPair.Alert?.Price,
//    //                //AlertCondition = customerTradingPair.Alert?.Condition,
//    //                //AlertStatus = customerTradingPair.Alert?.Status,
//    //                CreatedDate = customerTradingPair.CreatedDate,
//    //                LastUpdatedDate = customerTradingPair.LastUpdatedDate
//    //            });
//    //        }
            
//    //        return response;
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Error getting customer trading pairs for customer ID: {CustomerId}, customer market ID: {CustomerMarketId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, base currency code: {BaseCurrencyCode}, quote currency code: {QuoteCurrencyCode}", 
//    //            request.CustomerId, request.CustomerMarketId, request.PageNumber, request.PageSize, request.Status, request.BaseCurrencyCode, request.QuoteCurrencyCode);
            
//    //        return new CustomerTradingPairResponse
//    //        {
//    //            IsSuccessful = false,
//    //            Message = "Failed to get customer trading pairs: " + ex.Message,
//    //            CustomerId = request.CustomerId,
//    //            CustomerMarketId = request.CustomerMarketId,
//    //            PageNumber = request.PageNumber,
//    //            PageSize = request.PageSize,
//    //            Timestamp = DateTime.UtcNow
//    //        };
//    //    }
//    //}
//}
