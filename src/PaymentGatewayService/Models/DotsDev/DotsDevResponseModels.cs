using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.DotsDev;

/// <summary>
/// Response model for payout creation
/// </summary>
public class DotsDevPayoutResponse
{
    /// <summary>
    /// Unique payout ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Payout status (pending, processing, completed, failed)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    [JsonPropertyName("amount")]
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Selected payment method
    /// </summary>
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Recipient details
    /// </summary>
    [JsonPropertyName("recipient")]
    public DotsDevRecipientResponse Recipient { get; set; } = new();

    /// <summary>
    /// Estimated delivery time
    /// </summary>
    [JsonPropertyName("estimated_delivery")]
    public DateTime? EstimatedDelivery { get; set; }

    /// <summary>
    /// Transaction fees
    /// </summary>
    [JsonPropertyName("fees")]
    public DotsDevFees? Fees { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Error details if failed
    /// </summary>
    [JsonPropertyName("error")]
    public DotsDevError? Error { get; set; }

    /// <summary>
    /// Metadata passed in request
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Recipient information in response
/// </summary>
public class DotsDevRecipientResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("masked_details")]
    public Dictionary<string, string>? MaskedDetails { get; set; }
}

/// <summary>
/// Fee breakdown
/// </summary>
public class DotsDevFees
{
    [JsonPropertyName("platform_fee")]
    public int PlatformFeeInCents { get; set; }

    [JsonPropertyName("payment_method_fee")]
    public int PaymentMethodFeeInCents { get; set; }

    [JsonPropertyName("total_fee")]
    public int TotalFeeInCents { get; set; }

    [JsonPropertyName("net_amount")]
    public int NetAmountInCents { get; set; }
}

/// <summary>
/// Error details
/// </summary>
public class DotsDevError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Response for flow creation
/// </summary>
public class DotsDevFlowResponse
{
    /// <summary>
    /// Unique flow ID
    /// </summary>
    [JsonPropertyName("flow_id")]
    public string FlowId { get; set; } = string.Empty;

    /// <summary>
    /// URL for user to complete the flow
    /// </summary>
    [JsonPropertyName("flow_url")]
    public string FlowUrl { get; set; } = string.Empty;

    /// <summary>
    /// Flow expiration time
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Flow status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Flow status response
/// </summary>
public class DotsDevFlowStatus
{
    /// <summary>
    /// Flow ID
    /// </summary>
    [JsonPropertyName("flow_id")]
    public string FlowId { get; set; } = string.Empty;

    /// <summary>
    /// Current status (pending, in_progress, completed, expired)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Completion percentage
    /// </summary>
    [JsonPropertyName("completion_percentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Collected user data (if completed)
    /// </summary>
    [JsonPropertyName("collected_data")]
    public Dictionary<string, object>? CollectedData { get; set; }

    /// <summary>
    /// Verification results
    /// </summary>
    [JsonPropertyName("verification_results")]
    public DotsDevVerificationResults? VerificationResults { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Verification results from flow
/// </summary>
public class DotsDevVerificationResults
{
    [JsonPropertyName("identity_verified")]
    public bool IdentityVerified { get; set; }

    [JsonPropertyName("address_verified")]
    public bool AddressVerified { get; set; }

    [JsonPropertyName("tax_info_collected")]
    public bool TaxInfoCollected { get; set; }

    [JsonPropertyName("payment_method_verified")]
    public bool PaymentMethodVerified { get; set; }

    [JsonPropertyName("risk_score")]
    public string? RiskScore { get; set; }
}

/// <summary>
/// Webhook payload from Dots.dev
/// </summary>
public class DotsDevWebhookPayload
{
    /// <summary>
    /// Event type (payout.completed, payout.failed, flow.completed, etc.)
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>
    /// Unique event ID
    /// </summary>
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Event timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Event data (payout or flow data)
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Country support information
/// </summary>
public class DotsDevCountrySupport
{
    /// <summary>
    /// Country code (ISO 2)
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Country name
    /// </summary>
    [JsonPropertyName("country_name")]
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Supported currencies
    /// </summary>
    [JsonPropertyName("currencies")]
    public List<string> Currencies { get; set; } = new();

    /// <summary>
    /// Available payment methods
    /// </summary>
    [JsonPropertyName("payment_methods")]
    public List<DotsDevPaymentMethodInfo> PaymentMethods { get; set; } = new();

    /// <summary>
    /// Required compliance fields
    /// </summary>
    [JsonPropertyName("required_fields")]
    public List<string> RequiredFields { get; set; } = new();

    /// <summary>
    /// Estimated delivery times
    /// </summary>
    [JsonPropertyName("delivery_estimates")]
    public Dictionary<string, string> DeliveryEstimates { get; set; } = new();
}

/// <summary>
/// Payment method information
/// </summary>
public class DotsDevPaymentMethodInfo
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("min_amount")]
    public int MinAmountInCents { get; set; }

    [JsonPropertyName("max_amount")]
    public int MaxAmountInCents { get; set; }

    [JsonPropertyName("fees")]
    public DotsDevMethodFees Fees { get; set; } = new();

    [JsonPropertyName("required_details")]
    public List<string> RequiredDetails { get; set; } = new();
}

/// <summary>
/// Payment method fee structure
/// </summary>
public class DotsDevMethodFees
{
    [JsonPropertyName("fixed_fee")]
    public int FixedFeeInCents { get; set; }

    [JsonPropertyName("percentage_fee")]
    public decimal PercentageFee { get; set; }
}

/// <summary>
/// Validation result for recipient data
/// </summary>
public class DotsDevValidationResult
{
    /// <summary>
    /// Whether the data is valid
    /// </summary>
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors
    /// </summary>
    [JsonPropertyName("errors")]
    public List<DotsDevValidationError> Errors { get; set; } = new();

    /// <summary>
    /// Suggested payment methods
    /// </summary>
    [JsonPropertyName("suggested_methods")]
    public List<string> SuggestedMethods { get; set; } = new();
}

/// <summary>
/// Validation error details
/// </summary>
public class DotsDevValidationError
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}