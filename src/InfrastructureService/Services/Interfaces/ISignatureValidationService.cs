using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;

namespace InfrastructureService.Services.Interfaces;

/// <summary>
/// Service interface for RAGS (Robust Anti-replay Governance Signature) validation system
/// Provides quantum-resistant signature generation and validation with nonce replay protection
/// </summary>
public interface ISignatureValidationService
{
    /// <summary>
    /// Generates a signature for a service using the specified algorithm
    /// </summary>
    /// <param name="request">The signature generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature generation response</returns>
    Task<GenerateSignatureResponse> GenerateSignatureAsync(GenerateSignatureRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a signature using RAGS system with replay protection
    /// </summary>
    /// <param name="request">The signature validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature validation response</returns>
    Task<ValidateSignatureResponse> ValidateSignatureAsync(ValidateSignatureRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates signatures for multiple services in bulk
    /// </summary>
    /// <param name="request">The bulk signature generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The bulk signature generation response</returns>
    Task<BulkGenerateSignatureResponse> BulkGenerateSignaturesAsync(BulkGenerateSignatureRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a nonce has been used before for replay protection
    /// </summary>
    /// <param name="request">The nonce check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The nonce check response</returns>
    Task<CheckNonceResponse> CheckNonceAsync(CheckNonceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets signature validation metrics and performance statistics
    /// </summary>
    /// <param name="request">The metrics request with optional filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature metrics response</returns>
    Task<SignatureMetricsResponse> GetSignatureMetricsAsync(GetSignatureMetricsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available cryptographic algorithms supported by the system
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported algorithms</returns>
    Task<List<string>> GetSupportedAlgorithmsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a cryptographically secure nonce for replay protection
    /// </summary>
    /// <param name="serviceName">The service name to generate nonce for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique nonce string</returns>
    Task<string> GenerateNonceAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a service exists and has the required keys for signature operations
    /// </summary>
    /// <param name="serviceName">The service name to validate</param>
    /// <param name="algorithm">The algorithm to validate support for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is ready for signature operations</returns>
    Task<bool> ValidateServiceReadinessAsync(string serviceName, string algorithm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears expired nonces from the replay protection cache
    /// </summary>
    /// <param name="maxAge">Maximum age of nonces to keep (older nonces will be cleared)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of nonces cleared</returns>
    Task<int> ClearExpiredNoncesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of the signature validation system
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status information</returns>
    Task<Dictionary<string, object>> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
