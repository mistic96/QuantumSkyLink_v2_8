using Refit;
using OrchestrationService.Models;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for SignatureService communication
/// Zero-trust signature validation for all financial operations
/// </summary>
public interface ISignatureServiceClient
{
    [Post("/api/signatures/validate-request")]
    Task<SignatureValidationResult> ValidateRequestSignatureAsync(
        [Body] SignatureValidationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/signatures/validate-result")]
    Task<SignatureValidationResult> ValidateResultSignatureAsync(
        [Body] ResultSignatureValidationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/signatures/validate-dual")]
    Task<DualSignatureValidationResult> ValidateDualSignatureAsync(
        [Body] DualSignatureValidationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for signature validation
/// </summary>
public class SignatureValidationRequest
{
    public string AccountId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public object OperationData { get; set; } = new();
    public string Nonce { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
}

/// <summary>
/// Result of signature validation
/// </summary>
public class SignatureValidationResult
{
    public bool IsValid { get; set; }
    public string ValidationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request for result signature validation
/// </summary>
public class ResultSignatureValidationRequest
{
    public string OriginalValidationId { get; set; } = string.Empty;
    public object ResultData { get; set; } = new();
    public string ResultSignature { get; set; } = string.Empty;
    public string SigningService { get; set; } = string.Empty;
}

/// <summary>
/// Request for dual signature validation
/// </summary>
public class DualSignatureValidationRequest
{
    public SignatureValidationRequest PrimarySignature { get; set; } = new();
    public SignatureValidationRequest SecondarySignature { get; set; } = new();
    public string Operation { get; set; } = string.Empty;
}

/// <summary>
/// Result of dual signature validation
/// </summary>
public class DualSignatureValidationResult
{
    public bool IsValid { get; set; }
    public string PrimaryValidationId { get; set; } = string.Empty;
    public string SecondaryValidationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; }
}
