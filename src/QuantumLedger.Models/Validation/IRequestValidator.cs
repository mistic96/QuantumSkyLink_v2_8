using System.Threading.Tasks;

namespace QuantumLedger.Models.Validation
{
    /// <summary>
    /// Defines the contract for request validation in the Quantum Ledger system.
    /// </summary>
    public interface IRequestValidator
    {
        /// <summary>
        /// Validates a request including its basic properties and signature.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>A validation result indicating success or failure with details.</returns>
        Task<RequestValidationResult> ValidateAsync(Request request);

        /// <summary>
        /// Validates only the signature portion of a request.
        /// </summary>
        /// <param name="request">The request whose signature should be validated.</param>
        /// <returns>A signature validation result indicating success or failure with details.</returns>
        Task<SignatureValidationResult> ValidateSignatureAsync(Request request);
    }
}
