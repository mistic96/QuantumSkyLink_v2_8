using Microsoft.AspNetCore.Mvc;
using ComplianceService.Services.Interfaces;

namespace ComplianceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IComplianceService _complianceService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IComplianceService complianceService, ILogger<WebhookController> logger)
    {
        _complianceService = complianceService;
        _logger = logger;
    }

    [HttpPost("complycube")]
    public async Task<IActionResult> ComplyCubeWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            
            var signature = Request.Headers["X-ComplyCube-Signature"].FirstOrDefault();
            
            _logger.LogInformation("Received ComplyCube webhook with signature: {Signature}", signature);
            
            await _complianceService.ProcessComplyCubeWebhookAsync(payload, signature ?? "");
            
            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ComplyCube webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ComplianceService.Webhooks", timestamp = DateTime.UtcNow });
    }
}
