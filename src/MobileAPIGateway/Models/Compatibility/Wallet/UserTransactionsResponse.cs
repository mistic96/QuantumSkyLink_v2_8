namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Response model for user transactions
/// </summary>
public class UserTransactionsResponse
{
    /// <summary>
    /// List of transactions
    /// </summary>
    public List<UserTransaction> Transactions { get; set; } = new();

    /// <summary>
    /// Total count of transactions (for paginated responses)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Whether there are more transactions available
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// Individual user transaction
/// </summary>
public class UserTransaction
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type (Buy, Sell, Transfer, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency symbol
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Price at time of transaction
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Total value of transaction
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Transaction status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Blockchain transaction ID
    /// </summary>
    public string BlockchainTxId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction fees
    /// </summary>
    public decimal Fees { get; set; }
}
