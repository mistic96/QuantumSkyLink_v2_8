using PaymentGatewayService.Models.DotsDev;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for Dots.dev payout service supporting 190+ countries
/// </summary>
public interface IDotsDevService
{
    /// <summary>
    /// Creates a payout to recipients in 190+ countries with automatic payment method selection
    /// </summary>
    /// <param name="request">Payout request with recipient details</param>
    /// <returns>Payout response with transaction details</returns>
    Task<DotsDevPayoutResponse> CreatePayoutAsync(DotsDevPayoutRequest request);

    /// <summary>
    /// Creates an onboarding flow for recipient verification and compliance
    /// </summary>
    /// <param name="request">Flow creation request</param>
    /// <returns>Flow response with flow ID and URL</returns>
    Task<DotsDevFlowResponse> CreateOnboardingFlowAsync(DotsDevFlowRequest request);

    /// <summary>
    /// Gets the status and completion data of an onboarding flow
    /// </summary>
    /// <param name="flowId">The flow ID to check</param>
    /// <returns>Flow status and collected data</returns>
    Task<DotsDevFlowStatus> GetFlowStatusAsync(string flowId);

    /// <summary>
    /// Processes webhook notifications from Dots.dev
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature for verification</param>
    /// <returns>True if processed successfully</returns>
    Task<bool> ProcessWebhookAsync(DotsDevWebhookPayload payload, string signature);

    /// <summary>
    /// Gets the list of supported countries and their available payment methods
    /// </summary>
    /// <returns>List of supported countries with payment method details</returns>
    Task<List<DotsDevCountrySupport>> GetSupportedCountriesAsync();

    /// <summary>
    /// Validates recipient details for a specific country
    /// </summary>
    /// <param name="country">Country code (US, EU, IN, PH, etc.)</param>
    /// <param name="recipientData">Recipient information to validate</param>
    /// <returns>Validation result with any errors</returns>
    Task<DotsDevValidationResult> ValidateRecipientAsync(string country, Dictionary<string, object> recipientData);
}