namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Wallet assets
/// </summary>
public sealed class WalletAssets
{
    /// <summary>
    /// Gets or sets the tokens
    /// </summary>
    public List<DynamicUserCoin> Tokens { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the digital currencies
    /// </summary>
    public List<AssetBackedCurrency> DigitalCurrencies { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the total coins
    /// </summary>
    public decimal TotalCoins { get; set; }
    
    /// <summary>
    /// Gets or sets the total digital currencies
    /// </summary>
    public decimal TotalDigitalCurrencies { get; set; }
    
    /// <summary>
    /// Gets or sets the highest team sale offer
    /// </summary>
    public object? HighestTeamSaleOffer { get; set; }
    
    /// <summary>
    /// Gets or sets the total cost
    /// </summary>
    public decimal TotalCost { get; set; } = 0.00m;
    
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
    public decimal TotalMarketPrice { get; set; } = 0.00m;
    
    /// <summary>
    /// Gets or sets the teams
    /// </summary>
    public List<PublicTeamExtendedPrice> Teams { get; set; } = [];
}
