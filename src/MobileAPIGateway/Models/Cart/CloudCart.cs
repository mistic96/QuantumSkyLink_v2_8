using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Cloud cart
/// </summary>
public sealed class CloudCart
{
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public Guid CartId { get; set; }
    
    /// <summary>
    /// Gets or sets the cart owner ID
    /// </summary>
    public string? CartOwnerId { get; set; }
    
    /// <summary>
    /// Gets or sets the date created
    /// </summary>
    public DateTimeOffset? DateCreated { get; set; }
    
    /// <summary>
    /// Gets or sets the items
    /// </summary>
    public List<CartItem>? Items { get; set; }
    
    /// <summary>
    /// Gets or sets the ad hoc item
    /// </summary>
    public QuickBuyItem? AdHocItem { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public CloudCartCheckedOutStatus? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the item discount
    /// </summary>
    public decimal? ItemDiscount { get; set; }
    
    /// <summary>
    /// Gets or sets the discount code
    /// </summary>
    public string? DiscountCode { get; set; }
    
    /// <summary>
    /// Gets or sets the discount code type
    /// </summary>
    public DiscountCodeType? DiscountCodeType { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public CloudCartType? Type { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether MFA is required
    /// </summary>
    public bool IsMfaRequired { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether KYC is required for completion
    /// </summary>
    public bool IsKycRequiredCompletion { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum cart amount
    /// </summary>
    public decimal MinimumCartAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the total discount
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    /// <summary>
    /// Gets or sets the number of items
    /// </summary>
    public decimal NumberOfItems { get; set; }
    
    /// <summary>
    /// Gets or sets the total price
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the messages
    /// </summary>
    public List<CartMessages>? Messages { get; set; }
    
    /// <summary>
    /// Gets or sets the expire date UTC
    /// </summary>
    public DateTimeOffset? ExpireDateUtc { get; set; }
    
    /// <summary>
    /// Gets or sets the summary
    /// </summary>
    public CloudCartSummarySection? Summary { get; set; }
}
