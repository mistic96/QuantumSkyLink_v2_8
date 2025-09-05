using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureService.Controllers
{
    /// <summary>
    /// Controller for multi-network blockchain operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MultiNetworkController : ControllerBase
    {
        private readonly ILogger<MultiNetworkController> _logger;
        private readonly IMultiNetworkService _multiNetworkService;

        /// <summary>
        /// Initializes a new instance of the MultiNetworkController class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="multiNetworkService">The multi-network service.</param>
        public MultiNetworkController(
            ILogger<MultiNetworkController> logger,
            IMultiNetworkService multiNetworkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _multiNetworkService = multiNetworkService ?? throw new ArgumentNullException(nameof(multiNetworkService));
        }

        /// <summary>
        /// Generates addresses across multiple blockchain networks for a service.
        /// </summary>
        /// <param name="request">The multi-network generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network generation response with addresses for each network.</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(MultiNetworkGenerateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MultiNetworkGenerateResponse>> GenerateMultiNetworkAddressesAsync(
            [FromBody] MultiNetworkGenerateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received multi-network address generation request for service {ServiceName} across {NetworkCount} networks",
                    request.ServiceName, request.NetworkTypes.Count);

                var response = await _multiNetworkService.GenerateMultiNetworkAddressesAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully generated addresses for service {ServiceName} across {NetworkCount} networks",
                    request.ServiceName, response.NetworkAddresses.Count);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for multi-network address generation");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate multi-network addresses for service {ServiceName}",
                    request?.ServiceName ?? "Unknown");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while generating multi-network addresses",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Validates addresses across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The multi-network validation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network validation response with results for each address.</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(MultiNetworkValidateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MultiNetworkValidateResponse>> ValidateMultiNetworkAddressesAsync(
            [FromBody] MultiNetworkValidateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received multi-network address validation request for {AddressCount} addresses",
                    request.Addresses.Count);

                var response = await _multiNetworkService.ValidateMultiNetworkAddressesAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully validated {AddressCount} addresses across multiple networks",
                    response.ValidationResults.Count);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for multi-network address validation");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate multi-network addresses");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while validating multi-network addresses",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Compares performance metrics across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The network performance comparison request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network performance comparison response with metrics and analysis.</returns>
        [HttpPost("performance/compare")]
        [ProducesResponseType(typeof(NetworkPerformanceComparisonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NetworkPerformanceComparisonResponse>> CompareNetworkPerformanceAsync(
            [FromBody] NetworkPerformanceComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received network performance comparison request for {NetworkCount} networks",
                    request.NetworkTypes.Count);

                var response = await _multiNetworkService.CompareNetworkPerformanceAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully compared performance for {NetworkCount} networks",
                    response.NetworkMetrics.Count);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for network performance comparison");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compare network performance");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while comparing network performance",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves cross-network metadata for an address.
        /// </summary>
        /// <param name="request">The cross-network metadata request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Cross-network metadata response with related addresses and relationships.</returns>
        [HttpPost("metadata/cross-network")]
        [ProducesResponseType(typeof(CrossNetworkMetadataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CrossNetworkMetadataResponse>> GetCrossNetworkMetadataAsync(
            [FromBody] CrossNetworkMetadataRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received cross-network metadata request for address {Address} on {NetworkType}",
                    request.Address, request.NetworkType);

                var response = await _multiNetworkService.GetCrossNetworkMetadataAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully retrieved cross-network metadata for address {Address}",
                    request.Address);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for cross-network metadata");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cross-network metadata for address {Address}",
                    request?.Address ?? "Unknown");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving cross-network metadata",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Performs comprehensive testing across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The multi-network test request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network test response with results for each network.</returns>
        [HttpPost("test")]
        [ProducesResponseType(typeof(MultiNetworkTestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MultiNetworkTestResponse>> RunMultiNetworkTestAsync(
            [FromBody] MultiNetworkTestRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received multi-network test request for {NetworkCount} networks with {OperationsPerNetwork} operations each",
                    request.NetworkTypes.Count, request.OperationsPerNetwork);

                var response = await _multiNetworkService.RunMultiNetworkTestAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully completed multi-network test for {NetworkCount} networks",
                    response.NetworkResults.Count);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for multi-network test");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run multi-network test");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while running multi-network test",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Configures network-specific settings for a blockchain network.
        /// </summary>
        /// <param name="request">The network configuration request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network configuration response with applied settings.</returns>
        [HttpPost("configure")]
        [ProducesResponseType(typeof(NetworkConfigurationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NetworkConfigurationResponse>> ConfigureNetworkAsync(
            [FromBody] NetworkConfigurationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received network configuration request for {NetworkType}",
                    request.NetworkType);

                var response = await _multiNetworkService.ConfigureNetworkAsync(request, cancellationToken);
                
                _logger.LogInformation("Successfully configured network {NetworkType}",
                    request.NetworkType);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for network configuration");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure network {NetworkType}",
                    request?.NetworkType ?? "Unknown");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while configuring the network",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets the list of supported blockchain networks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of supported network types with their capabilities.</returns>
        [HttpGet("networks/supported")]
        [ProducesResponseType(typeof(List<SupportedNetwork>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SupportedNetwork>>> GetSupportedNetworksAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received request for supported networks");

                var response = await _multiNetworkService.GetSupportedNetworksAsync(cancellationToken);
                
                _logger.LogInformation("Successfully retrieved {NetworkCount} supported networks",
                    response.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get supported networks");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving supported networks",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets network-specific configuration for a blockchain network.
        /// </summary>
        /// <param name="networkType">The network type to get configuration for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network configuration details.</returns>
        [HttpGet("networks/{networkType}/configuration")]
        [ProducesResponseType(typeof(NetworkConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NetworkConfiguration>> GetNetworkConfigurationAsync(
            [FromRoute] string networkType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(networkType))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Network type cannot be null or empty",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("Received request for network configuration for {NetworkType}",
                    networkType);

                var response = await _multiNetworkService.GetNetworkConfigurationAsync(networkType, cancellationToken);
                
                _logger.LogInformation("Successfully retrieved configuration for network {NetworkType}",
                    networkType);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for network configuration");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network configuration for {NetworkType}",
                    networkType);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving network configuration",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets network health status for all supported networks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network health status for each supported network.</returns>
        [HttpGet("networks/health")]
        [ProducesResponseType(typeof(Dictionary<string, NetworkHealthStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Dictionary<string, NetworkHealthStatus>>> GetNetworkHealthStatusAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received request for network health status");

                var response = await _multiNetworkService.GetNetworkHealthStatusAsync(cancellationToken);
                
                _logger.LogInformation("Successfully retrieved health status for {NetworkCount} networks",
                    response.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network health status");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving network health status",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets a quick overview of multi-network capabilities and status.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network system overview.</returns>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(MultiNetworkOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MultiNetworkOverviewResponse>> GetMultiNetworkOverviewAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received request for multi-network overview");

                var supportedNetworks = await _multiNetworkService.GetSupportedNetworksAsync(cancellationToken);
                var healthStatus = await _multiNetworkService.GetNetworkHealthStatusAsync(cancellationToken);

                var overview = new MultiNetworkOverviewResponse
                {
                    TotalSupportedNetworks = supportedNetworks.Count,
                    HealthyNetworks = healthStatus.Count(h => h.Value.IsHealthy),
                    SupportedNetworkTypes = supportedNetworks.Select(n => n.NetworkType).ToList(),
                    AvailableCapabilities = supportedNetworks.SelectMany(n => n.Capabilities).Distinct().ToList(),
                    SystemStatus = healthStatus.Values.All(h => h.IsHealthy) ? "Healthy" : "Degraded",
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully generated multi-network overview");

                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get multi-network overview");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while generating multi-network overview",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

    /// <summary>
    /// Response for multi-network system overview.
    /// </summary>
    public class MultiNetworkOverviewResponse
    {
        /// <summary>
        /// Gets or sets the total number of supported networks.
        /// </summary>
        public int TotalSupportedNetworks { get; set; }

        /// <summary>
        /// Gets or sets the number of healthy networks.
        /// </summary>
        public int HealthyNetworks { get; set; }

        /// <summary>
        /// Gets or sets the list of supported network types.
        /// </summary>
        public List<string> SupportedNetworkTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the available capabilities across all networks.
        /// </summary>
        public List<string> AvailableCapabilities { get; set; } = new();

        /// <summary>
        /// Gets or sets the overall system status.
        /// </summary>
        public string SystemStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the overview was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
