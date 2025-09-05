namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Pricing strategy enum
/// </summary>
public enum PricingStrategy
{
    /// <summary>
    /// Fixed price strategy.
    /// </summary>
    Fixed,

    /// <summary>
    /// Dynamic price strategy (price changes based on market conditions).
    /// </summary>
    Dynamic,

    /// <summary>
    /// Tiered price strategy (price changes based on quantity).
    /// </summary>
    Tiered,

    /// <summary>
    /// Bulk price strategy (discounted price for bulk purchases).
    /// </summary>
    Bulk,

    /// <summary>
    /// Unit price strategy (price per unit).
    /// </summary>
    Unit,

    /// <summary>
    /// Margin-based price strategy (price with added margin).
    /// </summary>
    Margin
}
