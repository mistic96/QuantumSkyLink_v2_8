//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.Wallet;
//using MobileAPIGateway.Models.Wallet;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the wallet compatibility service
///// </summary>
//public class WalletCompatibilityService : IWalletCompatibilityService
//{
//    private readonly IWalletService _walletService;
//    private readonly ILogger<WalletCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="WalletCompatibilityService"/> class
//    /// </summary>
//    /// <param name="walletService">The wallet service</param>
//    /// <param name="logger">The logger</param>
//    public WalletCompatibilityService(IWalletService walletService, ILogger<WalletCompatibilityService> logger)
//    {
//        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<WalletBalanceResponse> GetWalletBalanceAsync(WalletBalanceRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Getting wallet balance for email: {Email}, wallet address: {WalletAddress}, currency code: {CurrencyCode}", 
//                request.Email, request.WalletAddress, request.CurrencyCode);
            
//            // Map to the new model
//            var newRequest = new Models.Wallet.WalletBalance
//            {
//                WalletAddress = request.WalletAddress ?? string.Empty,
//                CurrencyCode = request.CurrencyCode ?? string.Empty
//            };
            
//            // Call the new service
//            var balance = await _walletService.GetWalletBalanceAsync(request.Email, newRequest, cancellationToken);
            
//            // Map to the compatibility response
//            return new WalletBalanceResponse
//            {
//                IsSuccessful = true,
//                Message = "Wallet balance retrieved successfully",
//                WalletAddress = balance.WalletAddress,
//                CurrencyCode = balance.CurrencyCode,
//                AvailableBalance = balance.AvailableBalance,
//                PendingBalance = balance.PendingBalance,
//                TotalBalance = balance.TotalBalance,
//                BalanceUSD = balance.BalanceUSD,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting wallet balance for email: {Email}, wallet address: {WalletAddress}, currency code: {CurrencyCode}", 
//                request.Email, request.WalletAddress, request.CurrencyCode);
            
//            return new WalletBalanceResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to get wallet balance: " + ex.Message,
//                WalletAddress = request.WalletAddress ?? string.Empty,
//                CurrencyCode = request.CurrencyCode ?? string.Empty,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<WalletTransactionResponse> GetWalletTransactionsAsync(WalletTransactionRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Getting wallet transactions for email: {Email}, wallet address: {WalletAddress}, currency code: {CurrencyCode}", 
//                request.Email, request.WalletAddress, request.CurrencyCode);
            
//            // Map to the new model
//            var newRequest = new Models.Wallet.WalletTransaction
//            {
//                WalletAddress = request.WalletAddress ?? string.Empty,
//                CurrencyCode = request.CurrencyCode ?? string.Empty,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                StartDate = request.StartDate,
//                EndDate = request.EndDate,
//                TransactionType = request.TransactionType
//            };
            
//            // Call the new service
//            var transactions = await _walletService.GetWalletTransactionsAsync(request.Email, newRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var response = new WalletTransactionResponse
//            {
//                IsSuccessful = true,
//                Message = "Wallet transactions retrieved successfully",
//                WalletAddress = request.WalletAddress ?? string.Empty,
//                CurrencyCode = request.CurrencyCode ?? string.Empty,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                TotalCount = transactions.Count,
//                Timestamp = DateTime.UtcNow
//            };
            
//            // Map each transaction
//            foreach (var transaction in transactions)
//            {
//                response.Transactions.Add(new WalletTransactionItem
//                {
//                    TransactionId = transaction.TransactionId,
//                    TransactionType = transaction.TransactionType,
//                    Amount = transaction.Amount,
//                    CurrencyCode = transaction.CurrencyCode,
//                    AmountUSD = transaction.AmountUSD,
//                    Status = transaction.Status,
//                    Timestamp = transaction.Timestamp,
//                    SourceAddress = transaction.SourceAddress,
//                    DestinationAddress = transaction.DestinationAddress,
//                    Fee = transaction.Fee,
//                    FeeCurrencyCode = transaction.FeeCurrencyCode,
//                    Description = transaction.Description
//                });
//            }
            
//            return response;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting wallet transactions for email: {Email}, wallet address: {WalletAddress}, currency code: {CurrencyCode}", 
//                request.Email, request.WalletAddress, request.CurrencyCode);
            
//            return new WalletTransactionResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to get wallet transactions: " + ex.Message,
//                WalletAddress = request.WalletAddress ?? string.Empty,
//                CurrencyCode = request.CurrencyCode ?? string.Empty,
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<TransferCompatibilityResponse> TransferAsync(TransferCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Transferring funds for email: {Email}, from: {SourceWalletAddress}, to: {DestinationWalletAddress}, amount: {Amount} {CurrencyCode}", 
//                request.Email, request.SourceWalletAddress, request.DestinationWalletAddress, request.Amount, request.CurrencyCode);
            
//            // Map to the new model
//            var newRequest = new Models.Wallet.TransferRequest
//            {
//                SourceWalletAddress = request.SourceWalletAddress,
//                DestinationWalletAddress = request.DestinationWalletAddress,
//                Amount = request.Amount,
//                CurrencyCode = request.CurrencyCode,
//                Description = request.Description,
//                Memo = request.Memo,
//                Tag = request.Tag
//            };
            
//            // Call the new service
//            var result = await _walletService.TransferAsync(request.Email, newRequest, cancellationToken);
            
//            // Map to the compatibility response
//            return new TransferCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Transfer completed successfully",
//                TransactionId = result.TransactionId,
//                SourceWalletAddress = result.SourceWalletAddress,
//                DestinationWalletAddress = result.DestinationWalletAddress,
//                Amount = result.Amount,
//                CurrencyCode = result.CurrencyCode,
//                Fee = result.Fee,
//                FeeCurrencyCode = result.FeeCurrencyCode,
//                Status = result.Status,
//                Timestamp = DateTime.UtcNow
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error transferring funds for email: {Email}, from: {SourceWalletAddress}, to: {DestinationWalletAddress}, amount: {Amount} {CurrencyCode}", 
//                request.Email, request.SourceWalletAddress, request.DestinationWalletAddress, request.Amount, request.CurrencyCode);
            
//            return new TransferCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to transfer funds: " + ex.Message,
//                SourceWalletAddress = request.SourceWalletAddress,
//                DestinationWalletAddress = request.DestinationWalletAddress,
//                Amount = request.Amount,
//                CurrencyCode = request.CurrencyCode,
//                Status = "Failed",
//                Timestamp = DateTime.UtcNow
//            };
//        }
//    }
//}
