using System.Text.Json.Serialization;

namespace PaymentGatewayService.Models.MoonPay;

/// <summary>
/// Request model for creating a fiat-to-crypto transaction (buy)
/// </summary>
public class MoonPayBuyRequest
{
    /// <summary>
    /// Cryptocurrency code to purchase (BTC, ETH, etc.)
    /// </summary>
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Amount in base currency (fiat)
    /// </summary>
    [JsonPropertyName("baseCurrencyAmount")]
    public decimal BaseCurrencyAmount { get; set; }

    /// <summary>
    /// Wallet address to receive cryptocurrency
    /// </summary>
    [JsonPropertyName("walletAddress")]
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Base currency code (USD, EUR, etc.)
    /// </summary>
    [JsonPropertyName("baseCurrencyCode")]
    public string BaseCurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Whether fees are included in the amount
    /// </summary>
    [JsonPropertyName("areFeesIncluded")]
    public bool AreFeesIncluded { get; set; } = false;

    /// <summary>
    /// External transaction ID for tracking
    /// </summary>
    [JsonPropertyName("externalTransactionId")]
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Customer email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Return URL after transaction completion
    /// </summary>
    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// UI theme customization
    /// </summary>
    [JsonPropertyName("theme")]
    public MoonPayTheme? Theme { get; set; }

    /// <summary>
    /// Payment method (credit_debit_card, bank_transfer, etc.)
    /// </summary>
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Request model for creating a crypto-to-fiat transaction (sell)
/// </summary>
public class MoonPaySellRequest
{
    /// <summary>
    /// Cryptocurrency code to sell (BTC, ETH, etc.)
    /// </summary>
    [JsonPropertyName("baseCurrencyCode")]
    public string BaseCurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Amount of cryptocurrency to sell
    /// </summary>
    [JsonPropertyName("baseCurrencyAmount")]
    public decimal BaseCurrencyAmount { get; set; }

    /// <summary>
    /// Fiat currency code to receive (USD, EUR, etc.)
    /// </summary>
    [JsonPropertyName("quoteCurrencyCode")]
    public string QuoteCurrencyCode { get; set; } = "USD";

    /// <summary>
    /// External transaction ID for tracking
    /// </summary>
    [JsonPropertyName("externalTransactionId")]
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Bank account details for fiat payout
    /// </summary>
    [JsonPropertyName("bankAccount")]
    public MoonPayBankAccount? BankAccount { get; set; }

    /// <summary>
    /// Customer email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// Bank account details for sell transactions
/// </summary>
public class MoonPayBankAccount
{
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("accountHolderName")]
    public string AccountHolderName { get; set; } = string.Empty;

    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }

    [JsonPropertyName("bankAddress")]
    public string? BankAddress { get; set; }

    [JsonPropertyName("sortCode")]
    public string? SortCode { get; set; }

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("bic")]
    public string? Bic { get; set; }
}

/// <summary>
/// Request for creating a widget session
/// </summary>
public class MoonPayWidgetRequest
{
    /// <summary>
    /// Flow type (buy, sell, nft)
    /// </summary>
    [JsonPropertyName("flow")]
    public string Flow { get; set; } = "buy";

    /// <summary>
    /// Default cryptocurrency code
    /// </summary>
    [JsonPropertyName("defaultCurrencyCode")]
    public string? DefaultCurrencyCode { get; set; }

    /// <summary>
    /// Default amount in base currency
    /// </summary>
    [JsonPropertyName("defaultBaseCurrencyAmount")]
    public decimal? DefaultBaseCurrencyAmount { get; set; }

    /// <summary>
    /// Default wallet address
    /// </summary>
    [JsonPropertyName("walletAddress")]
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Customer email for pre-fill
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// External customer ID
    /// </summary>
    [JsonPropertyName("externalCustomerId")]
    public string? ExternalCustomerId { get; set; }

    /// <summary>
    /// UI customization theme
    /// </summary>
    [JsonPropertyName("theme")]
    public MoonPayTheme? Theme { get; set; }

    /// <summary>
    /// Language code (en, es, fr, etc.)
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Color mode (light, dark)
    /// </summary>
    [JsonPropertyName("colorCode")]
    public string? ColorCode { get; set; }

    /// <summary>
    /// Show only specific cryptocurrencies
    /// </summary>
    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Redirect URL after completion
    /// </summary>
    [JsonPropertyName("redirectUrl")]
    public string? RedirectUrl { get; set; }
}

/// <summary>
/// UI theme customization
/// </summary>
public class MoonPayTheme
{
    [JsonPropertyName("primaryColor")]
    public string? PrimaryColor { get; set; }

    [JsonPropertyName("secondaryColor")]
    public string? SecondaryColor { get; set; }

    [JsonPropertyName("primaryTextColor")]
    public string? PrimaryTextColor { get; set; }

    [JsonPropertyName("secondaryTextColor")]
    public string? SecondaryTextColor { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("borderRadius")]
    public string? BorderRadius { get; set; }

    [JsonPropertyName("fontFamily")]
    public string? FontFamily { get; set; }
}

/// <summary>
/// Request for KYC/AML screening
/// </summary>
public class MoonPayScreeningRequest
{
    /// <summary>
    /// Email address to screen
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// IP address for geo-location
    /// </summary>
    [JsonPropertyName("ipAddress")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Full name for screening
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Date of birth (YYYY-MM-DD)
    /// </summary>
    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    /// <summary>
    /// Country code
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// State/province code
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
}