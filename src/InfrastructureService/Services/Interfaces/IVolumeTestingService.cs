using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;

namespace InfrastructureService.Services.Interfaces;

/// <summary>
/// Interface for volume testing service
/// Provides high-volume testing capabilities for address generation and signature validation
/// </summary>
public interface IVolumeTestingService
{
    /// <summary>
    /// Starts a volume test for address generation
    /// </summary>
    /// <param name="request">The volume address test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The test start response</returns>
    Task<StartVolumeTestResponse> StartVolumeAddressTestAsync(StartVolumeAddressTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a volume test for signature operations
    /// </summary>
    /// <param name="request">The volume signature test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The test start response</returns>
    Task<StartVolumeTestResponse> StartVolumeSignatureTestAsync(StartVolumeSignatureTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a comprehensive stress test
    /// </summary>
    /// <param name="request">The stress test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The test start response</returns>
    Task<StartVolumeTestResponse> StartStressTestAsync(StartStressTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a running volume test
    /// </summary>
    /// <param name="request">The status request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The test status response</returns>
    Task<VolumeTestStatusResponse> GetVolumeTestStatusAsync(GetVolumeTestStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the results of a completed volume test
    /// </summary>
    /// <param name="request">The results request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The test results response</returns>
    Task<VolumeTestResultsResponse> GetVolumeTestResultsAsync(GetVolumeTestResultsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running volume test
    /// </summary>
    /// <param name="request">The stop test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the test was stopped successfully</returns>
    Task<bool> StopVolumeTestAsync(StopVolumeTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all volume tests (active and completed)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of volume test summaries</returns>
    Task<List<VolumeTestSummary>> GetAllVolumeTestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up completed volume test data older than specified age
    /// </summary>
    /// <param name="maxAge">Maximum age of test data to keep</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tests cleaned up</returns>
    Task<int> CleanupOldTestDataAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets system performance metrics during testing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current system performance metrics</returns>
    Task<ResourceUtilization> GetSystemPerformanceMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates system readiness for volume testing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System readiness status and recommendations</returns>
    Task<Dictionary<string, object>> ValidateSystemReadinessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates test completion time based on current performance
    /// </summary>
    /// <param name="operationCount">Number of operations to perform</param>
    /// <param name="operationType">Type of operation (Address, Signature, etc.)</param>
    /// <param name="parallelWorkers">Number of parallel workers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated completion time</returns>
    Task<TimeSpan> EstimateTestCompletionTimeAsync(long operationCount, string operationType, int parallelWorkers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recommended test parameters based on system capabilities
    /// </summary>
    /// <param name="targetOperations">Target number of operations</param>
    /// <param name="operationType">Type of operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recommended test parameters</returns>
    Task<Dictionary<string, object>> GetRecommendedTestParametersAsync(long targetOperations, string operationType, CancellationToken cancellationToken = default);
}
