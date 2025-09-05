using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureService.Controllers;

/// <summary>
/// Controller for RAGS (Robust Anti-replay Governance Signature) validation system
/// Provides quantum-resistant signature generation and validation with nonce replay protection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SignatureValidationController : ControllerBase
{
    private readonly ISignatureValidationService _signatureValidationService;
    private readonly ILogger<SignatureValidationController> _logger;

    public SignatureValidationController(
        ISignatureValidationService signatureValidationService,
        ILogger<SignatureValidationController> logger)
    {
        _signatureValidationService = signatureValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a signature for a service using quantum-resistant algorithms
    /// </summary>
    /// <param name="request">The signature generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature generation response</returns>
    /// <response code="200">Signature generated successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="404">Service not found or not ready</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateSignatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenerateSignatureResponse>> GenerateSignatureAsync(
        [FromBody] GenerateSignatureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received signature generation request for service {ServiceName} using {Algorithm}", 
                request.ServiceName, request.Algorithm);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _signatureValidationService.GenerateSignatureAsync(request, cancellationToken);

            if (!response.Success)
            {
                if (response.ErrorMessage?.Contains("not found") == true || 
                    response.ErrorMessage?.Contains("not ready") == true)
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature for service {ServiceName}", request.ServiceName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Validates a signature using RAGS system with replay protection
    /// </summary>
    /// <param name="request">The signature validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature validation response</returns>
    /// <response code="200">Signature validation completed</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateSignatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidateSignatureResponse>> ValidateSignatureAsync(
        [FromBody] ValidateSignatureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received signature validation request for service {ServiceName} using {Algorithm}", 
                request.ServiceName, request.Algorithm);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _signatureValidationService.ValidateSignatureAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating signature for service {ServiceName}", request.ServiceName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates signatures for multiple services in bulk
    /// </summary>
    /// <param name="request">The bulk signature generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The bulk signature generation response</returns>
    /// <response code="200">Bulk signature generation completed</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("bulk-generate")]
    [ProducesResponseType(typeof(BulkGenerateSignatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkGenerateSignatureResponse>> BulkGenerateSignaturesAsync(
        [FromBody] BulkGenerateSignatureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received bulk signature generation request for {Algorithm} algorithm", 
                request.Algorithm);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _signatureValidationService.BulkGenerateSignaturesAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk signature generation");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Checks if a nonce has been used before for replay protection
    /// </summary>
    /// <param name="request">The nonce check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The nonce check response</returns>
    /// <response code="200">Nonce check completed</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("check-nonce")]
    [ProducesResponseType(typeof(CheckNonceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CheckNonceResponse>> CheckNonceAsync(
        [FromBody] CheckNonceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received nonce check request for service {ServiceName}", request.ServiceName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _signatureValidationService.CheckNonceAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking nonce for service {ServiceName}", request.ServiceName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets signature validation metrics and performance statistics
    /// </summary>
    /// <param name="serviceName">Optional service name filter</param>
    /// <param name="algorithm">Optional algorithm filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The signature metrics response</returns>
    /// <response code="200">Metrics retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SignatureMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SignatureMetricsResponse>> GetSignatureMetricsAsync(
        [FromQuery] string? serviceName = null,
        [FromQuery] string? algorithm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received signature metrics request");

            var request = new GetSignatureMetricsRequest
            {
                ServiceName = serviceName,
                Algorithm = algorithm,
                StartDate = startDate,
                EndDate = endDate
            };

            var response = await _signatureValidationService.GetSignatureMetricsAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving signature metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all supported cryptographic algorithms
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported algorithms</returns>
    /// <response code="200">Algorithms retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("algorithms")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<string>>> GetSupportedAlgorithmsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received request for supported algorithms");

            var algorithms = await _signatureValidationService.GetSupportedAlgorithmsAsync(cancellationToken);

            return Ok(algorithms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported algorithms");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates a cryptographically secure nonce for replay protection
    /// </summary>
    /// <param name="serviceName">The service name to generate nonce for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique nonce string</returns>
    /// <response code="200">Nonce generated successfully</response>
    /// <response code="400">Invalid service name</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("generate-nonce")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GenerateNonceAsync(
        [FromBody] string serviceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest("Service name is required");
            }

            _logger.LogInformation("Received nonce generation request for service {ServiceName}", serviceName);

            var nonce = await _signatureValidationService.GenerateNonceAsync(serviceName, cancellationToken);

            return Ok(nonce);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating nonce for service {ServiceName}", serviceName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Validates that a service exists and has the required keys for signature operations
    /// </summary>
    /// <param name="serviceName">The service name to validate</param>
    /// <param name="algorithm">The algorithm to validate support for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is ready for signature operations</returns>
    /// <response code="200">Service readiness check completed</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("validate-service-readiness")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateServiceReadinessAsync(
        [FromQuery] string serviceName,
        [FromQuery] string algorithm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(algorithm))
            {
                return BadRequest("Service name and algorithm are required");
            }

            _logger.LogInformation("Received service readiness validation for {ServiceName} with {Algorithm}", 
                serviceName, algorithm);

            var isReady = await _signatureValidationService.ValidateServiceReadinessAsync(
                serviceName, algorithm, cancellationToken);

            return Ok(isReady);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating service readiness for {ServiceName}", serviceName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Clears expired nonces from the replay protection cache
    /// </summary>
    /// <param name="maxAgeHours">Maximum age of nonces to keep in hours (older nonces will be cleared)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of nonces cleared</returns>
    /// <response code="200">Nonce cleanup completed</response>
    /// <response code="400">Invalid max age parameter</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("clear-expired-nonces")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> ClearExpiredNoncesAsync(
        [FromQuery] int maxAgeHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (maxAgeHours <= 0)
            {
                return BadRequest("Max age hours must be greater than 0");
            }

            _logger.LogInformation("Received request to clear nonces older than {MaxAgeHours} hours", maxAgeHours);

            var maxAge = TimeSpan.FromHours(maxAgeHours);
            var clearedCount = await _signatureValidationService.ClearExpiredNoncesAsync(maxAge, cancellationToken);

            return Ok(clearedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing expired nonces");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the health status of the signature validation system
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status information</returns>
    /// <response code="200">Health status retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<string, object>>> GetHealthStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received health status request");

            var healthStatus = await _signatureValidationService.GetHealthStatusAsync(cancellationToken);

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health status");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Performs a comprehensive test of the RAGS signature validation system
    /// </summary>
    /// <param name="serviceName">Optional service name to test (if not provided, uses first available service)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive test results</returns>
    /// <response code="200">System test completed</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("system-test")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<string, object>>> SystemTestAsync(
        [FromQuery] string? serviceName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received system test request");

            var testResults = new Dictionary<string, object>
            {
                ["test_started"] = DateTime.UtcNow,
                ["test_service"] = serviceName ?? "auto-selected"
            };

            // Get supported algorithms
            var algorithms = await _signatureValidationService.GetSupportedAlgorithmsAsync(cancellationToken);
            testResults["supported_algorithms"] = algorithms;

            // Test each algorithm
            var algorithmTests = new Dictionary<string, object>();
            
            foreach (var algorithm in algorithms)
            {
                var algorithmResult = new Dictionary<string, object>();
                
                try
                {
                    // Generate signature
                    var generateRequest = new GenerateSignatureRequest
                    {
                        ServiceName = serviceName ?? "TestService",
                        Message = $"Test message for {algorithm} at {DateTime.UtcNow}",
                        Algorithm = algorithm
                    };

                    var generateResponse = await _signatureValidationService.GenerateSignatureAsync(generateRequest, cancellationToken);
                    algorithmResult["generation_success"] = generateResponse.Success;
                    algorithmResult["generation_time"] = generateResponse.GenerationTime.TotalMilliseconds;

                    if (generateResponse.Success)
                    {
                        // Validate signature
                        var validateRequest = new ValidateSignatureRequest
                        {
                            ServiceName = generateResponse.ServiceName,
                            Message = generateRequest.Message,
                            Signature = generateResponse.Signature,
                            Algorithm = algorithm,
                            Nonce = generateResponse.Nonce,
                            BlockchainAddress = generateResponse.BlockchainAddress
                        };

                        var validateResponse = await _signatureValidationService.ValidateSignatureAsync(validateRequest, cancellationToken);
                        algorithmResult["validation_success"] = validateResponse.IsValid;
                        algorithmResult["validation_time"] = validateResponse.ValidationTime.TotalMilliseconds;
                        algorithmResult["nonce_reused"] = validateResponse.IsNonceReused;

                        // Test replay protection
                        var replayResponse = await _signatureValidationService.ValidateSignatureAsync(validateRequest, cancellationToken);
                        algorithmResult["replay_protection_working"] = replayResponse.IsNonceReused;
                    }
                    else
                    {
                        algorithmResult["generation_error"] = generateResponse.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    algorithmResult["error"] = ex.Message;
                }

                algorithmTests[algorithm] = algorithmResult;
            }

            testResults["algorithm_tests"] = algorithmTests;
            testResults["test_completed"] = DateTime.UtcNow;

            // Get health status
            var healthStatus = await _signatureValidationService.GetHealthStatusAsync(cancellationToken);
            testResults["health_status"] = healthStatus;

            return Ok(testResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system test");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", message = ex.Message });
        }
    }
}
