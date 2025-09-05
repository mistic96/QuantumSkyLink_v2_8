using MobileAPIGateway.Authentication;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Services;

/// <summary>
/// Wallet service
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletClient _walletClient;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<WalletService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletService"/> class
    /// </summary>
    /// <param name="walletClient">Wallet client</param>
    /// <param name="userContextAccessor">User context accessor</param>
    /// <param name="logger">Logger</param>
    public WalletService(
        IWalletClient walletClient,
        IUserContextAccessor userContextAccessor,
        ILogger<WalletService> logger)
    {
        _walletClient = walletClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting wallet balances for user {UserId}", userId);
            
            var walletBalances = await _walletClient.GetWalletBalancesAsync(userId, cancellationToken);
            
            _logger.LogInformation("Wallet balances retrieved successfully for user {UserId}", userId);
            
            return walletBalances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet balances for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<WalletBalance>> GetCurrentUserWalletBalancesAsync(CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        return await GetWalletBalancesAsync(userContext.UserId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<WalletBalance> GetWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting wallet balance for wallet {WalletId}", walletId);
            
            var walletBalance = await _walletClient.GetWalletBalanceAsync(walletId, cancellationToken);
            
            _logger.LogInformation("Wallet balance retrieved successfully for wallet {WalletId}", walletId);
            
            return walletBalance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet balance for wallet {WalletId}", walletId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<WalletTransaction>> GetWalletTransactionsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting wallet transactions for user {UserId}", userId);
            
            var walletTransactions = await _walletClient.GetWalletTransactionsAsync(userId, page, pageSize, cancellationToken);
            
            _logger.LogInformation("Wallet transactions retrieved successfully for user {UserId}", userId);
            
            return walletTransactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet transactions for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<WalletTransaction>> GetCurrentUserWalletTransactionsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        return await GetWalletTransactionsAsync(userContext.UserId, page, pageSize, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<WalletTransaction>> GetWalletTransactionsByWalletIdAsync(string walletId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting wallet transactions for wallet {WalletId}", walletId);
            
            var walletTransactions = await _walletClient.GetWalletTransactionsByWalletIdAsync(walletId, page, pageSize, cancellationToken);
            
            _logger.LogInformation("Wallet transactions retrieved successfully for wallet {WalletId}", walletId);
            
            return walletTransactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet transactions for wallet {WalletId}", walletId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<WalletTransaction> GetWalletTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting wallet transaction {TransactionId}", transactionId);
            
            var walletTransaction = await _walletClient.GetWalletTransactionAsync(transactionId, cancellationToken);
            
            _logger.LogInformation("Wallet transaction retrieved successfully for transaction {TransactionId}", transactionId);
            
            return walletTransaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet transaction {TransactionId}", transactionId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Transferring {Amount} {CurrencyCode} from wallet {SourceWalletId} to wallet {DestinationWalletId}", 
                request.Amount, request.CurrencyCode, request.SourceWalletId, request.DestinationWalletId);
            
            var response = await _walletClient.TransferAsync(request, cancellationToken);
            
            _logger.LogInformation("Transfer completed successfully with transaction ID {TransactionId}", response.TransactionId);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring {Amount} {CurrencyCode} from wallet {SourceWalletId} to wallet {DestinationWalletId}", 
                request.Amount, request.CurrencyCode, request.SourceWalletId, request.DestinationWalletId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Withdrawing {Amount} {CurrencyCode} from wallet {WalletId} to address {DestinationAddress} on network {BlockchainNetwork}", 
                request.Amount, request.CurrencyCode, request.WalletId, request.DestinationAddress, request.BlockchainNetwork);
            
            var response = await _walletClient.WithdrawAsync(request, cancellationToken);
            
            _logger.LogInformation("Withdrawal completed successfully with transaction ID {TransactionId}", response.TransactionId);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing {Amount} {CurrencyCode} from wallet {WalletId} to address {DestinationAddress} on network {BlockchainNetwork}", 
                request.Amount, request.CurrencyCode, request.WalletId, request.DestinationAddress, request.BlockchainNetwork);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<DepositResponse> GetDepositAddressAsync(DepositRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting deposit address for wallet {WalletId} for currency {CurrencyCode} on network {BlockchainNetwork}", 
                request.WalletId, request.CurrencyCode, request.BlockchainNetwork);
            
            var response = await _walletClient.GetDepositAddressAsync(request, cancellationToken);
            
            _logger.LogInformation("Deposit address retrieved successfully: {DepositAddress}", response.DepositAddress);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit address for wallet {WalletId} for currency {CurrencyCode} on network {BlockchainNetwork}", 
                request.WalletId, request.CurrencyCode, request.BlockchainNetwork);
            throw;
        }
    }
}
