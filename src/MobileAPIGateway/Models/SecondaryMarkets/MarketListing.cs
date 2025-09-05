using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Market listing
/// </summary>
public sealed class MarketListing
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the seller ID
    /// </summary>
    public Guid SellerId { get; set; }
    
    /// <summary>
    /// Gets or sets the asset
    /// </summary>
    public CloudCheckoutCurrency Asset { get; set; }
    
    /// <summary>
    /// Gets or sets the market symbol
    /// </summary>
    public string? MarketSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets the main image
    /// </summary>
    public Image? MainImage { get; set; }
    
    /// <summary>
    /// Gets or sets the image gallery
    /// </summary>
    public List<Image>? ImageGallery { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal? Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Gets or sets the starting price
    /// </summary>
    public decimal? StartingPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum price
    /// </summary>
    public decimal? MaximumPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum price
    /// </summary>
    public decimal? MinimumPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the pricing model
    /// </summary>
    public PricingModel? PricingModel { get; set; }
    
    /// <summary>
    /// Gets or sets the margin model type
    /// </summary>
    public MarginModelType? MarginModelType { get; set; }
    
    /// <summary>
    /// Gets or sets the margin
    /// </summary>
    public decimal? Margin { get; set; }
    
    /// <summary>
    /// Gets or sets the unit
    /// </summary>
    public decimal? Unit { get; set; }
    
    /// <summary>
    /// Gets or sets the current rate
    /// </summary>
    public LedgerMarketExchangeRate? CurrentRate { get; set; }
    
    /// <summary>
    /// Gets or sets the accepted payment methods
    /// </summary>
    public List<AcceptedPaymentMethod>? AcceptedPaymentMethods { get; set; }
    
    /// <summary>
    /// Gets or sets the requested listing date
    /// </summary>
    public DateTimeOffset? RequestedListingDate { get; set; }
    
    /// <summary>
    /// Gets or sets the listing date
    /// </summary>
    public DateTimeOffset ListingDate { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration date
    /// </summary>
    public DateTimeOffset ExpirationDate { get; set; }
    
    /// <summary>
    /// Gets or sets the payment source
    /// </summary>
    public MarketPaymentMethod PaymentSource { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the listing is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets the fees
    /// </summary>
    public List<CloudFee>? Fees { get; set; }
    
    /// <summary>
    /// Gets or sets the message type
    /// </summary>
    public ListingMessageType MessageType { get; set; }
    
    /// <summary>
    /// Gets or sets the time remaining
    /// </summary>
    public TimeSpan TimeRemaining { get; set; }
    
    /// <summary>
    /// Gets or sets the total price in USD
    /// </summary>
    public decimal TotalPriceInUsd { get; set; }
    
    /// <summary>
    /// Gets or sets the customer
    /// </summary>
    public Customer? Customer { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit request
    /// </summary>
    public MicroDepositCryptoResponse? DepositRequest { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated profit
    /// </summary>
    public EstimatedProfitProjection? EstimatedProfit { get; set; }
    
    /// <summary>
    /// Gets or sets the total fees
    /// </summary>
    public decimal TotalFees { get; set; }
    
    /// <summary>
    /// Gets or sets the sales
    /// </summary>
    public List<ListingSale> Sales { get; set; } = new();
}
