using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Interfaces;
using System.Text;

namespace QuantumLedger.Hub.Middleware;

/// <summary>
/// Middleware for authenticating requests using substitution keys (user-controlled delegation keys)
/// </summary>
public class SubstitutionKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISubstitutionKeyService _substitutionKeyService;
    private readonly ILogger<SubstitutionKeyAuthenticationMiddleware> _logger;

    public SubstitutionKeyAuthenticationMiddleware(
        RequestDelegate next,
        ISubstitutionKeyService substitutionKeyService,
        ILogger<SubstitutionKeyAuthenticationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _substitutionKeyService = substitutionKeyService ?? throw new ArgumentNullException(nameof(substitutionKeyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the HTTP request and verifies substitution key authentication
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Check if substitution key authentication headers are present
            var substitutionSignature = context.Request.Headers["X-Substitution-Signature"].FirstOrDefault();
            var substitutionKeyId = context.Request.Headers["X-Substitution-Key-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(substitutionSignature) && !string.IsNullOrEmpty(substitutionKeyId))
            {
                _logger.LogDebug("Processing substitution key authentication for key {SubstitutionKeyId}", substitutionKeyId);

                // Read and cache the request body for signature verification
                var requestBody = await ReadRequestBodyAsync(context.Request);
                
                if (requestBody != null)
                {
                    // Verify the substitution key signature
                    var verificationResult = await _substitutionKeyService.VerifySubstitutionKeyRequestAsync(
                        requestBody, 
                        substitutionSignature, 
                        substitutionKeyId);

                    if (verificationResult.Success)
                    {
                        // Authentication successful - add authenticated address to context
                        context.Items["AuthenticatedAddress"] = verificationResult.AuthenticatedAddress;
                        context.Items["SubstitutionKeyId"] = substitutionKeyId;
                        context.Items["AuthenticationMethod"] = "SubstitutionKey";
                        context.Items["AuthenticationTime"] = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Substitution key authentication successful for address {Address} using key {SubstitutionKeyId}",
                            verificationResult.AuthenticatedAddress, substitutionKeyId);

                        // Add custom response headers for debugging (in development only)
                        if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                        {
                            context.Response.Headers.Add("X-Auth-Method", "SubstitutionKey");
                            context.Response.Headers.Add("X-Auth-Address", verificationResult.AuthenticatedAddress);
                        }
                    }
                    else
                    {
                        // Authentication failed
                        _logger.LogWarning(
                            "Substitution key authentication failed for key {SubstitutionKeyId}: {ErrorMessage}",
                            substitutionKeyId, verificationResult.ErrorMessage);

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        
                        var errorResponse = new
                        {
                            error = "Authentication failed",
                            message = "Invalid substitution key signature or authorization",
                            details = verificationResult.ErrorMessage,
                            timestamp = DateTime.UtcNow,
                            requestId = context.TraceIdentifier
                        };

                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("Could not read request body for substitution key verification");
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid request body for signature verification");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(substitutionSignature) || !string.IsNullOrEmpty(substitutionKeyId))
            {
                // Partial substitution key headers provided
                _logger.LogWarning("Incomplete substitution key authentication headers provided");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Both X-Substitution-Signature and X-Substitution-Key-Id headers are required for substitution key authentication");
                return;
            }
            // If no substitution key headers are provided, continue without authentication
            // This allows endpoints to be optionally authenticated or use other authentication methods

            // Continue to the next middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in substitution key authentication middleware");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "Internal server error",
                message = "An error occurred during authentication",
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
        }
    }

    /// <summary>
    /// Reads the request body and enables rewinding for subsequent middleware
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <returns>The request body as byte array</returns>
    private async Task<byte[]?> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // Enable buffering to allow multiple reads of the request body
            request.EnableBuffering();

            // Read the request body
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            // Reset the stream position for subsequent middleware
            request.Body.Position = 0;

            // Convert to bytes for signature verification
            return Encoding.UTF8.GetBytes(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading request body for substitution key verification");
            return null;
        }
    }
}

/// <summary>
/// Extension methods for adding substitution key authentication middleware
/// </summary>
public static class SubstitutionKeyAuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Adds substitution key authentication middleware to the pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseSubstitutionKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SubstitutionKeyAuthenticationMiddleware>();
    }

    /// <summary>
    /// Adds substitution key authentication middleware to the pipeline with custom configuration
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <param name="options">Configuration options for the middleware</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseSubstitutionKeyAuthentication(
        this IApplicationBuilder builder, 
        SubstitutionKeyAuthenticationOptions options)
    {
        // Store options in the service collection for the middleware to use
        builder.ApplicationServices.GetService<IServiceCollection>()?.AddSingleton(options);
        return builder.UseMiddleware<SubstitutionKeyAuthenticationMiddleware>();
    }
}

/// <summary>
/// Configuration options for substitution key authentication middleware
/// </summary>
public class SubstitutionKeyAuthenticationOptions
{
    /// <summary>
    /// Gets or sets whether to require substitution key authentication for all requests
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;

    /// <summary>
    /// Gets or sets the paths that require substitution key authentication
    /// </summary>
    public List<string> RequiredPaths { get; set; } = new();

    /// <summary>
    /// Gets or sets the paths that are excluded from substitution key authentication
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to add debug headers in development mode
    /// </summary>
    public bool AddDebugHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum age of signatures to accept (for replay protection)
    /// </summary>
    public TimeSpan MaxSignatureAge { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether to log authentication attempts
    /// </summary>
    public bool LogAuthenticationAttempts { get; set; } = true;
}

/// <summary>
/// Helper methods for working with substitution key authentication in controllers
/// </summary>
public static class SubstitutionKeyAuthenticationHelper
{
    /// <summary>
    /// Gets the authenticated address from the HTTP context
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The authenticated address, or null if not authenticated</returns>
    public static string? GetAuthenticatedAddress(HttpContext context)
    {
        return context.Items["AuthenticatedAddress"] as string;
    }

    /// <summary>
    /// Gets the substitution key ID used for authentication
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The substitution key ID, or null if not authenticated with substitution key</returns>
    public static string? GetSubstitutionKeyId(HttpContext context)
    {
        return context.Items["SubstitutionKeyId"] as string;
    }

    /// <summary>
    /// Checks if the request was authenticated using a substitution key
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>True if authenticated with substitution key</returns>
    public static bool IsAuthenticatedWithSubstitutionKey(HttpContext context)
    {
        return context.Items["AuthenticationMethod"] as string == "SubstitutionKey";
    }

    /// <summary>
    /// Gets the authentication time
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>The authentication time, or null if not authenticated</returns>
    public static DateTime? GetAuthenticationTime(HttpContext context)
    {
        return context.Items["AuthenticationTime"] as DateTime?;
    }

    /// <summary>
    /// Verifies that the authenticated address matches the expected address
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="expectedAddress">The expected address</param>
    /// <returns>True if the addresses match</returns>
    public static bool VerifyAuthenticatedAddress(HttpContext context, string expectedAddress)
    {
        var authenticatedAddress = GetAuthenticatedAddress(context);
        return !string.IsNullOrEmpty(authenticatedAddress) && authenticatedAddress == expectedAddress;
    }
}
