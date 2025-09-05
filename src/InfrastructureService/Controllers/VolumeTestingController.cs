using Microsoft.AspNetCore.Mvc;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;

namespace InfrastructureService.Controllers;

/// <summary>
/// Controller for volume testing operations
/// Provides endpoints for high-volume testing of address generation and signature validation
/// Supports testing up to 1 million operations with comprehensive performance monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VolumeTestingController : ControllerBase
{
    private readonly IVolumeTestingService _volumeTestingService;
    private readonly ILogger<VolumeTestingController> _logger;

    public VolumeTestingController(
        IVolumeTestingService volumeTestingService,
        ILogger<VolumeTestingController> logger)
    {
        _volumeTestingService = volumeTestingService;
        _logger = logger;
    }

    /// <summary>
    /// Starts a volume test for address generation
    /// </summary>
    /// <param name="request">The volume address test configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test start response with test ID and configuration</returns>
    /// <response code="200">Test started successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="503">System not ready for volume testing</response>
    [HttpPost("address-test")]
    public async Task<ActionResult<StartVolumeTestResponse>> StartVolumeAddressTest(
        [FromBody] StartVolumeAddressTestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting volume address test for {AddressCount} addresses", request.AddressCount);

            var response = await _volumeTestingService.StartVolumeAddressTestAsync(request, cancellationToken);

            if (!response.Success)
            {
                return response.ErrorMessage?.Contains("not ready") == true 
                    ? StatusCode(503, response) 
                    : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting volume address test");
            return StatusCode(500, new StartVolumeTestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error occurred while starting test"
            });
        }
    }

    /// <summary>
    /// Starts a volume test for signature operations
    /// </summary>
    /// <param name="request">The volume signature test configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test start response with test ID and configuration</returns>
    /// <response code="200">Test started successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="503">System not ready for volume testing</response>
    [HttpPost("signature-test")]
    public async Task<ActionResult<StartVolumeTestResponse>> StartVolumeSignatureTest(
        [FromBody] StartVolumeSignatureTestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting volume signature test for {SignatureCount} signatures", request.SignatureCount);

            var response = await _volumeTestingService.StartVolumeSignatureTestAsync(request, cancellationToken);

            if (!response.Success)
            {
                return response.ErrorMessage?.Contains("not ready") == true 
                    ? StatusCode(503, response) 
                    : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting volume signature test");
            return StatusCode(500, new StartVolumeTestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error occurred while starting test"
            });
        }
    }

    /// <summary>
    /// Starts a comprehensive stress test
    /// </summary>
    /// <param name="request">The stress test configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test start response with test ID and configuration</returns>
    /// <response code="200">Test started successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="503">System not ready for stress testing</response>
    [HttpPost("stress-test")]
    public async Task<ActionResult<StartVolumeTestResponse>> StartStressTest(
        [FromBody] StartStressTestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting stress test for {Duration} minutes at {TargetOPS} ops/sec", 
                request.DurationMinutes, request.TargetOperationsPerSecond);

            var response = await _volumeTestingService.StartStressTestAsync(request, cancellationToken);

            if (!response.Success)
            {
                return response.ErrorMessage?.Contains("not ready") == true 
                    ? StatusCode(503, response) 
                    : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stress test");
            return StatusCode(500, new StartVolumeTestResponse
            {
                Success = false,
                ErrorMessage = "Internal server error occurred while starting test"
            });
        }
    }

    /// <summary>
    /// Gets the status of a running volume test
    /// </summary>
    /// <param name="testId">The test identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current test status and progress information</returns>
    /// <response code="200">Test status retrieved successfully</response>
    /// <response code="404">Test not found</response>
    [HttpGet("{testId}/status")]
    public async Task<ActionResult<VolumeTestStatusResponse>> GetVolumeTestStatus(
        string testId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetVolumeTestStatusRequest { TestId = testId };
            var response = await _volumeTestingService.GetVolumeTestStatusAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Test not found: {TestId}", testId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test status for {TestId}", testId);
            return StatusCode(500, new { message = "Internal server error occurred while getting test status" });
        }
    }

    /// <summary>
    /// Gets the results of a completed volume test
    /// </summary>
    /// <param name="testId">The test identifier</param>
    /// <param name="includeDetailedMetrics">Whether to include detailed performance metrics</param>
    /// <param name="includeSamples">Whether to include sample data</param>
    /// <param name="sampleCount">Number of samples to include (if includeSamples is true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete test results with performance metrics and analysis</returns>
    /// <response code="200">Test results retrieved successfully</response>
    /// <response code="404">Test results not found</response>
    [HttpGet("{testId}/results")]
    public async Task<ActionResult<VolumeTestResultsResponse>> GetVolumeTestResults(
        string testId,
        [FromQuery] bool includeDetailedMetrics = true,
        [FromQuery] bool includeSamples = false,
        [FromQuery] int sampleCount = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetVolumeTestResultsRequest
            {
                TestId = testId,
                IncludeDetailedMetrics = includeDetailedMetrics,
                IncludeSamples = includeSamples,
                SampleCount = sampleCount
            };

            var response = await _volumeTestingService.GetVolumeTestResultsAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Test results not found: {TestId}", testId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test results for {TestId}", testId);
            return StatusCode(500, new { message = "Internal server error occurred while getting test results" });
        }
    }

    /// <summary>
    /// Stops a running volume test
    /// </summary>
    /// <param name="testId">The test identifier</param>
    /// <param name="request">The stop test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status of the stop operation</returns>
    /// <response code="200">Test stopped successfully</response>
    /// <response code="404">Test not found or already completed</response>
    [HttpPost("{testId}/stop")]
    public async Task<ActionResult<object>> StopVolumeTest(
        string testId,
        [FromBody] StopVolumeTestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            request.TestId = testId; // Ensure consistency
            var success = await _volumeTestingService.StopVolumeTestAsync(request, cancellationToken);

            if (!success)
            {
                return NotFound(new { message = $"Test {testId} not found or already completed" });
            }

            _logger.LogInformation("Stopped volume test {TestId}", testId);
            return Ok(new { message = "Test stopped successfully", testId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping test {TestId}", testId);
            return StatusCode(500, new { message = "Internal server error occurred while stopping test" });
        }
    }

    /// <summary>
    /// Gets a list of all volume tests (active and completed)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of volume test summaries</returns>
    /// <response code="200">Test list retrieved successfully</response>
    [HttpGet("tests")]
    public async Task<ActionResult<List<VolumeTestSummary>>> GetAllVolumeTests(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tests = await _volumeTestingService.GetAllVolumeTestsAsync(cancellationToken);
            return Ok(tests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all volume tests");
            return StatusCode(500, new { message = "Internal server error occurred while getting test list" });
        }
    }

    /// <summary>
    /// Cleans up old test data
    /// </summary>
    /// <param name="maxAgeDays">Maximum age of test data to keep (in days)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tests cleaned up</returns>
    /// <response code="200">Cleanup completed successfully</response>
    [HttpDelete("cleanup")]
    public async Task<ActionResult<object>> CleanupOldTestData(
        [FromQuery] int maxAgeDays = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var maxAge = TimeSpan.FromDays(maxAgeDays);
            var cleanedCount = await _volumeTestingService.CleanupOldTestDataAsync(maxAge, cancellationToken);

            _logger.LogInformation("Cleaned up {Count} old test records", cleanedCount);
            return Ok(new { message = "Cleanup completed", cleanedCount, maxAgeDays });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test data cleanup");
            return StatusCode(500, new { message = "Internal server error occurred during cleanup" });
        }
    }

    /// <summary>
    /// Gets current system performance metrics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current system performance and resource utilization</returns>
    /// <response code="200">Performance metrics retrieved successfully</response>
    [HttpGet("system/performance")]
    public async Task<ActionResult<ResourceUtilization>> GetSystemPerformanceMetrics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _volumeTestingService.GetSystemPerformanceMetricsAsync(cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system performance metrics");
            return StatusCode(500, new { message = "Internal server error occurred while getting performance metrics" });
        }
    }

    /// <summary>
    /// Validates system readiness for volume testing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System readiness status and recommendations</returns>
    /// <response code="200">System readiness validated successfully</response>
    [HttpGet("system/readiness")]
    public async Task<ActionResult<Dictionary<string, object>>> ValidateSystemReadiness(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var readiness = await _volumeTestingService.ValidateSystemReadinessAsync(cancellationToken);
            return Ok(readiness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating system readiness");
            return StatusCode(500, new { message = "Internal server error occurred while validating system readiness" });
        }
    }

    /// <summary>
    /// Estimates test completion time
    /// </summary>
    /// <param name="operationCount">Number of operations to perform</param>
    /// <param name="operationType">Type of operation (Address, Signature, Stress)</param>
    /// <param name="parallelWorkers">Number of parallel workers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated completion time</returns>
    /// <response code="200">Estimation completed successfully</response>
    [HttpGet("estimate")]
    public async Task<ActionResult<object>> EstimateTestCompletionTime(
        [FromQuery] long operationCount,
        [FromQuery] string operationType = "Address",
        [FromQuery] int parallelWorkers = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var estimatedTime = await _volumeTestingService.EstimateTestCompletionTimeAsync(
                operationCount, operationType, parallelWorkers, cancellationToken);

            return Ok(new
            {
                operationCount,
                operationType,
                parallelWorkers,
                estimatedCompletionTime = estimatedTime,
                estimatedCompletionTimeFormatted = estimatedTime.ToString(@"hh\:mm\:ss")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating test completion time");
            return StatusCode(500, new { message = "Internal server error occurred while estimating completion time" });
        }
    }

    /// <summary>
    /// Gets recommended test parameters
    /// </summary>
    /// <param name="targetOperations">Target number of operations</param>
    /// <param name="operationType">Type of operation (Address, Signature, Stress)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recommended test parameters based on system capabilities</returns>
    /// <response code="200">Recommendations generated successfully</response>
    [HttpGet("recommendations")]
    public async Task<ActionResult<Dictionary<string, object>>> GetRecommendedTestParameters(
        [FromQuery] long targetOperations,
        [FromQuery] string operationType = "Address",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = await _volumeTestingService.GetRecommendedTestParametersAsync(
                targetOperations, operationType, cancellationToken);

            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended test parameters");
            return StatusCode(500, new { message = "Internal server error occurred while getting recommendations" });
        }
    }

    /// <summary>
    /// Runs a quick system test to validate volume testing capabilities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System test results</returns>
    /// <response code="200">System test completed successfully</response>
    [HttpPost("system-test")]
    public async Task<ActionResult<object>> RunSystemTest(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Running volume testing system test");

            // Run a small test to validate system capabilities
            var testRequest = new StartVolumeAddressTestRequest
            {
                AddressCount = 10,
                BatchSize = 5,
                ParallelWorkers = 2,
                NetworkType = "MULTICHAIN",
                TestId = $"SYSTEST_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            var startResponse = await _volumeTestingService.StartVolumeAddressTestAsync(testRequest, cancellationToken);

            if (!startResponse.Success)
            {
                return Ok(new
                {
                    systemTestPassed = false,
                    message = "System test failed to start",
                    error = startResponse.ErrorMessage,
                    timestamp = DateTime.UtcNow
                });
            }

            // Wait a moment for the test to progress
            await Task.Delay(2000, cancellationToken);

            // Get test status
            var statusRequest = new GetVolumeTestStatusRequest { TestId = startResponse.TestId };
            var statusResponse = await _volumeTestingService.GetVolumeTestStatusAsync(statusRequest, cancellationToken);

            return Ok(new
            {
                systemTestPassed = true,
                message = "System test completed successfully",
                testId = startResponse.TestId,
                testStatus = statusResponse.Status,
                operationsCompleted = statusResponse.OperationsCompleted,
                systemReadiness = await _volumeTestingService.ValidateSystemReadinessAsync(cancellationToken),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system test");
            return Ok(new
            {
                systemTestPassed = false,
                message = "System test encountered an error",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets health status of the volume testing service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status information</returns>
    /// <response code="200">Health check completed</response>
    [HttpGet("health")]
    public async Task<ActionResult<object>> GetHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var readiness = await _volumeTestingService.ValidateSystemReadinessAsync(cancellationToken);
            var allTests = await _volumeTestingService.GetAllVolumeTestsAsync(cancellationToken);

            return Ok(new
            {
                status = "healthy",
                systemReady = (bool)readiness["ready"],
                activeTests = allTests.Count(t => t.Status == "Running" || t.Status == "Starting"),
                completedTests = allTests.Count(t => t.Status == "Completed"),
                failedTests = allTests.Count(t => t.Status == "Failed"),
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            return StatusCode(500, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
