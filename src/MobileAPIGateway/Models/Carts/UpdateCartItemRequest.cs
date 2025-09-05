namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a request to update an existing cart item
/// </summary>
public class UpdateCartItemRequest
{
    /// <summary>
    /// Gets or sets the product quantity
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the item notes
    /// </summary>
    public string Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the item options (JSON string)
    /// </summary>
    public string Options { get; set; }
}
