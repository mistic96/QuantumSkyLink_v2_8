using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Payment address
/// </summary>
public sealed class PaymentAddress
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the address
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public CloudCheckoutCurrency Type { get; set; }
    
    /// <summary>
    /// Gets or sets the expected crypto amount
    /// </summary>
    public decimal ExpectedCryptoAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the rate
    /// </summary>
    public decimal Rate { get; set; }
    
    /// <summary>
    /// Gets or sets the address as QR code
    /// </summary>
    public string? AddressAsQrCode { get; set; }
}
