using System.ComponentModel.DataAnnotations;
using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index profile
/// </summary>
public sealed class TokenIndexProfile
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the index ID
    /// </summary>
    public Guid IndexId { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the logo
    /// </summary>
    public string? Logo { get; set; }
    
    /// <summary>
    /// Gets or sets the base price
    /// </summary>
    public decimal? BasePrice { get; set; }
    
    /// <summary>
    /// Gets or sets the current price
    /// </summary>
    public decimal? CurrentPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the market symbol
    /// </summary>
    public string? MarketSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets the activation date
    /// </summary>
    public DateTime? ActivationDate { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the token is contracted
    /// </summary>
    public bool? IsContracted { get; set; }
    
    /// <summary>
    /// Gets or sets the liquidity open date
    /// </summary>
    public DateTime? LiquidityOpenDate { get; set; }
    
    /// <summary>
    /// Gets or sets the available amount
    /// </summary>
    public decimal Available { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum supply
    /// </summary>
    public decimal? MaxSupply { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the token is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the retirement date
    /// </summary>
    public DateTime? RetirementDate { get; set; }
    
    /// <summary>
    /// Gets or sets the exchange rate
    /// </summary>
    public MarketExchangeRate? ExchangeRate { get; set; }
    
    /// <summary>
    /// Gets or sets the smallest unit
    /// </summary>
    public decimal SmallestUnit { get; set; }
    
    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public TokenType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the link type
    /// </summary>
    public MarketAsset LinkType { get; set; } = MarketAsset.None;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public TokenIndexDescription? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the key features
    /// </summary>
    public List<TokenIndexKeyFeatures>? KeyFeatures { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether milestone is enabled
    /// </summary>
    public bool? IsMileStoneEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the milestones
    /// </summary>
    public List<TokenIndexMileStone>? MileStones { get; set; }
    
    /// <summary>
    /// Gets or sets the gallery
    /// </summary>
    public List<TokenIndexImageGallery>? Gallery { get; set; }
    
    /// <summary>
    /// Gets or sets the token tag link
    /// </summary>
    [StringLength(64)]
    public string? TokenTagLink { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the token is a secondary cover product
    /// </summary>
    public bool? IsSecondaryCoverProduct { get; set; }
}
