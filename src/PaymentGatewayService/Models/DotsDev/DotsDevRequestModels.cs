using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.DotsDev;

/// <summary>
/// Request model for creating a payout through Dots.dev
/// </summary>
public class DotsDevPayoutRequest
{
    /// <summary>
    /// Amount to payout in cents
    /// </summary>
    [JsonPropertyName("amount")]
    public int AmountInCents { get; set; }

    /// <summary>
    /// Currency code (USD, EUR, INR, PHP, etc.)
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Recipient information
    /// </summary>
    [JsonPropertyName("recipient")]
    public DotsDevRecipient Recipient { get; set; } = new();

    /// <summary>
    /// Idempotency key to prevent duplicate payouts
    /// </summary>
    [JsonPropertyName("idempotency_key")]
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the payout
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional metadata for tracking
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Callback URL for webhook notifications
    /// </summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }
}

/// <summary>
/// Recipient information for payouts
/// </summary>
public class DotsDevRecipient
{
    /// <summary>
    /// Recipient's full name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Country code (US, GB, DE, IN, PH, etc.)
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (optional, required for some countries)
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Preferred payment method (auto-selected if not specified)
    /// </summary>
    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment method specific details (varies by country/method)
    /// </summary>
    [JsonPropertyName("payment_details")]
    public Dictionary<string, object>? PaymentDetails { get; set; }

    /// <summary>
    /// Compliance data (tax ID, address, etc.)
    /// </summary>
    [JsonPropertyName("compliance_data")]
    public DotsDevComplianceData? ComplianceData { get; set; }
}

/// <summary>
/// Compliance data for regulatory requirements
/// </summary>
public class DotsDevComplianceData
{
    /// <summary>
    /// Tax identification number (SSN, PAN, etc.)
    /// </summary>
    [JsonPropertyName("tax_id")]
    public string? TaxId { get; set; }

    /// <summary>
    /// Date of birth (required for some regions)
    /// </summary>
    [JsonPropertyName("date_of_birth")]
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Address information
    /// </summary>
    [JsonPropertyName("address")]
    public DotsDevAddress? Address { get; set; }

    /// <summary>
    /// Government ID type (passport, driver_license, etc.)
    /// </summary>
    [JsonPropertyName("id_type")]
    public string? IdType { get; set; }

    /// <summary>
    /// Government ID number
    /// </summary>
    [JsonPropertyName("id_number")]
    public string? IdNumber { get; set; }
}

/// <summary>
/// Address information for compliance
/// </summary>
public class DotsDevAddress
{
    [JsonPropertyName("line1")]
    public string Line1 { get; set; } = string.Empty;

    [JsonPropertyName("line2")]
    public string? Line2 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating an onboarding flow
/// </summary>
public class DotsDevFlowRequest
{
    /// <summary>
    /// Type of flow (payout_onboarding, identity_verification, etc.)
    /// </summary>
    [JsonPropertyName("flow_type")]
    public string FlowType { get; set; } = "payout_onboarding";

    /// <summary>
    /// User information to pre-fill
    /// </summary>
    [JsonPropertyName("user_data")]
    public Dictionary<string, object>? UserData { get; set; }

    /// <summary>
    /// Custom metadata for tracking
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Redirect URL after flow completion
    /// </summary>
    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Callback URL for webhooks
    /// </summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// UI customization options
    /// </summary>
    [JsonPropertyName("theme")]
    public DotsDevTheme? Theme { get; set; }
}

/// <summary>
/// Theme customization for flows
/// </summary>
public class DotsDevTheme
{
    [JsonPropertyName("primary_color")]
    public string? PrimaryColor { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }
}