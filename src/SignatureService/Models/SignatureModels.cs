using QuantumLedger.Cryptography.Models;

namespace SignatureService.Models;

/// <summary>
/// Universal signature validation request that works with any system (QuantumSkyLink v2, QuantumLedger, etc.)
/// </summary>
public class UniversalSignatureValidationRequest
{
    /// <summary>
    /// Gets or sets the system making the request (e.g., "QuantumSkyLink_v2", "QuantumLedger")
    /// </summary>
    public string SystemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service making the request (e.g., "PaymentGatewayService", "LedgerEndpoints")
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID associated with this request
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation being performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation data being signed
    /// </summary>
    public object OperationData { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique nonce for replay protection
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence number for ordering
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the request
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the signature to validate
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signature algorithm used
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Dual signature validation request for QuantumLedger's classic + quantum signatures
/// </summary>
public class DualSignatureValidationRequest
{
    /// <summary>
    /// Gets or sets the account ID associated with this request
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation being performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation data being signed
    /// </summary>
    public byte[] OperationData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the dual signature (reuses QuantumLedger's DualSignature model)
    /// </summary>
    public DualSignature Signature { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the request
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the unique nonce for replay protection
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence number for ordering
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Result of signature validation
/// </summary>
public class SignatureValidationResult
{
    /// <summary>
    /// Gets or sets whether the signature is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the unique validation ID for tracking
    /// </summary>
    public string ValidationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if validation failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; }

    /// <summary>
    /// Gets or sets how long the validation took
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional validation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static SignatureValidationResult Valid(string validationId = "")
    {
        return new SignatureValidationResult
        {
            IsValid = true,
            ValidationId = string.IsNullOrEmpty(validationId) ? Guid.NewGuid().ToString() : validationId,
            ValidatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    public static SignatureValidationResult Failed(string error)
    {
        return new SignatureValidationResult
        {
            IsValid = false,
            Error = error,
            ValidatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Result of dual signature validation (extends QuantumLedger's VerificationResult)
/// </summary>
public class DualSignatureValidationResult : VerificationResult
{
    /// <summary>
    /// Gets or sets the unique validation ID for tracking
    /// </summary>
    public string ValidationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; }

    /// <summary>
    /// Gets or sets how long the validation took
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional validation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Transaction confirmation request
/// </summary>
public class TransactionConfirmationRequest
{
    /// <summary>
    /// Gets or sets the system confirming the transaction
    /// </summary>
    public string SystemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original validation ID
    /// </summary>
    public string OriginalValidationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction data
    /// </summary>
    public object TransactionData { get; set; } = new();

    /// <summary>
    /// Gets or sets the result signature
    /// </summary>
    public string ResultSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Transaction confirmation result
/// </summary>
public class TransactionConfirmationResult
{
    /// <summary>
    /// Gets or sets whether the confirmation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the confirmation ID
    /// </summary>
    public string ConfirmationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if confirmation failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the confirmation was performed
    /// </summary>
    public DateTime ConfirmedAt { get; set; }
}

/// <summary>
/// Nonce validation request
/// </summary>
public class NonceValidationRequest
{
    /// <summary>
    /// Gets or sets the account ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce to validate
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence number
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Nonce validation result
/// </summary>
public class NonceValidationResult
{
    /// <summary>
    /// Gets or sets whether the nonce is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if validation failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets additional error details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}
