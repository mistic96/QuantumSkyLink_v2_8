using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Cart messages
/// </summary>
public sealed class CartMessages
{
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public CartMessageTypes Type { get; set; }
}
