using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Payment method management service interface
/// </summary>
public interface IPaymentMethodService
{
    /// <summary>
    /// Creates a new payment method for a user
    /// </summary>
    /// <param name="request">The create payment method request</param>
    /// <returns>The created payment method response</returns>
    Task<PaymentMethodResponse> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);

    /// <summary>
    /// Gets a payment method by ID
    /// </summary>
    /// <param name="methodId">The payment method ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <returns>The payment method response</returns>
    Task<PaymentMethodResponse?> GetPaymentMethodAsync(Guid methodId, Guid userId);

    /// <summary>
    /// Gets all payment methods for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <returns>List of payment method responses</returns>
    Task<IEnumerable<PaymentMethodResponse>> GetUserPaymentMethodsAsync(Guid userId, bool? isActive = null);

    /// <summary>
    /// Updates a payment method
    /// </summary>
    /// <param name="request">The update payment method request</param>
    /// <returns>The updated payment method response</returns>
    Task<PaymentMethodResponse> UpdatePaymentMethodAsync(UpdatePaymentMethodRequest request);

    /// <summary>
    /// Deletes a payment method
    /// </summary>
    /// <param name="methodId">The payment method ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeletePaymentMethodAsync(Guid methodId, Guid userId);

    /// <summary>
    /// Sets a payment method as the default for a user
    /// </summary>
    /// <param name="methodId">The payment method ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The updated payment method response</returns>
    Task<PaymentMethodResponse> SetDefaultPaymentMethodAsync(Guid methodId, Guid userId);

    /// <summary>
    /// Verifies a payment method
    /// </summary>
    /// <param name="methodId">The payment method ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <param name="verificationData">Verification data from the gateway</param>
    /// <returns>The verified payment method response</returns>
    Task<PaymentMethodResponse> VerifyPaymentMethodAsync(Guid methodId, Guid userId, Dictionary<string, object> verificationData);

    /// <summary>
    /// Gets the default payment method for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The default payment method response</returns>
    Task<PaymentMethodResponse?> GetDefaultPaymentMethodAsync(Guid userId);
}
