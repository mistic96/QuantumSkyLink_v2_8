namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// User coin metric
/// </summary>
public sealed class UserCoinMetric
{
    /// <summary>
    /// Gets or sets the user wallet
    /// </summary>
    public WalletExtended? UserWallet { get; set; }
    
    /// <summary>
    /// Gets or sets the user wallet total coins
    /// </summary>
    public decimal UserWalletTotalCoins { get; set; }
    
    /// <summary>
    /// Gets or sets the on hold total coins
    /// </summary>
    public decimal OnHoldTotalCoins { get; set; }
    
    /// <summary>
    /// Gets or sets the user wallet digital currencies
    /// </summary>
    public decimal UserWalletDigitalCurrencies { get; set; }
    
    /// <summary>
    /// Gets or sets the on hold total digital currencies
    /// </summary>
    public decimal OnHoldTotalDigitalCurrencies { get; set; }
    
    /// <summary>
    /// Gets or sets the highest team sale offer
    /// </summary>
    public object? HighestTeamSaleOffer { get; set; }
    
    /// <summary>
    /// Gets or sets the profit loss
    /// </summary>
    public decimal ProfitLoss { get; set; }
    
    /// <summary>
    /// Gets or sets the profit loss percentage
    /// </summary>
    public PercentageFlux? ProfitLossPercentage { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether is profit
    /// </summary>
    public bool IsProfit { get; set; }
    
    /// <summary>
    /// Gets or sets the total market price
    /// </summary>
    public decimal TotalMarketPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the teams
    /// </summary>
    public List<PublicTeamExtendedPrice> Teams { get; set; } = [];
}
