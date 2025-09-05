namespace MobileAPIGateway.Models.Compatibility.Wallet;

/// <summary>
/// Response model for user coin metrics
/// </summary>
public class UserCoinMetricsResponse
{
    /// <summary>
    /// Total balance in base currency
    /// </summary>
    public decimal TotalBalance { get; set; }

    /// <summary>
    /// Total balance in USD
    /// </summary>
    public decimal TotalBalanceUSD { get; set; }

    /// <summary>
    /// Portfolio change in 24 hours
    /// </summary>
    public decimal PortfolioChange24h { get; set; }

    /// <summary>
    /// Portfolio change percentage in 24 hours
    /// </summary>
    public decimal PortfolioChangePercent24h { get; set; }

    /// <summary>
    /// List of coins in the portfolio
    /// </summary>
    public List<CoinMetric> Coins { get; set; } = new();
}

/// <summary>
/// Individual coin metric
/// </summary>
public class CoinMetric
{
    /// <summary>
    /// Cryptocurrency symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Balance amount
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Balance in USD
    /// </summary>
    public decimal BalanceUSD { get; set; }

    /// <summary>
    /// Current price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 24 hour change
    /// </summary>
    public decimal Change24h { get; set; }

    /// <summary>
    /// 24 hour change percentage
    /// </summary>
    public decimal ChangePercent24h { get; set; }
}
