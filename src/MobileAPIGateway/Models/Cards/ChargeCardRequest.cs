using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cards;

/// <summary>
/// Charge card request
/// </summary>
public sealed class ChargeCardRequest
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the request ID
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the application ID
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public CloudCheckoutCurrency Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the request date
    /// </summary>
    public DateTime? RequestDate { get; set; }
    
    /// <summary>
    /// Gets or sets the response date
    /// </summary>
    public DateTime? ResponseDate { get; set; }
    
    /// <summary>
    /// Gets or sets the payment status
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the blockchain deposit ID
    /// </summary>
    public string BlockChainDepositId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the receipt number
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the receipt URL
    /// </summary>
    public string ReceiptUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public ChargeCardRequestStatus Status { get; set; } = ChargeCardRequestStatus.Pending;
    
    /// <summary>
    /// Gets or sets the fees
    /// </summary>
    public List<ChargeRequestServiceFee> Fees { get; set; } = new List<ChargeRequestServiceFee>();
    
    /// <summary>
    /// Gets or sets the team revenues
    /// </summary>
    public List<TeamRevenue> TeamRevenues { get; set; } = new List<TeamRevenue>();
    
    /// <summary>
    /// Gets or sets the sales management ID
    /// </summary>
    public long SalesManagementId { get; set; }
    
    /// <summary>
    /// Gets the total
    /// </summary>
    public decimal Total => decimal.Round(Amount + Fees.Sum(a => a.Amount), 2);
    
    /// <summary>
    /// Gets or sets the error
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the vendor
    /// </summary>
    public VendorType? Vendor { get; set; }
    
    /// <summary>
    /// Gets or sets the network interface
    /// </summary>
    public NetworkInterfaces NetworkInterface { get; set; }
    
    /// <summary>
    /// Gets or sets the vendor charge ID
    /// </summary>
    public string VendorChargeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets the total charge amount
    /// </summary>
    public decimal TotalChargeAmount => Amount + Fees.Sum(s => s.Amount);
    
    /// <summary>
    /// Gets or sets the location ID
    /// </summary>
    [StringLength(32)]
    public string LocationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user payment request URL
    /// </summary>
    [NotMapped]
    public string UserPaymentRequestUrl { get; set; } = string.Empty;
}
