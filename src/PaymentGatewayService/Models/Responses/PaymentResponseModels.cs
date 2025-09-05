using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models.Responses;

/// <summary>
/// Response model for payment operations
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment status
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the payment type
    /// </summary>
    public PaymentType Type { get; set; }

    /// <summary>
    /// Gets or sets the gateway transaction ID
    /// </summary>
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway information
    /// </summary>
    public PaymentGatewayResponse? PaymentGateway { get; set; }

    /// <summary>
    /// Gets or sets the payment method information
    /// </summary>
    public PaymentMethodResponse? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the fee amount
    /// </summary>
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Gets or sets the net amount after fees
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets when the payment was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets PIX-specific data (only populated for PIX payments)
    /// </summary>
    public PIXPaymentData? PIXData { get; set; }
}

/// <summary>
/// PIX-specific payment data
/// </summary>
public class PIXPaymentData
{
    /// <summary>
    /// Gets or sets the PIX QR code string (copy/paste format)
    /// </summary>
    public string? QRCodeString { get; set; }

    /// <summary>
    /// Gets or sets the base64 encoded QR code image
    /// </summary>
    public string? QRCodeImage { get; set; }

    /// <summary>
    /// Gets or sets the PIX key (for payouts)
    /// </summary>
    public string? PixKey { get; set; }

    /// <summary>
    /// Gets or sets the PIX key type
    /// </summary>
    public string? PixKeyType { get; set; }

    /// <summary>
    /// Gets or sets the end-to-end identifier
    /// </summary>
    public string? EndToEndId { get; set; }
}

/// <summary>
/// Response model for paginated payment results
/// </summary>
public class PaginatedPaymentResponse
{
    /// <summary>
    /// Gets or sets the list of payments
    /// </summary>
    public IEnumerable<PaymentResponse> Payments { get; set; } = new List<PaymentResponse>();

    /// <summary>
    /// Gets or sets the current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of payments
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Gets or sets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Response model for payment statistics
/// </summary>
public class PaymentStatisticsResponse
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the total number of payments
    /// </summary>
    public int TotalPayments { get; set; }

    /// <summary>
    /// Gets or sets the total amount of all payments
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the total fees paid
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the number of successful payments
    /// </summary>
    public int SuccessfulPayments { get; set; }

    /// <summary>
    /// Gets or sets the number of failed payments
    /// </summary>
    public int FailedPayments { get; set; }

    /// <summary>
    /// Gets or sets the number of pending payments
    /// </summary>
    public int PendingPayments { get; set; }

    /// <summary>
    /// Gets or sets the average payment amount
    /// </summary>
    public decimal AveragePaymentAmount { get; set; }

    /// <summary>
    /// Gets or sets the statistics by currency
    /// </summary>
    public IEnumerable<CurrencyStatistics> CurrencyBreakdown { get; set; } = new List<CurrencyStatistics>();

    /// <summary>
    /// Gets or sets the statistics by payment type
    /// </summary>
    public IEnumerable<PaymentTypeStatistics> TypeBreakdown { get; set; } = new List<PaymentTypeStatistics>();

    /// <summary>
    /// Gets or sets the date range for the statistics
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for the statistics
    /// </summary>
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Currency-specific statistics
/// </summary>
public class CurrencyStatistics
{
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount in this currency
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the number of payments in this currency
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// Gets or sets the average amount in this currency
    /// </summary>
    public decimal AverageAmount { get; set; }
}

/// <summary>
/// Payment type-specific statistics
/// </summary>
public class PaymentTypeStatistics
{
    /// <summary>
    /// Gets or sets the payment type
    /// </summary>
    public PaymentType Type { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this type
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the number of payments of this type
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// Gets or sets the average amount for this type
    /// </summary>
    public decimal AverageAmount { get; set; }
}

/// <summary>
/// Response model for payment history
/// </summary>
public class PaymentHistoryResponse
{
    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the current payment status
    /// </summary>
    public PaymentStatus CurrentStatus { get; set; }

    /// <summary>
    /// Gets or sets the payment attempts
    /// </summary>
    public IEnumerable<PaymentAttemptResponse> PaymentAttempts { get; set; } = new List<PaymentAttemptResponse>();

    /// <summary>
    /// Gets or sets the refunds
    /// </summary>
    public IEnumerable<RefundResponse> Refunds { get; set; } = new List<RefundResponse>();

    /// <summary>
    /// Gets or sets the status history
    /// </summary>
    public IEnumerable<PaymentStatusHistory> StatusHistory { get; set; } = new List<PaymentStatusHistory>();

    /// <summary>
    /// Gets or sets when the payment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for payment attempts
/// </summary>
public class PaymentAttemptResponse
{
    /// <summary>
    /// Gets or sets the attempt ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the attempt number
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Gets or sets the attempt status
    /// </summary>
    public PaymentAttemptStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the gateway transaction ID
    /// </summary>
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the amount attempted
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the attempt was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public int? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets when the attempt was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response model for refunds
/// </summary>
public class RefundResponse
{
    /// <summary>
    /// Gets or sets the refund ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the refund amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refund reason
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refund status
    /// </summary>
    public RefundStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the gateway refund ID
    /// </summary>
    public string? GatewayRefundId { get; set; }

    /// <summary>
    /// Gets or sets who requested the refund
    /// </summary>
    public Guid RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets who approved the refund
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Gets or sets when the refund was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the refund fee
    /// </summary>
    public decimal RefundFee { get; set; }

    /// <summary>
    /// Gets or sets the net refund amount
    /// </summary>
    public decimal NetRefundAmount { get; set; }

    /// <summary>
    /// Gets or sets whether this is a partial refund
    /// </summary>
    public bool IsPartialRefund { get; set; }

    /// <summary>
    /// Gets or sets when the refund was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Payment status history entry
/// </summary>
public class PaymentStatusHistory
{
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the status changed
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for the status change
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Response model for payment gateways
/// </summary>
public class PaymentGatewayResponse
{
    /// <summary>
    /// Gets or sets the gateway ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the gateway name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gateway type
    /// </summary>
    public PaymentGatewayType GatewayType { get; set; }

    /// <summary>
    /// Gets or sets whether the gateway is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the gateway is in test mode
    /// </summary>
    public bool IsTestMode { get; set; }

    /// <summary>
    /// Gets or sets the fee percentage
    /// </summary>
    public decimal FeePercentage { get; set; }

    /// <summary>
    /// Gets or sets the fixed fee
    /// </summary>
    public decimal FixedFee { get; set; }

    /// <summary>
    /// Gets or sets the minimum amount
    /// </summary>
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount
    /// </summary>
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Gets or sets the supported currencies
    /// </summary>
    public IEnumerable<string> SupportedCurrencies { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the supported countries
    /// </summary>
    public IEnumerable<string> SupportedCountries { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the priority
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets when the gateway was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the gateway was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for payment methods
/// </summary>
public class PaymentMethodResponse
{
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway
    /// </summary>
    public PaymentGatewayResponse? PaymentGateway { get; set; }

    /// <summary>
    /// Gets or sets the method type
    /// </summary>
    public PaymentMethodType MethodType { get; set; }

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the method is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the method is verified
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default method
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the last 4 digits
    /// </summary>
    public string? Last4Digits { get; set; }

    /// <summary>
    /// Gets or sets the brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets when it was last used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets when it was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response model for gateway statistics
/// </summary>
public class GatewayStatisticsResponse
{
    /// <summary>
    /// Gets or sets the gateway ID
    /// </summary>
    public Guid GatewayId { get; set; }

    /// <summary>
    /// Gets or sets the gateway name
    /// </summary>
    public string GatewayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of transactions
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Gets or sets the total transaction amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the success rate percentage
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the average processing time
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the total fees collected
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the date range
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date
    /// </summary>
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Response model for gateway tests
/// </summary>
public class GatewayTestResponse
{
    /// <summary>
    /// Gets or sets the gateway ID
    /// </summary>
    public Guid GatewayId { get; set; }

    /// <summary>
    /// Gets or sets whether the test was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets or sets the test message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets when the test was performed
    /// </summary>
    public DateTime TestedAt { get; set; }

    /// <summary>
    /// Gets or sets additional test details
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
