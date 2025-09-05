using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GovernanceService.Services.Interfaces;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;
using GovernanceService.Data.Entities;

namespace GovernanceService.Controllers;

/// <summary>
/// Controller for governance rule configuration and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GovernanceRuleController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<GovernanceRuleController> _logger;

    public GovernanceRuleController(
        IGovernanceService governanceService,
        ILogger<GovernanceRuleController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new governance rule
    /// </summary>
    /// <param name="request">Governance rule creation request</param>
    /// <returns>Created governance rule details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(GovernanceRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GovernanceRuleResponse>> CreateGovernanceRule(
        [FromBody] CreateGovernanceRuleRequest request)
    {
        try
        {
            _logger.LogInformation("Creating governance rule: {RuleName}, Type: {ApplicableType}, Creator: {CreatedById}", 
                request.RuleName, request.ApplicableType, request.CreatedById);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate governance rule parameters
            if (request.MinimumQuorum < 0 || request.MinimumQuorum > 100)
            {
                return BadRequest("Minimum quorum must be between 0 and 100 percent");
            }

            if (request.ApprovalThreshold < 0 || request.ApprovalThreshold > 100)
            {
                return BadRequest("Approval threshold must be between 0 and 100 percent");
            }

            if (request.VotingPeriod <= TimeSpan.Zero)
            {
                return BadRequest("Voting period must be greater than zero");
            }

            if (request.ExecutionDelay < TimeSpan.Zero)
            {
                return BadRequest("Execution delay cannot be negative");
            }

            if (request.RequiresMultiSig && (!request.RequiredSignatures.HasValue || request.RequiredSignatures.Value < 1))
            {
                return BadRequest("Required signatures must be specified and greater than 0 when multi-signature is required");
            }

            var result = await _governanceService.CreateGovernanceRuleAsync(request);
            
            _logger.LogInformation("Governance rule created successfully: {RuleId}", result.Id);
            return CreatedAtAction(nameof(GetGovernanceRule), new { proposalType = request.ApplicableType }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid governance rule creation request: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Governance rule creation failed: {Request}", request);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized governance rule creation attempt: {CreatedById}", request.CreatedById);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating governance rule: {Request}", request);
            return StatusCode(500, "An error occurred while creating the governance rule");
        }
    }

    /// <summary>
    /// Get governance rule by proposal type
    /// </summary>
    /// <param name="proposalType">Proposal type</param>
    /// <returns>Governance rule details</returns>
    [HttpGet("{proposalType}")]
    [ProducesResponseType(typeof(GovernanceRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GovernanceRuleResponse>> GetGovernanceRule(
        [FromRoute] string proposalType)
    {
        try
        {
            _logger.LogInformation("Getting governance rule for proposal type: {ProposalType}", proposalType);

            if (string.IsNullOrWhiteSpace(proposalType))
            {
                return BadRequest("Proposal type cannot be empty");
            }

            if (!Enum.TryParse<ProposalType>(proposalType, true, out var parsedType))
            {
                return BadRequest($"Invalid proposal type: {proposalType}");
            }

            var rule = await _governanceService.GetGovernanceRuleAsync(parsedType);
            
            if (rule == null)
            {
                return NotFound($"Governance rule not found for proposal type: {proposalType}");
            }

            return Ok(rule);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid governance rule request: {ProposalType}", proposalType);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance rule: {ProposalType}", proposalType);
            return StatusCode(500, "An error occurred while retrieving the governance rule");
        }
    }

    /// <summary>
    /// Get all governance rules
    /// </summary>
    /// <returns>List of all governance rules</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<GovernanceRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<GovernanceRuleResponse>>> GetAllGovernanceRules()
    {
        try
        {
            _logger.LogInformation("Getting all governance rules");

            var rules = await _governanceService.GetAllGovernanceRulesAsync();
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all governance rules");
            return StatusCode(500, "An error occurred while retrieving governance rules");
        }
    }

    /// <summary>
    /// Update governance rule
    /// </summary>
    /// <param name="ruleId">Governance rule ID</param>
    /// <param name="request">Governance rule update request</param>
    /// <returns>Updated governance rule details</returns>
    [HttpPut("{ruleId}")]
    [ProducesResponseType(typeof(GovernanceRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GovernanceRuleResponse>> UpdateGovernanceRule(
        [FromRoute] Guid ruleId,
        [FromBody] UpdateGovernanceRuleRequest request)
    {
        try
        {
            _logger.LogInformation("Updating governance rule: {RuleId} by user: {UpdatedById}", ruleId, request.UpdatedById);

            if (ruleId == Guid.Empty)
            {
                return BadRequest("Rule ID cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate governance rule parameters if provided
            if (request.MinimumQuorum.HasValue && (request.MinimumQuorum.Value < 0 || request.MinimumQuorum.Value > 100))
            {
                return BadRequest("Minimum quorum must be between 0 and 100 percent");
            }

            if (request.ApprovalThreshold.HasValue && (request.ApprovalThreshold.Value < 0 || request.ApprovalThreshold.Value > 100))
            {
                return BadRequest("Approval threshold must be between 0 and 100 percent");
            }

            if (request.VotingPeriod.HasValue && request.VotingPeriod.Value <= TimeSpan.Zero)
            {
                return BadRequest("Voting period must be greater than zero");
            }

            if (request.ExecutionDelay.HasValue && request.ExecutionDelay.Value < TimeSpan.Zero)
            {
                return BadRequest("Execution delay cannot be negative");
            }

            if (request.RequiresMultiSig.HasValue && request.RequiresMultiSig.Value && 
                (!request.RequiredSignatures.HasValue || request.RequiredSignatures.Value < 1))
            {
                return BadRequest("Required signatures must be specified and greater than 0 when multi-signature is required");
            }

            var result = await _governanceService.UpdateGovernanceRuleAsync(ruleId, request);
            
            if (result == null)
            {
                return NotFound($"Governance rule not found: {ruleId}");
            }

            _logger.LogInformation("Governance rule updated successfully: {RuleId}", ruleId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid governance rule update request: {RuleId}, {Request}", ruleId, request);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Governance rule update failed: {RuleId}", ruleId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized governance rule update attempt: {RuleId}, {UpdatedById}", ruleId, request.UpdatedById);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating governance rule: {RuleId}", ruleId);
            return StatusCode(500, "An error occurred while updating the governance rule");
        }
    }

    /// <summary>
    /// Deactivate governance rule
    /// </summary>
    /// <param name="ruleId">Governance rule ID</param>
    /// <param name="userId">User ID performing the deactivation</param>
    /// <returns>Success status</returns>
    [HttpDelete("{ruleId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> DeactivateGovernanceRule(
        [FromRoute] Guid ruleId,
        [FromBody] Guid userId)
    {
        try
        {
            _logger.LogInformation("Deactivating governance rule: {RuleId} by user: {UserId}", ruleId, userId);

            if (ruleId == Guid.Empty)
            {
                return BadRequest("Rule ID cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var result = await _governanceService.DeactivateGovernanceRuleAsync(ruleId, userId);
            
            if (!result)
            {
                return NotFound($"Governance rule not found or already deactivated: {ruleId}");
            }

            _logger.LogInformation("Governance rule deactivated successfully: {RuleId}", ruleId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid governance rule deactivation request: {RuleId}, {UserId}", ruleId, userId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Governance rule deactivation failed: {RuleId}", ruleId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized governance rule deactivation attempt: {RuleId}, {UserId}", ruleId, userId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating governance rule: {RuleId}", ruleId);
            return StatusCode(500, "An error occurred while deactivating the governance rule");
        }
    }

    /// <summary>
    /// Get governance rule configuration summary
    /// </summary>
    /// <returns>Summary of all governance rule configurations</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetGovernanceRuleSummary()
    {
        try
        {
            _logger.LogInformation("Getting governance rule configuration summary");

            var rules = await _governanceService.GetAllGovernanceRulesAsync();
            
            var summary = new
            {
                TotalRules = rules.Count,
                ActiveRules = rules.Count(r => r.IsActive),
                InactiveRules = rules.Count(r => !r.IsActive),
                RulesByType = rules.GroupBy(r => r.ApplicableType)
                    .ToDictionary(g => g.Key.ToString(), g => new
                    {
                        Count = g.Count(),
                        Active = g.Count(r => r.IsActive),
                        AverageQuorum = g.Average(r => r.MinimumQuorum),
                        AverageApprovalThreshold = g.Average(r => r.ApprovalThreshold),
                        AverageVotingPeriodHours = g.Average(r => r.VotingPeriod.TotalHours),
                        RequireMultiSig = g.Count(r => r.RequiresMultiSig)
                    }),
                Configuration = new
                {
                    OverallAverageQuorum = rules.Where(r => r.IsActive).Average(r => r.MinimumQuorum),
                    OverallAverageApprovalThreshold = rules.Where(r => r.IsActive).Average(r => r.ApprovalThreshold),
                    OverallAverageVotingPeriodHours = rules.Where(r => r.IsActive).Average(r => r.VotingPeriod.TotalHours),
                    MultiSigRequiredCount = rules.Count(r => r.IsActive && r.RequiresMultiSig),
                    DelegationAllowedCount = rules.Count(r => r.IsActive && r.AllowDelegation)
                }
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance rule summary");
            return StatusCode(500, "An error occurred while retrieving governance rule summary");
        }
    }

    /// <summary>
    /// Validate governance rule configuration
    /// </summary>
    /// <param name="request">Governance rule creation request to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> ValidateGovernanceRule(
        [FromBody] CreateGovernanceRuleRequest request)
    {
        try
        {
            _logger.LogInformation("Validating governance rule configuration: {RuleName}, Type: {ApplicableType}", 
                request.RuleName, request.ApplicableType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResults = new List<string>();
            var warnings = new List<string>();

            // Validate quorum and approval thresholds
            if (request.MinimumQuorum < 0 || request.MinimumQuorum > 100)
            {
                validationResults.Add("Minimum quorum must be between 0 and 100 percent");
            }
            else if (request.MinimumQuorum < 10)
            {
                warnings.Add("Low quorum requirement may lead to decisions with limited participation");
            }

            if (request.ApprovalThreshold < 0 || request.ApprovalThreshold > 100)
            {
                validationResults.Add("Approval threshold must be between 0 and 100 percent");
            }
            else if (request.ApprovalThreshold < 50)
            {
                warnings.Add("Approval threshold below 50% may allow minority decisions");
            }

            // Validate timing parameters
            if (request.VotingPeriod <= TimeSpan.Zero)
            {
                validationResults.Add("Voting period must be greater than zero");
            }
            else if (request.VotingPeriod < TimeSpan.FromHours(24))
            {
                warnings.Add("Short voting period may limit participation");
            }
            else if (request.VotingPeriod > TimeSpan.FromDays(30))
            {
                warnings.Add("Long voting period may delay governance decisions");
            }

            if (request.ExecutionDelay < TimeSpan.Zero)
            {
                validationResults.Add("Execution delay cannot be negative");
            }
            else if (request.ExecutionDelay > TimeSpan.FromDays(7))
            {
                warnings.Add("Long execution delay may reduce governance responsiveness");
            }

            // Validate multi-signature requirements
            if (request.RequiresMultiSig)
            {
                if (!request.RequiredSignatures.HasValue || request.RequiredSignatures.Value < 1)
                {
                    validationResults.Add("Required signatures must be specified and greater than 0 when multi-signature is required");
                }
                else if (request.RequiredSignatures.Value > 20)
                {
                    warnings.Add("High signature requirement may make execution difficult");
                }
            }

            // Validate proposal type specific requirements
            switch (request.ApplicableType)
            {
                case ProposalType.Constitutional:
                    if (request.ApprovalThreshold < 66.67m)
                    {
                        warnings.Add("Constitutional changes typically require supermajority (67%+)");
                    }
                    if (!request.RequiresMultiSig)
                    {
                        warnings.Add("Constitutional changes typically require multi-signature approval");
                    }
                    break;

                case ProposalType.Emergency:
                    if (request.VotingPeriod > TimeSpan.FromHours(24))
                    {
                        warnings.Add("Emergency proposals typically have shorter voting periods");
                    }
                    if (request.ExecutionDelay > TimeSpan.FromHours(6))
                    {
                        warnings.Add("Emergency proposals typically have minimal execution delay");
                    }
                    break;

                case ProposalType.Treasury:
                    if (request.MinimumTokensRequired == null || request.MinimumTokensRequired.Value == 0)
                    {
                        warnings.Add("Treasury proposals typically require minimum token holdings");
                    }
                    if (request.ProposalDeposit == null || request.ProposalDeposit.Value == 0)
                    {
                        warnings.Add("Treasury proposals typically require a proposal deposit");
                    }
                    break;
            }

            var result = new
            {
                IsValid = validationResults.Count == 0,
                Errors = validationResults,
                Warnings = warnings,
                Recommendations = new List<string>
                {
                    validationResults.Count == 0 ? "Configuration appears valid" : "Please address validation errors",
                    warnings.Count > 0 ? "Consider reviewing warnings for optimal governance" : "No warnings detected"
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating governance rule: {Request}", request);
            return StatusCode(500, "An error occurred while validating the governance rule");
        }
    }
}
