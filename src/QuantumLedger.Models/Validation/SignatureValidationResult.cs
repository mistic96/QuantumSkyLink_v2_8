namespace QuantumLedger.Models.Validation;

/// <summary>
/// Represents the result of a signature validation operation
/// </summary>
public class SignatureValidationResult : ValidationResultBase
{
    /// <summary>
    /// Gets or sets whether the ECC (EC-256) signature is valid
    /// </summary>
    public bool IsEccValid { get; set; }

    /// <summary>
    /// Gets or sets whether the PQC (Dilithium) signature is valid
    /// </summary>
    public bool IsPqcValid { get; set; }

    /// <summary>
    /// Creates a successful signature validation result.
    /// </summary>
    /// <param name="eccValid">Whether the ECC signature is valid.</param>
    /// <param name="pqcValid">Whether the PQC signature is valid.</param>
    /// <returns>A validation result indicating the signature validation status.</returns>
    public static SignatureValidationResult Create(bool eccValid, bool pqcValid)
    {
        var isValid = eccValid && pqcValid;
        return new SignatureValidationResult
        {
            IsValid = isValid,
            IsEccValid = eccValid,
            IsPqcValid = pqcValid,
            Message = isValid 
                ? "All signatures valid" 
                : $"Signature validation failed: ECC={eccValid}, PQC={pqcValid}"
        };
    }
}
