namespace QuantumLedger.Models.Validation;

/// <summary>
/// Represents the result of validating a request in the Quantum Ledger system.
/// </summary>
public class RequestValidationResult : ValidationResultBase
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A validation result indicating success.</returns>
    public static RequestValidationResult Success()
    {
        return new RequestValidationResult
        {
            IsValid = true,
            Message = "Request validation successful"
        };
    }

    /// <summary>
    /// Creates a failed validation result with specified errors.
    /// </summary>
    /// <param name="message">The validation failure message.</param>
    /// <returns>A validation result indicating failure with the specified message.</returns>
    public static RequestValidationResult Failure(string message)
    {
        return new RequestValidationResult
        {
            IsValid = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }
}
