using Refit;
using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Wallet client interface
/// </summary>
public interface IWalletClient
{
    /// <summary>
    /// Gets the wallet balances for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet balances</returns>
    [Get("/api/wallets/users/{userId}/balances")]
    Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet balance for a specific wallet
    /// </summary>
    /// <param name="walletId">Wallet ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet balance</returns>
    [Get("/api/wallets/{walletId}/balance")]
    Task<WalletBalance> GetWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet transactions for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet transactions</returns>
    [Get("/api/wallets/users/{userId}/transactions")]
    Task<IEnumerable<WalletTransaction>> GetWalletTransactionsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet transactions for a specific wallet
    /// </summary>
    /// <param name="walletId">Wallet ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet transactions</returns>
    [Get("/api/wallets/{walletId}/transactions")]
    Task<IEnumerable<WalletTransaction>> GetWalletTransactionsByWalletIdAsync(string walletId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a specific wallet transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet transaction</returns>
    [Get("/api/wallets/transactions/{transactionId}")]
    Task<WalletTransaction> GetWalletTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transfers funds between wallets
    /// </summary>
    /// <param name="request">Transfer request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer response</returns>
    [Post("/api/wallets/transfer")]
    Task<TransferResponse> TransferAsync([Body] TransferRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Withdraws funds from a wallet
    /// </summary>
    /// <param name="request">Withdraw request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Withdraw response</returns>
    [Post("/api/wallets/withdraw")]
    Task<WithdrawResponse> WithdrawAsync([Body] WithdrawRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a deposit address for a wallet
    /// </summary>
    /// <param name="request">Deposit request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit response</returns>
    [Post("/api/wallets/deposit")]
    Task<DepositResponse> GetDepositAddressAsync([Body] DepositRequest request, CancellationToken cancellationToken = default);
}
