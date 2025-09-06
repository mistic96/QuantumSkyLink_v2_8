using LiquidStorageCloud.Core.Database;

namespace UnifiedCartService.Models.Entities;

/// <summary>
/// Represents an item in a unified cart
/// </summary>
public class UnifiedCartItem : ISurrealEntity
{
    /// <inheritdoc/>
    public string Namespace => "quantumskylink";
    
    /// <inheritdoc/>
    public string TableName => "cart_items";
    
    /// <inheritdoc/>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <inheritdoc/>
    public bool SolidState { get; set; }
    
    /// <inheritdoc/>
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets the cart ID this item belongs to
    /// </summary>
    public string CartId { get; set; }
    
    /// <summary>
    /// Gets or sets the listing ID from either market
    /// </summary>
    public Guid ListingId { get; set; }
    
    /// <summary>
    /// Gets or sets the market type (Primary or Secondary)
    /// </summary>
    public MarketType MarketType { get; set; }
    
    /// <summary>
    /// Gets or sets the token/asset ID
    /// </summary>
    public string TokenId { get; set; }
    
    /// <summary>
    /// Gets or sets the token/asset name
    /// </summary>
    public string TokenName { get; set; }
    
    /// <summary>
    /// Gets or sets the token/asset symbol
    /// </summary>
    public string TokenSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets the asset type (Token, Crypto, Fiat)
    /// </summary>
    public string AssetType { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the price per unit at time of adding
    /// </summary>
    public decimal PricePerUnit { get; set; }
    
    /// <summary>
    /// Gets or sets the currency for the price
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Gets or sets the total value of this item
    /// </summary>
    public decimal TotalValue => Quantity * PricePerUnit;
    
    /// <summary>
    /// Gets or sets the seller ID (for Secondary Market items)
    /// </summary>
    public Guid? SellerId { get; set; }
    
    /// <summary>
    /// Gets or sets the seller name (for Secondary Market items)
    /// </summary>
    public string? SellerName { get; set; }
    
    /// <summary>
    /// Gets or sets whether the seller is verified (Secondary Market)
    /// </summary>
    public bool? SellerIsVerified { get; set; }
    
    /// <summary>
    /// Gets or sets whether to use escrow (Secondary Market only)
    /// </summary>
    public bool UseEscrow { get; set; }
    
    /// <summary>
    /// Gets or sets when the item was added to cart
    /// </summary>
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets any special instructions or notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Gets or sets additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets whether this item has been validated
    /// </summary>
    public bool IsValidated { get; set; }
    
    /// <summary>
    /// Gets or sets the last validation timestamp
    /// </summary>
    public DateTimeOffset? LastValidatedAt { get; set; }
}

/// <summary>
/// Market type enumeration
/// </summary>
public enum MarketType
{
    /// <summary>
    /// Primary market - Company direct sales
    /// </summary>
    Primary = 1,
    
    /// <summary>
    /// Secondary market - Peer-to-peer trading
    /// </summary>
    Secondary = 2
}