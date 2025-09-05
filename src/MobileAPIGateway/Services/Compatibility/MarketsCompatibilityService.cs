//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.Markets;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the markets compatibility service
///// </summary>
//public class MarketsCompatibilityService : IMarketsCompatibilityService
//{
//    private readonly IMarketsService _marketsService;
//    private readonly ILogger<MarketsCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="MarketsCompatibilityService"/> class
//    /// </summary>
//    /// <param name="marketsService">The markets service</param>
//    /// <param name="logger">The logger</param>
//    public MarketsCompatibilityService(IMarketsService marketsService, ILogger<MarketsCompatibilityService> logger)
//    {
//        _marketsService = marketsService ?? throw new ArgumentNullException(nameof(marketsService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<MarketListResponse> GetMarketListAsync(MarketListRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Getting market list with page number: {PageNumber}, page size: {PageSize}, status: {Status}, market type: {MarketType}, currency code: {CurrencyCode}", 
//                request.PageNumber, request.PageSize, request.Status, request.MarketType, request.CurrencyCode);
            
//            // Map to the new model
//            var filter = new Models.Markets.Market
//            {
//                Status = request.Status,
//                MarketType = request.MarketType,
//                BaseCurrencyCode = request.CurrencyCode
//            };
            
//            // Call the new service
//            var markets = await _marketsService.GetMarketsAsync(filter, request.PageNumber, request.PageSize, cancellationToken);
            
//            // Map to the compatibility response
//            var response = new MarketListResponse
//            {
//                IsSuccessful = true,
//                Message = "Market list retrieved successfully",
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                TotalCount = markets.Count,
//                Timestamp = DateTime.UtcNow
//            };
            
//            // Map each market
//            foreach (var market in markets)
//            {
//                response.Markets.Add(new MarketItem
//                {
//                    MarketId = market.Id,
//                    MarketName = market.Name,
//                    Description = market.Description,
//                    MarketType = market.MarketType,
//                    BaseCurrencyCode = market.BaseCurrencyCode,
//                    QuoteCurrencyCode = market.QuoteCurrencyCode,
//                    Status = market.Status,
//                    CurrentPrice = market.CurrentPrice,
//                    High24h = market.High24h,
//                    Low24h = market.Low24h,
//                    Volume24h = market.Volume24h,
//                    PriceChangePercentage24h = market.PriceChangePercentage24h,
//                    MarketCap = market.MarketCap,
//                    TradingPairs = market.TradingPairs?.Select(tp => tp.Symbol).ToList() ?? new List<string>(),
//                    CreatedDate = market.CreatedDate,
//                    LastUpdatedDate = market.LastUpdatedDate
//                });
//            }
            
//            return response;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting market list with page number: {PageNumber}, page size: {PageSize}, status: {Status}, market type: {MarketType}, currency code: {CurrencyCode}", 
//                request.PageNumber, request.PageSize, request.Status, request.MarketType, request.CurrencyCode);
            
//            return new MarketListResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to get market list: " + ex.Message,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<TradingPairResponse> GetTradingPairsAsync(TradingPairRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Getting trading pairs for market ID: {MarketId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, base currency code: {BaseCurrencyCode}, quote currency code: {QuoteCurrencyCode}", 
//                request.MarketId, request.PageNumber, request.PageSize, request.Status, request.BaseCurrencyCode, request.QuoteCurrencyCode);
            
//            // Map to the new model
//            var filter = new Models.Markets.TradingPair
//            {
//                MarketId = request.MarketId,
//                Status = request.Status,
//                BaseCurrencyCode = request.BaseCurrencyCode,
//                QuoteCurrencyCode = request.QuoteCurrencyCode
//            };
            
//            // Call the new service
//            var tradingPairs = await _marketsService.GetTradingPairsAsync(request.MarketId, filter, request.PageNumber, request.PageSize, cancellationToken);
            
//            // Map to the compatibility response
//            var response = new TradingPairResponse
//            {
//                IsSuccessful = true,
//                Message = "Trading pairs retrieved successfully",
//                MarketId = request.MarketId,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                TotalCount = tradingPairs.Count,
//                Timestamp = DateTime.UtcNow
//            };
            
//            // Map each trading pair
//            foreach (var tradingPair in tradingPairs)
//            {
//                response.TradingPairs.Add(new TradingPairItem
//                {
//                    TradingPairId = tradingPair.Id,
//                    Symbol = tradingPair.Symbol,
//                    BaseCurrencyCode = tradingPair.BaseCurrencyCode,
//                    QuoteCurrencyCode = tradingPair.QuoteCurrencyCode,
//                    Status = tradingPair.Status,
//                    CurrentPrice = tradingPair.CurrentPrice,
//                    High24h = tradingPair.High24h,
//                    Low24h = tradingPair.Low24h,
//                    Volume24h = tradingPair.Volume24h,
//                    PriceChangePercentage24h = tradingPair.PriceChangePercentage24h,
//                    MinOrderSize = tradingPair.MinOrderSize,
//                    MaxOrderSize = tradingPair.MaxOrderSize,
//                    PricePrecision = tradingPair.PricePrecision,
//                    QuantityPrecision = tradingPair.QuantityPrecision,
//                    CreatedDate = tradingPair.CreatedDate,
//                    LastUpdatedDate = tradingPair.LastUpdatedDate
//                });
//            }
            
//            return response;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting trading pairs for market ID: {MarketId}, page number: {PageNumber}, page size: {PageSize}, status: {Status}, base currency code: {BaseCurrencyCode}, quote currency code: {QuoteCurrencyCode}", 
//                request.MarketId, request.PageNumber, request.PageSize, request.Status, request.BaseCurrencyCode, request.QuoteCurrencyCode);
            
//            return new TradingPairResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to get trading pairs: " + ex.Message,
//                MarketId = request.MarketId,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
//}
