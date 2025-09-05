using Refit;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// FeeService API client interface for rejection fee calculations
/// </summary>
public interface IFeeServiceApi
{
    /// <summary>
    /// Calculates fiat rejection fees (wire fees, Square fees, internal fees)
    /// </summary>
    [Post("/api/fees/calculate-fiat-rejection")]
    Task<FiatRejectionFeesResponse> CalculateFiatRejectionFeesAsync([Body] FiatRejectionFeesRequest request);

    /// <summary>
    /// Calculates crypto rejection fees (network fees, internal fees)
    /// </summary>
    [Post("/api/fees/calculate-crypto-rejection")]
    Task<CryptoRejectionFeesResponse> CalculateCryptoRejectionFeesAsync([Body] CryptoRejectionFeesRequest request);
}

/// <summary>
/// Request model for fiat rejection fees calculation
/// </summary>
public class FiatRejectionFeesRequest
{
    /// <summary>
    /// Gets or sets the deposit amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment gateway type
    /// </summary>
    public string GatewayType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string PaymentMethodType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for rejection
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for crypto rejection fees calculation
/// </summary>
public class CryptoRejectionFeesRequest
{
    /// <summary>
    /// Gets or sets the deposit amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the cryptocurrency symbol
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the network name
    /// </summary>
    public string Network { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the wallet address
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for rejection
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Response model for fiat rejection fees
/// </summary>
public class FiatRejectionFeesResponse
{
    /// <summary>
    /// Gets or sets the wire transfer fees
    /// </summary>
    public decimal WireFees { get; set; }

    /// <summary>
    /// Gets or sets the Square processing fees
    /// </summary>
    public decimal SquareFees { get; set; }

    /// <summary>
    /// Gets or sets the internal processing fees
    /// </summary>
    public decimal InternalFees { get; set; }

    /// <summary>
    /// Gets or sets the total rejection fees
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the net amount after fees
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Response model for crypto rejection fees
/// </summary>
public class CryptoRejectionFeesResponse
{
    /// <summary>
    /// Gets or sets the network transaction fees
    /// </summary>
    public decimal NetworkFees { get; set; }

    /// <summary>
    /// Gets or sets the internal processing fees
    /// </summary>
    public decimal InternalFees { get; set; }

    /// <summary>
    /// Gets or sets the total rejection fees
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Gets or sets the net amount after fees
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the cryptocurrency symbol
    /// </summary>
    public string Cryptocurrency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the network name
    /// </summary>
    public string Network { get; set; } = string.Empty;
}