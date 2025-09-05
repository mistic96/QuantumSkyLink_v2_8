using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Compatibility.User;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility
{
    [ApiController]
    [Route("User")]
    public class UserCompatibilityController : ControllerBase
    {
        private readonly IUserCompatibilityService _userCompatibilityService;
        private readonly ILogger<UserCompatibilityController> _logger;

        public UserCompatibilityController(
            IUserCompatibilityService userCompatibilityService,
            ILogger<UserCompatibilityController> logger)
        {
            _userCompatibilityService = userCompatibilityService;
            _logger = logger;
        }

        [HttpGet("GetUser")]
        public async Task<ActionResult<GetUserResponse>> GetUser([FromQuery] string email, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting user: {Email}", email);
                var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var response = await _userCompatibilityService.GetUserAsync(email, clientIpAddress, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return StatusCode(500, new GetUserResponse { UserId = "", EmailAddress = email ?? "", AccountStatus = "Error" });
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<ActionResult<UserOperationResponse>> UpdateUser([FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating user");
                var response = await _userCompatibilityService.UpdateUserAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new UserOperationResponse { Success = false, Message = "Internal server error" });
            }
        }

        [HttpPost("AcceptTerms")]
        public async Task<ActionResult<UserOperationResponse>> AcceptTerms([FromBody] AcceptTermsRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Accepting terms for user");
                var response = await _userCompatibilityService.AcceptedTermsAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting terms");
                return StatusCode(500, new UserOperationResponse { Success = false, Message = "Internal server error" });
            }
        }

        [HttpPost("RegisterDevice")]
        public async Task<ActionResult<UserDeviceRegistrationResponse>> RegisterDevice([FromBody] UserDeviceRegistrationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Registering device");
                var response = await _userCompatibilityService.RegisterDeviceAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device");
                return StatusCode(500, new UserDeviceRegistrationResponse { IsSuccessful = false, Message = "Internal server error" });
            }
        }
    }
}
