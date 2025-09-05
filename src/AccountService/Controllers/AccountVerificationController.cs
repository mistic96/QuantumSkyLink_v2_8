using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using AccountService.Data.Entities;
using System.Security.Claims;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AccountVerificationController : ControllerBase
{
    private readonly IAccountVerificationService _verificationService;
    private readonly ILogger<AccountVerificationController> _logger;

    public AccountVerificationController(
        IAccountVerificationService verificationService,
        ILogger<AccountVerificationController> logger)
    {
        _verificationService = verificationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new account verification
    /// </summary>
    /// <param name="request">Verification creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created verification information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountVerificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountVerificationResponse>> CreateVerification(
        [FromBody] CreateAccountVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var verification = await _verificationService.CreateVerificationAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetVerification), new { id = verification.Id }, verification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating verification for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get verification by ID
    /// </summary>
    /// <param name="id">Verification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification information</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountVerificationResponse>> GetVerification(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the verification
            if (!await _verificationService.UserOwnsVerificationAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this verification");
            }

            var verification = await _verificationService.GetVerificationAsync(id, cancellationToken);
            return Ok(verification);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification {VerificationId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get verifications for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of verifications</returns>
    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AccountVerificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountVerificationResponse>>> GetAccountVerifications(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verifications = await _verificationService.GetAccountVerificationsAsync(accountId, cancellationToken);
            return Ok(verifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verifications for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get verifications for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user verifications</returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(IEnumerable<AccountVerificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountVerificationResponse>>> GetUserVerifications(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var verifications = await _verificationService.GetUserVerificationsAsync(userId, cancellationToken);
            return Ok(verifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verifications for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update verification status
    /// </summary>
    /// <param name="id">Verification ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated verification</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AccountVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountVerificationResponse>> UpdateVerificationStatus(
        Guid id,
        [FromBody] UpdateVerificationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            
            // Verify user owns the verification
            if (!await _verificationService.UserOwnsVerificationAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this verification");
            }

            var verification = await _verificationService.UpdateVerificationStatusAsync(id, Enum.Parse<VerificationStatus>(request.Status), request.Notes, cancellationToken);
            return Ok(verification);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating verification status for {VerificationId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Initiate KYC workflow for an account
    /// </summary>
    /// <param name="request">KYC initiation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>KYC workflow information</returns>
    [HttpPost("kyc/initiate")]
    [ProducesResponseType(typeof(KycWorkflowResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KycWorkflowResponse>> InitiateKycWorkflow(
        [FromBody] InitiateKycWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var workflow = await _verificationService.InitiateKycWorkflowAsync(request.AccountId, request.KycLevel, cancellationToken);
            return CreatedAtAction(nameof(GetKycWorkflowStatus), new { accountId = request.AccountId }, workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating KYC workflow for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get KYC workflow status for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>KYC workflow status</returns>
    [HttpGet("kyc/status/{accountId:guid}")]
    [ProducesResponseType(typeof(KycWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KycWorkflowResponse>> GetKycWorkflowStatus(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _verificationService.GetKycWorkflowStatusAsync(accountId, cancellationToken);
            return Ok(status);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KYC workflow status for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Process KYC results for an account
    /// </summary>
    /// <param name="request">KYC results processing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result</returns>
    [HttpPost("kyc/process-results")]
    [ProducesResponseType(typeof(KycProcessingResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KycProcessingResultResponse>> ProcessKycResults(
        [FromBody] ProcessKycResultsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var kycResults = string.IsNullOrEmpty(request.KycResults) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.KycResults) ?? new Dictionary<string, object>();
                
            var complianceData = string.IsNullOrEmpty(request.ComplianceData) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.ComplianceData) ?? new Dictionary<string, object>();

            if (!request.AccountId.HasValue)
            {
                return BadRequest(new { message = "AccountId is required" });
            }

            var result = await _verificationService.ProcessKycResultsAsync(
                request.AccountId!.Value, 
                kycResults, 
                complianceData, 
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing KYC results for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Upload verification document
    /// </summary>
    /// <param name="request">Document upload request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result</returns>
    [HttpPost("documents/upload")]
    [ProducesResponseType(typeof(DocumentUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentUploadResponse>> UploadDocument(
        [FromBody] UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.DocumentData == null || request.DocumentData.Length == 0)
            {
                return BadRequest(new { message = "Document data is required and cannot be empty" });
            }

            var result = await _verificationService.UploadDocumentAsync(
                request.VerificationId, 
                request.DocumentType, 
                request.DocumentData, 
                request.FileName, 
                cancellationToken);

            return CreatedAtAction(nameof(GetVerification), new { id = request.VerificationId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for verification {VerificationId}", request.VerificationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete verification document
    /// </summary>
    /// <param name="verificationId">Verification ID</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{verificationId:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        Guid verificationId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the verification
            if (!await _verificationService.UserOwnsVerificationAsync(userId, verificationId, cancellationToken))
            {
                return Forbid("You do not have access to this verification");
            }

            var result = await _verificationService.DeleteDocumentAsync(verificationId, documentId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Document not found" });
            }

            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} for verification {VerificationId}", documentId, verificationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Perform compliance check for an account
    /// </summary>
    /// <param name="request">Compliance check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check result</returns>
    [HttpPost("compliance/check")]
    [ProducesResponseType(typeof(ComplianceCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComplianceCheckResponse>> PerformComplianceCheck(
        [FromBody] PerformComplianceCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var additionalData = string.IsNullOrEmpty(request.AdditionalData) 
                ? null 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.AdditionalData);

            var result = await _verificationService.PerformComplianceCheckAsync(
                request.AccountId, 
                request.CheckType, 
                additionalData, 
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing compliance check for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get compliance issues for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance issues</returns>
    [HttpGet("compliance/issues/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceIssueResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceIssueResponse>>> GetComplianceIssues(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var issues = await _verificationService.GetComplianceIssuesAsync(accountId, cancellationToken);
            return Ok(issues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance issues for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get compliance status for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance status</returns>
    [HttpGet("compliance/status/{accountId:guid}")]
    [ProducesResponseType(typeof(ComplianceStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComplianceStatusResponse>> GetComplianceStatus(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _verificationService.GetComplianceStatusAsync(accountId, cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance status for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get verification requirements for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification requirements</returns>
    [HttpGet("requirements/{accountId:guid}")]
    [ProducesResponseType(typeof(VerificationRequirementsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationRequirementsResponse>> GetVerificationRequirements(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requirements = await _verificationService.GetVerificationRequirementsAsync(accountId, cancellationToken);
            return Ok(requirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification requirements for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get verification status summary for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification status summary</returns>
    [HttpGet("status/summary/{accountId:guid}")]
    [ProducesResponseType(typeof(VerificationStatusSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationStatusSummaryResponse>> GetVerificationStatusSummary(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _verificationService.GetVerificationStatusSummaryAsync(accountId, cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status summary for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }
}
