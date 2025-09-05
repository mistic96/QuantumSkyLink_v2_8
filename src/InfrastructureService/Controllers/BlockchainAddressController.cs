using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureService.Controllers
{
    /// <summary>
    /// Controller for blockchain address generation and management operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BlockchainAddressController : ControllerBase
    {
        private readonly ILogger<BlockchainAddressController> _logger;
        private readonly IBlockchainAddressService _blockchainAddressService;

        /// <summary>
        /// Initializes a new instance of the BlockchainAddressController class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="blockchainAddressService">The blockchain address service.</param>
        public BlockchainAddressController(
            ILogger<BlockchainAddressController> logger,
            IBlockchainAddressService blockchainAddressService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainAddressService = blockchainAddressService ?? throw new ArgumentNullException(nameof(blockchainAddressService));
        }

        /// <summary>
        /// Generates a blockchain address for a specific service.
        /// </summary>
        /// <param name="request">The address generation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The generated address response.</returns>
        /// <response code="200">Address generated successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(GenerateAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GenerateAddressResponse>> GenerateAddressAsync(
            [FromBody] GenerateAddressRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating {NetworkType} address for service: {ServiceName}", 
                    request.NetworkType, request.ServiceName);

                var response = await _blockchainAddressService.GenerateAddressAsync(request, cancellationToken);

                _logger.LogInformation("Successfully generated {NetworkType} address for service {ServiceName}: {Address}",
                    request.NetworkType, request.ServiceName, response.Address);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for address generation: {ServiceName}, {NetworkType}", 
                    request.ServiceName, request.NetworkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate {NetworkType} address for service: {ServiceName}", 
                    request.NetworkType, request.ServiceName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while generating the blockchain address",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Generates blockchain addresses for multiple services.
        /// </summary>
        /// <param name="request">The bulk address generation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk generation response.</returns>
        /// <response code="200">Addresses generated successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("generate/bulk")]
        [ProducesResponseType(typeof(BulkGenerateAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkGenerateAddressResponse>> BulkGenerateAddressesAsync(
            [FromBody] BulkGenerateAddressRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting bulk generation of {Count} {NetworkType} addresses", 
                    request.ServiceNames.Count, request.NetworkType);

                var response = await _blockchainAddressService.BulkGenerateAddressesAsync(request, cancellationToken);

                _logger.LogInformation("Successfully completed bulk generation of {Count} {NetworkType} addresses",
                    response.TotalGenerated, request.NetworkType);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for bulk address generation: {NetworkType}", request.NetworkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk generate {NetworkType} addresses", request.NetworkType);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while generating blockchain addresses",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Generates blockchain addresses for all registered services.
        /// </summary>
        /// <param name="networkType">The blockchain network type (MultiChain, Ethereum).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk generation response for all services.</returns>
        /// <response code="200">Addresses generated successfully for all services.</response>
        /// <response code="400">Invalid network type.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("generate/all-services/{networkType}")]
        [ProducesResponseType(typeof(BulkGenerateAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkGenerateAddressResponse>> GenerateAddressesForAllServicesAsync(
            [FromRoute] string networkType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating {NetworkType} addresses for all registered services", networkType);

                var response = await _blockchainAddressService.GenerateAddressesForAllServicesAsync(networkType, cancellationToken);

                _logger.LogInformation("Successfully generated {NetworkType} addresses for {Count} services",
                    networkType, response.TotalGenerated);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid network type for all-services address generation: {NetworkType}", networkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate {NetworkType} addresses for all services", networkType);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while generating addresses for all services",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Validates a blockchain address.
        /// </summary>
        /// <param name="request">The address validation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation response.</returns>
        /// <response code="200">Address validation completed.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidateAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ValidateAddressResponse>> ValidateAddressAsync(
            [FromBody] ValidateAddressRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Validating {NetworkType} address: {Address}", request.NetworkType, request.Address);

                var response = await _blockchainAddressService.ValidateAddressAsync(request, cancellationToken);

                _logger.LogDebug("Address validation completed for {NetworkType} address: {Address}, Valid: {IsValid}",
                    request.NetworkType, request.Address, response.IsValid);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for address validation: {Address}, {NetworkType}", 
                    request.Address, request.NetworkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while validating the blockchain address",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets detailed information about a blockchain address.
        /// </summary>
        /// <param name="request">The address info request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The address information response.</returns>
        /// <response code="200">Address information retrieved successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="404">Address not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("info")]
        [ProducesResponseType(typeof(AddressInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddressInfoResponse>> GetAddressInfoAsync(
            [FromBody] GetAddressInfoRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting address info for {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);

                var response = await _blockchainAddressService.GetAddressInfoAsync(request, cancellationToken);

                _logger.LogDebug("Successfully retrieved address info for {NetworkType} address: {Address}",
                    request.NetworkType, request.Address);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for address info: {Address}, {NetworkType}", 
                    request.Address, request.NetworkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get address info for {NetworkType} address: {Address}", 
                    request.NetworkType, request.Address);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving address information",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets network statistics for a specific blockchain network.
        /// </summary>
        /// <param name="networkType">The blockchain network type (MultiChain, Ethereum).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The network statistics response.</returns>
        /// <response code="200">Network statistics retrieved successfully.</response>
        /// <response code="400">Invalid network type.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("network-stats/{networkType}")]
        [ProducesResponseType(typeof(NetworkStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NetworkStatsResponse>> GetNetworkStatsAsync(
            [FromRoute] string networkType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting network stats for: {NetworkType}", networkType);

                var response = await _blockchainAddressService.GetNetworkStatsAsync(networkType, cancellationToken);

                _logger.LogDebug("Successfully retrieved network stats for: {NetworkType}", networkType);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid network type for stats: {NetworkType}", networkType);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network stats for: {NetworkType}", networkType);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving network statistics",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Gets address generation performance metrics.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The performance metrics response.</returns>
        /// <response code="200">Performance metrics retrieved successfully.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(AddressGenerationMetricsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddressGenerationMetricsResponse>> GetGenerationMetricsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting address generation metrics");

                var response = await _blockchainAddressService.GetGenerationMetricsAsync(cancellationToken);

                _logger.LogDebug("Successfully retrieved address generation metrics");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get address generation metrics");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving performance metrics",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Health check endpoint for the blockchain address controller.
        /// </summary>
        /// <returns>Health status.</returns>
        /// <response code="200">Service is healthy.</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "BlockchainAddressController",
                Timestamp = DateTime.UtcNow,
                SupportedNetworks = new[] { "MultiChain", "Ethereum" }
            });
        }
    }
}
