using PaymentGatewayService.Models.MoonPay;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for MoonPay fiat-to-crypto and crypto-to-fiat service
/// </summary>
public interface IMoonPayService
{
    /// <summary>
    /// Creates a fiat-to-crypto transaction (on-ramp)
    /// </summary>
    /// <param name="request">Transaction request with currency and amount details</param>
    /// <returns>Transaction response with widget URL and transaction details</returns>
    Task<MoonPayTransactionResponse> CreateBuyTransactionAsync(MoonPayBuyRequest request);

    /// <summary>
    /// Creates a crypto-to-fiat transaction (off-ramp)
    /// </summary>
    /// <param name="request">Sell transaction request</param>
    /// <returns>Transaction response with status and details</returns>
    Task<MoonPayTransactionResponse> CreateSellTransactionAsync(MoonPaySellRequest request);

    /// <summary>
    /// Gets the status of a transaction
    /// </summary>
    /// <param name="transactionId">MoonPay transaction ID</param>
    /// <returns>Current transaction status and details</returns>
    Task<MoonPayTransactionStatus> GetTransactionStatusAsync(string transactionId);

    /// <summary>
    /// Gets available cryptocurrencies for trading
    /// </summary>
    /// <param name="fiatCurrency">Optional fiat currency filter</param>
    /// <returns>List of supported cryptocurrencies</returns>
    Task<List<MoonPayCurrency>> GetSupportedCurrenciesAsync(string? fiatCurrency = null);

    /// <summary>
    /// Gets current exchange rates
    /// </summary>
    /// <param name="cryptoCurrency">Cryptocurrency code (BTC, ETH, etc.)</param>
    /// <param name="fiatCurrency">Fiat currency code (USD, EUR, etc.)</param>
    /// <returns>Current exchange rate information</returns>
    Task<MoonPayQuote> GetQuoteAsync(string cryptoCurrency, string fiatCurrency);

    /// <summary>
    /// Processes webhook notifications from MoonPay
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature for verification</param>
    /// <returns>True if processed successfully</returns>
    Task<bool> ProcessWebhookAsync(MoonPayWebhookPayload payload, string signature);

    /// <summary>
    /// Creates a widget session for embedded UI
    /// </summary>
    /// <param name="request">Widget configuration request</param>
    /// <returns>Widget session with URL and configuration</returns>
    Task<MoonPayWidgetResponse> CreateWidgetSessionAsync(MoonPayWidgetRequest request);

    /// <summary>
    /// Performs KYC/AML screening for a recipient
    /// </summary>
    /// <param name="request">Recipient screening request</param>
    /// <returns>Screening result with risk assessment</returns>
    Task<MoonPayScreeningResult> ScreenRecipientAsync(MoonPayScreeningRequest request);

    /// <summary>
    /// Gets transaction limits for a currency pair
    /// </summary>
    /// <param name="cryptoCurrency">Cryptocurrency code</param>
    /// <param name="fiatCurrency">Fiat currency code</param>
    /// <param name="paymentMethod">Payment method (card, bank_transfer, etc.)</param>
    /// <returns>Transaction limits and fees</returns>
    Task<MoonPayLimits> GetTransactionLimitsAsync(string cryptoCurrency, string fiatCurrency, string paymentMethod);
}