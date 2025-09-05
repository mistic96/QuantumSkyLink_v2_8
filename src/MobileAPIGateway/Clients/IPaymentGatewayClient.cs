using MobileAPIGateway.Models.Wallet;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Payment Gateway Service client interface for mobile-specific operations
/// </summary>
public interface IPaymentGatewayClient
{
    /// <summary>
    /// Generates a new deposit code for the user
    /// </summary>
    /// <param name="request">Deposit code generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit code generation response</returns>
    Task<DepositCodeGenerationResponse> GenerateDepositCodeAsync(DepositCodeGenerationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a deposit code in real-time
    /// </summary>
    /// <param name="request">Deposit code validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit code validation response</returns>
    Task<DepositCodeValidationResponse> ValidateDepositCodeAsync(DepositCodeValidationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes a deposit with deposit code validation
    /// </summary>
    /// <param name="request">Enhanced deposit request with code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit processing response</returns>
    Task<DepositProcessingResponse> ProcessDepositWithCodeAsync(EnhancedDepositRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets deposit code status and details
    /// </summary>
    /// <param name="depositCode">The deposit code to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit code status response</returns>
    Task<DepositCodeStatusResponse> GetDepositCodeStatusAsync(string depositCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user's active deposit codes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active deposit codes</returns>
    Task<IEnumerable<UserDepositCode>> GetUserActiveDepositCodesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes/expires a deposit code
    /// </summary>
    /// <param name="depositCode">The deposit code to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revocation response</returns>
    Task<DepositCodeRevocationResponse> RevokeDepositCodeAsync(string depositCode, CancellationToken cancellationToken = default);

    // Provider helpers (Square)
    Task<Dictionary<string, object>> GetSquareClientParamsAsync(decimal amount, string currency, string? referenceId = null, CancellationToken cancellationToken = default);
    Task<(string? CheckoutUrl, DateTime? ExpiresAt, string? ReferenceId, string? Error)> CreateSquarePaymentLinkAsync(decimal amount, string currency, string? referenceId = null, string? email = null, CancellationToken cancellationToken = default);
}
