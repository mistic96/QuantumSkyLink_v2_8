namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Interface for wallet balance operations
/// </summary>
public interface IWalletBalanceService
{
    /// <summary>
    /// Gets the wallet balance for a user in a specific currency
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currency">The currency code</param>
    /// <returns>The wallet balance</returns>
    Task<decimal> GetBalanceAsync(string userId, string currency);

    /// <summary>
    /// Gets detailed wallet information including available and pending balances
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currency">The currency code</param>
    /// <returns>Detailed wallet balance information</returns>
    Task<WalletBalanceInfo> GetBalanceInfoAsync(string userId, string currency);

    /// <summary>
    /// Checks if user has sufficient balance for a transaction
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currency">The currency code</param>
    /// <param name="amount">The required amount</param>
    /// <returns>True if sufficient balance exists</returns>
    Task<bool> HasSufficientBalanceAsync(string userId, string currency, decimal amount);

    /// <summary>
    /// Updates the wallet balance (for testing or admin purposes)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currency">The currency code</param>
    /// <param name="amount">The amount to add (positive) or subtract (negative)</param>
    /// <param name="reason">The reason for the balance update</param>
    /// <returns>The updated balance</returns>
    Task<decimal> UpdateBalanceAsync(string userId, string currency, decimal amount, string reason);
}

/// <summary>
/// Detailed wallet balance information
/// </summary>
public class WalletBalanceInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal LockedBalance { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsActive { get; set; }
}