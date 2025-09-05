using PaymentGatewayService.Models.Requests;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Payment validation service interface
/// Handles payment request validation and business rules
/// </summary>
public interface IPaymentValidationService
{
    /// <summary>
    /// Validates a payment request
    /// </summary>
    /// <param name="request">The payment request to validate</param>
    Task ValidatePaymentRequestAsync(global::PaymentGatewayService.Models.Requests.ProcessPaymentRequest request);

    /// <summary>
    /// Validates a payment method for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="paymentMethodId">The payment method ID</param>
    Task ValidatePaymentMethodAsync(Guid userId, Guid paymentMethodId);

    /// <summary>
    /// Validates payment amount and currency
    /// </summary>
    /// <param name="amount">The payment amount</param>
    /// <param name="currency">The payment currency</param>
    Task ValidateAmountAndCurrencyAsync(decimal amount, string currency);

    /// <summary>
    /// Validates user payment limits
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="amount">The payment amount</param>
    /// <param name="currency">The payment currency</param>
    Task ValidateUserLimitsAsync(Guid userId, decimal amount, string currency);

    /// <summary>
    /// Validates deposit code for deposits
    /// </summary>
    /// <param name="userId">The user ID (nullable for system-generated deposits)</param>
    /// <param name="depositCode">The 8-character deposit code</param>
    /// <param name="amount">The deposit amount</param>
    /// <param name="currency">The deposit currency</param>
    Task ValidateDepositCodeAsync(Guid? userId, string depositCode, decimal amount, string currency);

    /// <summary>
    /// Generates a unique 8-character cryptographic deposit code
    /// </summary>
    /// <param name="userId">The user ID requesting the code (nullable for system-generated deposits)</param>
    /// <returns>A unique 8-character deposit code</returns>
    Task<string> GenerateDepositCodeAsync(Guid? userId);
}
