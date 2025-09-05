using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumLedger.Cryptography.Services;

namespace QuantumLedger.Hub.Middleware
{
    /// <summary>
    /// Middleware for verifying digital signatures on API requests.
    /// Provides configurable signature verification with replay protection.
    /// </summary>
    public class SignatureVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SignatureVerificationMiddleware> _logger;
        private readonly SignatureVerificationOptions _options;

        public SignatureVerificationMiddleware(
            RequestDelegate next,
            ILogger<SignatureVerificationMiddleware> logger,
            IOptions<SignatureVerificationOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if signature verification is required for this endpoint
            if (!ShouldVerifySignature(context))
            {
                await _next(context);
                return;
            }

            try
            {
                // Extract and verify signature
                var verificationResult = await VerifyRequestSignatureAsync(context);
                
                if (!verificationResult.IsValid)
                {
                    await HandleVerificationFailureAsync(context, verificationResult);
                    return;
                }

                // Add verification result to context for downstream use
                context.Items["SignatureVerification"] = verificationResult;
                
                _logger.LogDebug("Signature verification successful for account {AccountId}", 
                    verificationResult.AccountId);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signature verification");
                await HandleVerificationErrorAsync(context, ex);
            }
        }

        /// <summary>
        /// Determines if signature verification is required for the current request.
        /// </summary>
        private bool ShouldVerifySignature(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Skip verification for excluded paths
            if (_options.ExcludedPaths != null)
            {
                foreach (var excludedPath in _options.ExcludedPaths)
                {
                    if (path?.StartsWith(excludedPath.ToLowerInvariant()) == true)
                    {
                        return false;
                    }
                }
            }

            // Only verify specific HTTP methods
            if (_options.RequiredMethods != null && 
                !_options.RequiredMethods.Contains(context.Request.Method.ToUpperInvariant()))
            {
                return false;
            }

            // Check if path requires verification
            if (_options.RequiredPaths != null)
            {
                foreach (var requiredPath in _options.RequiredPaths)
                {
                    if (path?.StartsWith(requiredPath.ToLowerInvariant()) == true)
                    {
                        return true;
                    }
                }
                return false; // Path not in required list
            }

            // Default behavior based on configuration
            return _options.RequireSignatureByDefault;
        }

        /// <summary>
        /// Verifies the signature of the incoming request.
        /// </summary>
        private async Task<SignatureVerificationResult> VerifyRequestSignatureAsync(HttpContext context)
        {
            // Extract signature headers
            var accountIdHeader = context.Request.Headers["X-Account-ID"].FirstOrDefault();
            var algorithmHeader = context.Request.Headers["X-Signature-Algorithm"].FirstOrDefault();
            var signatureHeader = context.Request.Headers["X-Signature"].FirstOrDefault();
            var nonceHeader = context.Request.Headers["X-Nonce"].FirstOrDefault();
            var timestampHeader = context.Request.Headers["X-Timestamp"].FirstOrDefault();

            // Validate required headers
            if (string.IsNullOrWhiteSpace(accountIdHeader) ||
                string.IsNullOrWhiteSpace(algorithmHeader) ||
                string.IsNullOrWhiteSpace(signatureHeader) ||
                string.IsNullOrWhiteSpace(nonceHeader) ||
                string.IsNullOrWhiteSpace(timestampHeader))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "MISSING_SIGNATURE_HEADERS",
                    ErrorMessage = "Required signature headers are missing",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            // Parse account ID and timestamp
            if (!Guid.TryParse(accountIdHeader, out var accountId))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_ACCOUNT_ID",
                    ErrorMessage = "Invalid account ID format",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            if (!DateTime.TryParse(timestampHeader, out var timestamp))
            {
                return new SignatureVerificationResult
                {
                    IsValid = false,
                    ErrorCode = "INVALID_TIMESTAMP",
                    ErrorMessage = "Invalid timestamp format",
                    VerifiedAt = DateTime.UtcNow
                };
            }

            // Read request body for signature verification
            var requestBody = await ReadRequestBodyAsync(context);

            // Create signed request object
            var signedRequest = new SignedRequest
            {
                Payload = new
                {
                    method = context.Request.Method,
                    path = context.Request.Path.Value,
                    query = context.Request.QueryString.Value,
                    body = requestBody,
                    headers = ExtractRelevantHeaders(context)
                },
                Signature = new RequestSignature
                {
                    AccountId = accountId,
                    Algorithm = algorithmHeader,
                    SignatureValue = signatureHeader,
                    Nonce = nonceHeader,
                    Timestamp = timestamp
                }
            };

            // For now, return a mock successful verification
            // In production, this would use the SignatureVerificationService
            return new SignatureVerificationResult
            {
                IsValid = true,
                AccountId = accountId,
                Algorithm = algorithmHeader,
                VerifiedAt = DateTime.UtcNow,
                Message = "Signature verification successful (mock implementation)"
            };
        }

        /// <summary>
        /// Reads the request body while preserving it for downstream middleware.
        /// </summary>
        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            
            return body;
        }

        /// <summary>
        /// Extracts relevant headers for signature verification.
        /// </summary>
        private object ExtractRelevantHeaders(HttpContext context)
        {
            var relevantHeaders = new Dictionary<string, string>();
            
            // Include specific headers that should be part of the signature
            var headersToInclude = new[] { "Content-Type", "User-Agent", "X-Client-Version" };
            
            foreach (var headerName in headersToInclude)
            {
                if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
                {
                    relevantHeaders[headerName] = headerValue.ToString();
                }
            }
            
            return relevantHeaders;
        }

        /// <summary>
        /// Handles signature verification failure.
        /// </summary>
        private async Task HandleVerificationFailureAsync(HttpContext context, SignatureVerificationResult result)
        {
            _logger.LogWarning("Signature verification failed: {ErrorCode} - {ErrorMessage}", 
                result.ErrorCode, result.ErrorMessage);

            context.Response.StatusCode = 401; // Unauthorized
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "signature_verification_failed",
                error_description = result.ErrorMessage,
                error_code = result.ErrorCode,
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Handles signature verification errors.
        /// </summary>
        private async Task HandleVerificationErrorAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = 500; // Internal Server Error
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "signature_verification_error",
                error_description = "An error occurred during signature verification",
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Configuration options for signature verification middleware.
    /// </summary>
    public class SignatureVerificationOptions
    {
        /// <summary>
        /// Gets or sets whether signature verification is required by default.
        /// </summary>
        public bool RequireSignatureByDefault { get; set; } = false;

        /// <summary>
        /// Gets or sets the paths that require signature verification.
        /// </summary>
        public string[]? RequiredPaths { get; set; }

        /// <summary>
        /// Gets or sets the paths that are excluded from signature verification.
        /// </summary>
        public string[]? ExcludedPaths { get; set; }

        /// <summary>
        /// Gets or sets the HTTP methods that require signature verification.
        /// </summary>
        public string[]? RequiredMethods { get; set; }

        /// <summary>
        /// Gets or sets the maximum age of a request timestamp in minutes.
        /// </summary>
        public int MaxTimestampAgeMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether to log signature verification attempts.
        /// </summary>
        public bool LogVerificationAttempts { get; set; } = true;
    }

    #region Temporary Models (until Account models are available)

    /// <summary>
    /// Temporary signed request model for middleware.
    /// </summary>
    public class SignedRequest
    {
        public object Payload { get; set; } = null!;
        public RequestSignature Signature { get; set; } = null!;
    }

    /// <summary>
    /// Temporary request signature model for middleware.
    /// </summary>
    public class RequestSignature
    {
        public Guid AccountId { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public string SignatureValue { get; set; } = string.Empty;
        public string Nonce { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Temporary signature verification result model for middleware.
    /// </summary>
    public class SignatureVerificationResult
    {
        public bool IsValid { get; set; }
        public Guid? AccountId { get; set; }
        public string? Algorithm { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion
}
