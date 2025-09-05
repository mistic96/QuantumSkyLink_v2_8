using System.ComponentModel.DataAnnotations;
using PaymentGatewayService.Models;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models.Requests;

/// <summary>
/// Request model for processing a payment
/// </summary>
public class ProcessPaymentRequest
{
    /// <summary>
    /// Gets or sets the user ID initiating the payment (nullable for system-generated deposits)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment amount
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217)
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment type
    /// </summary>
    [Required]
    public PaymentType Type { get; set; }

    /// <summary>
    /// Gets or sets the payment method ID (optional)
    /// </summary>
    public Guid? PaymentMethodId { get; set; }

    /// <summary>
    /// Gets or sets the preferred payment gateway ID (optional)
    /// </summary>
    public Guid? PreferredGatewayId { get; set; }

    /// <summary>
    /// Gets or sets the payment description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets additional metadata (JSON)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the client IP address
    /// </summary>
    [StringLength(45)]
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the return URL for redirects
    /// </summary>
    [Url]
    [StringLength(500)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Gets or sets the cancel URL for redirects
    /// </summary>
    [Url]
    [StringLength(500)]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Gets or sets the deposit code (required for deposit transactions)
    /// </summary>
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Deposit code must be exactly 8 characters")]
    public string? DepositCode { get; set; }
}

/// <summary>
/// Request model for getting payments with filtering and pagination
/// </summary>
public class GetPaymentsRequest
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment status filter
    /// </summary>
    public PaymentStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the payment type filter
    /// </summary>
    public PaymentType? Type { get; set; }

    /// <summary>
    /// Gets or sets the currency filter
    /// </summary>
    [StringLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the minimum amount filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the start date filter
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date filter
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Gets or sets the page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the sort field
    /// </summary>
    [StringLength(50)]
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Gets or sets the sort direction
    /// </summary>
    [StringLength(4)]
    public string? SortDirection { get; set; } = "desc";
}

/// <summary>
/// Request model for updating payment status
/// </summary>
public class UpdatePaymentStatusRequest
{
    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    [Required]
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the new payment status
    /// </summary>
    [Required]
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the reason for the status change
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the gateway transaction ID
    /// </summary>
    [StringLength(255)]
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request model for creating a payment gateway
/// </summary>
public class CreatePaymentGatewayRequest
{
    /// <summary>
    /// Gets or sets the gateway name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gateway type
    /// </summary>
    [Required]
    public PaymentGatewayType GatewayType { get; set; }

    /// <summary>
    /// Gets or sets whether the gateway is in test mode
    /// </summary>
    public bool IsTestMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the gateway configuration
    /// </summary>
    [Required]
    public Dictionary<string, object> Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets the fee percentage
    /// </summary>
    [Range(0, 100)]
    public decimal FeePercentage { get; set; } = 0;

    /// <summary>
    /// Gets or sets the fixed fee amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal FixedFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the minimum transaction amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum transaction amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Gets or sets the supported currencies
    /// </summary>
    [StringLength(500)]
    public string SupportedCurrencies { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the supported countries
    /// </summary>
    [StringLength(1000)]
    public string? SupportedCountries { get; set; }

    /// <summary>
    /// Gets or sets the priority order
    /// </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>
/// Request model for updating a payment gateway
/// </summary>
public class UpdatePaymentGatewayRequest
{
    /// <summary>
    /// Gets or sets the gateway ID
    /// </summary>
    [Required]
    public Guid GatewayId { get; set; }

    /// <summary>
    /// Gets or sets the gateway name
    /// </summary>
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether the gateway is in test mode
    /// </summary>
    public bool? IsTestMode { get; set; }

    /// <summary>
    /// Gets or sets the gateway configuration
    /// </summary>
    public Dictionary<string, object>? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the fee percentage
    /// </summary>
    [Range(0, 100)]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Gets or sets the fixed fee amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? FixedFee { get; set; }

    /// <summary>
    /// Gets or sets the minimum transaction amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum transaction amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Gets or sets the supported currencies
    /// </summary>
    [StringLength(500)]
    public string? SupportedCurrencies { get; set; }

    /// <summary>
    /// Gets or sets the supported countries
    /// </summary>
    [StringLength(1000)]
    public string? SupportedCountries { get; set; }

    /// <summary>
    /// Gets or sets the priority order
    /// </summary>
    public int? Priority { get; set; }
}
