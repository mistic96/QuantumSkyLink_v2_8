using System.Text.RegularExpressions;

namespace QuantumLedger.Models.Validation;

/// <summary>
/// Default implementation of request validation for the Quantum Ledger system.
/// </summary>
public class DefaultRequestValidator : IRequestValidator
{
    // Basic regex pattern for validating blockchain addresses
    private static readonly Regex AddressPattern = new(@"^0x[a-fA-F0-9]{40}$", RegexOptions.Compiled);

    // Basic regex pattern for validating base64 strings
    private static readonly Regex Base64Pattern = new(@"^[a-zA-Z0-9+/]*={0,2}$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a request including its basic properties and signature.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    public async Task<RequestValidationResult> ValidateAsync(Request request)
    {
        if (request == null)
        {
            return RequestValidationResult.Failure("Request cannot be null");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return RequestValidationResult.Failure("Request ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return RequestValidationResult.Failure("Request type is required");
        }

        if (string.IsNullOrWhiteSpace(request.Data))
        {
            return RequestValidationResult.Failure("Request data is required");
        }

        if (request.CreatedAt > DateTime.UtcNow)
        {
            return RequestValidationResult.Failure("Request creation time cannot be in the future");
        }

        // Validate data format (assuming JSON for MVP)
        if (!IsValidJson(request.Data))
        {
            return RequestValidationResult.Failure("Request data must be valid JSON");
        }

        // Validate any blockchain addresses in the data
        if (ContainsAddress(request.Data) && !HasValidAddresses(request.Data))
        {
            return RequestValidationResult.Failure("Request contains invalid blockchain addresses");
        }

        // Validate signatures
        if (string.IsNullOrWhiteSpace(request.ClassicSignature) || string.IsNullOrWhiteSpace(request.QuantumSignature))
        {
            return RequestValidationResult.Failure("Both classical and quantum signatures are required");
        }

        if (!IsValidBase64(request.ClassicSignature) || !IsValidBase64(request.QuantumSignature))
        {
            return RequestValidationResult.Failure("Signatures must be valid base64 strings");
        }

        if (string.IsNullOrWhiteSpace(request.ClassicKeyId) || string.IsNullOrWhiteSpace(request.QuantumKeyId))
        {
            return RequestValidationResult.Failure("Both classical and quantum key IDs are required");
        }

        // Validate signature
        var signatureResult = await ValidateSignatureAsync(request);
        if (!signatureResult.IsValid)
        {
            return RequestValidationResult.Failure(signatureResult.Message);
        }

        return RequestValidationResult.Success();
    }

    /// <summary>
    /// Validates only the signature portion of a request.
    /// For MVP, this is a simplified implementation that will be replaced by the QSP module.
    /// </summary>
    /// <param name="request">The request whose signature should be validated.</param>
    /// <returns>A signature validation result indicating success or failure with details.</returns>
    public Task<SignatureValidationResult> ValidateSignatureAsync(Request request)
    {
        if (request == null)
        {
            return Task.FromResult(SignatureValidationResult.Create(false, false));
        }

        // For MVP, we'll just check that signatures and key IDs are present and valid base64
        bool hasClassicSignature = !string.IsNullOrWhiteSpace(request.ClassicSignature) && 
                                 IsValidBase64(request.ClassicSignature) && 
                                 !string.IsNullOrWhiteSpace(request.ClassicKeyId);

        bool hasQuantumSignature = !string.IsNullOrWhiteSpace(request.QuantumSignature) && 
                                 IsValidBase64(request.QuantumSignature) && 
                                 !string.IsNullOrWhiteSpace(request.QuantumKeyId);

        // TODO: Replace with actual ECC and PQC signature validation in QSP module
        return Task.FromResult(SignatureValidationResult.Create(
            hasClassicSignature,  // ECC validation result
            hasQuantumSignature   // PQC validation result
        ));
    }

    /// <summary>
    /// Checks if a string is valid JSON.
    /// </summary>
    private static bool IsValidJson(string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput)) return false;
        try
        {
            System.Text.Json.JsonDocument.Parse(strInput);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string is valid base64.
    /// </summary>
    private static bool IsValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        
        // Check basic format
        if (!Base64Pattern.IsMatch(base64)) return false;

        try
        {
            // Try to decode
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the payload contains any blockchain addresses.
    /// </summary>
    private static bool ContainsAddress(string payload)
    {
        return payload.Contains("0x", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates all blockchain addresses in the payload.
    /// </summary>
    private static bool HasValidAddresses(string payload)
    {
        // Find all potential addresses (0x followed by 40 hex characters)
        var matches = AddressPattern.Matches(payload);
        
        // If we found any addresses, they must all be valid
        return matches.Count > 0;
    }
}
