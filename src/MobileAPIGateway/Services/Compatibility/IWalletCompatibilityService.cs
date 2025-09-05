using MobileAPIGateway.Models.Compatibility.Wallet;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for wallet compatibility service
/// </summary>
public interface IWalletCompatibilityService
{
    /// <summary>
    /// Gets the wallet balance
    /// </summary>
    /// <param name="request">The wallet balance request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The wallet balance response</returns>
    Task<WalletBalanceResponse> GetWalletBalanceAsync(WalletBalanceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet transactions
    /// </summary>
    /// <param name="request">The wallet transaction request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The wallet transaction response</returns>
    Task<WalletTransactionResponse> GetWalletTransactionsAsync(WalletTransactionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transfers funds between wallets
    /// </summary>
    /// <param name="request">The transfer request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The transfer response</returns>
    Task<TransferCompatibilityResponse> TransferAsync(TransferCompatibilityRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user coin metrics
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user coin metrics response</returns>
    Task<UserCoinMetricsResponse> GetUserCoinMetricsAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current user transactions
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user transactions response</returns>
    Task<UserTransactionsResponse> GetCurrentUserTransactionsAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user transactions with pagination
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to return</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user transactions response</returns>
    Task<UserTransactionsResponse> GetUserTransactionsAsync(string emailAddress, string clientIpAddress, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an external wallet
    /// </summary>
    /// <param name="request">The add external wallet request</param>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The add external wallet response</returns>
    Task<AddExternalWalletResponse> AddExternalWalletAsync(AddExternalWalletRequest request, string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
}
