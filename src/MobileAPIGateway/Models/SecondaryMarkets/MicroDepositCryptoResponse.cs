using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Micro deposit crypto response
/// </summary>
public sealed class MicroDepositCryptoResponse
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the request ID
    /// </summary>
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// Gets or sets the amount in USD
    /// </summary>
    public decimal AmountInUsd { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public CloudCheckoutCurrency Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the cart type
    /// </summary>
    public CloudCartType CartType { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction set ID
    /// </summary>
    public Guid TransactionSetId { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public CardChargeCryptoRequestStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the payment address
    /// </summary>
    public PaymentAddress? PaymentAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the fees total in USD
    /// </summary>
    public decimal FeesTotalInUsd { get; set; }
    
    /// <summary>
    /// Gets or sets the total charge amount
    /// </summary>
    public decimal TotalChargeAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the request type
    /// </summary>
    public OrderRequestType RequestType { get; set; }
    
    /// <summary>
    /// Gets or sets the vendor payment code
    /// </summary>
    public string? VendorPaymentCode { get; set; }
    
    /// <summary>
    /// Gets or sets the interactive payment flow URL
    /// </summary>
    public string? InteractivePaymentFlowUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the vendor
    /// </summary>
    public VendorType? Vendor { get; set; }
    
    /// <summary>
    /// Gets or sets the vendor charge ID
    /// </summary>
    public string? VendorChargeId { get; set; }
    
    /// <summary>
    /// Gets or sets the fees
    /// </summary>
    public List<MicroServiceChargeRequestFee> Fees { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the error
    /// </summary>
    public string? Error { get; set; }
}
