using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumLedger.Hub.Middleware;

namespace QuantumLedger.Hub.Attributes
{
    /// <summary>
    /// Attribute to require signature verification for specific controllers or actions.
    /// Provides fine-grained control over which endpoints require signature verification.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireSignatureAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Gets or sets the required signature algorithm.
        /// If specified, only requests with this algorithm will be accepted.
        /// </summary>
        public string? RequiredAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets whether to allow requests without signatures.
        /// If true, requests without signatures will be allowed but logged.
        /// </summary>
        public bool AllowUnsigned { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum age of the request timestamp in minutes.
        /// Overrides the global setting for this specific endpoint.
        /// </summary>
        public int MaxTimestampAgeMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether to log signature verification attempts for this endpoint.
        /// </summary>
        public bool LogAttempts { get; set; } = true;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireSignatureAttribute>>();
            
            try
            {
                // Check if signature verification was already performed by middleware
                var verificationResult = context.HttpContext.Items["SignatureVerification"] as SignatureVerificationResult;
                
                if (verificationResult != null)
                {
                    // Signature was already verified by middleware
                    if (!verificationResult.IsValid)
                    {
                        HandleVerificationFailure(context, verificationResult, logger);
                        return;
                    }

                    // Check algorithm requirement if specified
                    if (!string.IsNullOrEmpty(RequiredAlgorithm) && 
                        verificationResult.Algorithm != RequiredAlgorithm)
                    {
                        logger?.LogWarning("Required algorithm {RequiredAlgorithm} but got {ActualAlgorithm}", 
                            RequiredAlgorithm, verificationResult.Algorithm);
                        
                        HandleAlgorithmMismatch(context, logger);
                        return;
                    }

                    // Verification successful
                    if (LogAttempts)
                    {
                        logger?.LogInformation("Signature verification successful for account {AccountId} using {Algorithm}", 
                            verificationResult.AccountId, verificationResult.Algorithm);
                    }
                    
                    return;
                }

                // No signature verification was performed
                if (AllowUnsigned)
                {
                    if (LogAttempts)
                    {
                        logger?.LogInformation("Request allowed without signature verification for {Path}", 
                            context.HttpContext.Request.Path);
                    }
                    return;
                }

                // Signature verification is required but not performed
                logger?.LogWarning("Signature verification required but not performed for {Path}", 
                    context.HttpContext.Request.Path);
                
                HandleMissingSignature(context, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error in signature verification attribute");
                HandleVerificationError(context, ex, logger);
            }
        }

        private void HandleVerificationFailure(AuthorizationFilterContext context, SignatureVerificationResult result, ILogger? logger)
        {
            logger?.LogWarning("Signature verification failed: {ErrorCode} - {ErrorMessage}", 
                result.ErrorCode, result.ErrorMessage);

            context.Result = new UnauthorizedObjectResult(new
            {
                error = "signature_verification_failed",
                error_description = result.ErrorMessage,
                error_code = result.ErrorCode,
                timestamp = DateTime.UtcNow
            });
        }

        private void HandleAlgorithmMismatch(AuthorizationFilterContext context, ILogger? logger)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "algorithm_mismatch",
                error_description = $"Required algorithm '{RequiredAlgorithm}' but different algorithm was used",
                required_algorithm = RequiredAlgorithm,
                timestamp = DateTime.UtcNow
            });
        }

        private void HandleMissingSignature(AuthorizationFilterContext context, ILogger? logger)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "signature_required",
                error_description = "This endpoint requires signature verification",
                required_headers = new[]
                {
                    "X-Account-ID",
                    "X-Signature-Algorithm", 
                    "X-Signature",
                    "X-Nonce",
                    "X-Timestamp"
                },
                timestamp = DateTime.UtcNow
            });
        }

        private void HandleVerificationError(AuthorizationFilterContext context, Exception exception, ILogger? logger)
        {
            context.Result = new ObjectResult(new
            {
                error = "signature_verification_error",
                error_description = "An error occurred during signature verification",
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Attribute to exclude specific controllers or actions from signature verification.
    /// Useful when signature verification is enabled globally but certain endpoints should be excluded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ExcludeFromSignatureVerificationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the reason for excluding this endpoint from signature verification.
        /// Used for documentation and logging purposes.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets whether to log when this exclusion is applied.
        /// </summary>
        public bool LogExclusion { get; set; } = true;
    }

    /// <summary>
    /// Attribute to specify signature verification options for specific algorithms.
    /// Allows fine-tuning of verification behavior per algorithm.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SignatureAlgorithmOptionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the algorithm this configuration applies to.
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this algorithm is allowed for this endpoint.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum timestamp age for this algorithm in minutes.
        /// Some algorithms may require stricter timing requirements.
        /// </summary>
        public int MaxTimestampAgeMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether to require additional validation for this algorithm.
        /// </summary>
        public bool RequireAdditionalValidation { get; set; } = false;

        public SignatureAlgorithmOptionsAttribute(string algorithm)
        {
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        }
    }
}
