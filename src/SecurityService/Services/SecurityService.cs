using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SecurityService.Data;
using SecurityService.Data.Entities;
using SecurityService.Models.Requests;
using SecurityService.Models.Responses;
using SecurityService.Services.Interfaces;

namespace SecurityService.Services;

public class SecurityService : ISecurityService
{
    private readonly SecurityDbContext _context;
    private readonly IUserServiceClient _userService;
    private readonly IAccountServiceClient _accountService;
    private readonly ILogger<SecurityService> _logger;

    public SecurityService(
        SecurityDbContext context,
        IUserServiceClient userService,
        IAccountServiceClient accountService,
        ILogger<SecurityService> logger)
    {
        _context = context;
        _userService = userService;
        _accountService = accountService;
        _logger = logger;
    }

    #region Security Policy Management

    public async Task<SecurityPolicyResponse> CreateSecurityPolicyAsync(Guid userId, CreateSecurityPolicyRequest request, string correlationId)
    {
        _logger.LogInformation("Creating security policy for user {UserId} with correlation {CorrelationId}", userId, correlationId);

        try
        {
            // Validate user exists
            var user = await _userService.GetUserAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User {userId} not found");
            }

            // Check if policy type already exists for user
            var existingPolicy = await _context.SecurityPolicies
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PolicyType == request.PolicyType && p.IsActive);

            if (existingPolicy != null)
            {
                throw new InvalidOperationException($"Active policy of type {request.PolicyType} already exists for user");
            }

            var policy = new SecurityPolicy
            {
                UserId = userId,
                PolicyType = request.PolicyType,
                Name = request.Name,
                Description = request.Description,
                Configuration = JsonSerializer.Serialize(request.Configuration),
                IsRequired = request.IsRequired,
                CreatedBy = userId.ToString(),
                UpdatedBy = userId.ToString(),
                CorrelationId = correlationId
            };

            _context.SecurityPolicies.Add(policy);
            await _context.SaveChangesAsync();

            // Log security event
            await LogSecurityEventAsync(userId, "PolicyCreated", "Medium", $"Security policy {request.Name} created", 
                "SecurityService", "CreatePolicy", "Success", new { PolicyType = request.PolicyType, PolicyName = request.Name }, 
                correlationId, "", "");

            _logger.LogInformation("Security policy {PolicyId} created successfully for user {UserId}", policy.Id, userId);

            return policy.Adapt<SecurityPolicyResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security policy for user {UserId} with correlation {CorrelationId}", userId, correlationId);
            
            await LogSecurityEventAsync(userId, "PolicyCreationFailed", "High", $"Failed to create security policy: {ex.Message}", 
                "SecurityService", "CreatePolicy", "Failure", new { Error = ex.Message }, correlationId, "", "");
            
            throw;
        }
    }

    public async Task<SecurityPolicyResponse> GetSecurityPolicyAsync(Guid policyId, Guid userId)
    {
        var policy = await _context.SecurityPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.UserId == userId);

        if (policy == null)
        {
            throw new ArgumentException($"Security policy {policyId} not found for user {userId}");
        }

        var response = policy.Adapt<SecurityPolicyResponse>();
        
        // Deserialize configuration
        if (!string.IsNullOrEmpty(policy.Configuration))
        {
            response.Configuration = JsonSerializer.Deserialize<object>(policy.Configuration) ?? new();
        }

        return response;
    }

    public async Task<SecurityPolicyListResponse> GetUserSecurityPoliciesAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        var query = _context.SecurityPolicies
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var policies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var policyResponses = policies.Select(p =>
        {
            var response = p.Adapt<SecurityPolicyResponse>();
            if (!string.IsNullOrEmpty(p.Configuration))
            {
                response.Configuration = JsonSerializer.Deserialize<object>(p.Configuration) ?? new();
            }
            return response;
        }).ToList();

        return new SecurityPolicyListResponse
        {
            Policies = policyResponses,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<SecurityPolicyResponse> UpdateSecurityPolicyAsync(Guid policyId, Guid userId, CreateSecurityPolicyRequest request, string correlationId)
    {
        var policy = await _context.SecurityPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.UserId == userId);

        if (policy == null)
        {
            throw new ArgumentException($"Security policy {policyId} not found for user {userId}");
        }

        policy.Name = request.Name;
        policy.Description = request.Description;
        policy.Configuration = JsonSerializer.Serialize(request.Configuration);
        policy.IsRequired = request.IsRequired;
        policy.UpdatedBy = userId.ToString();
        policy.CorrelationId = correlationId;

        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "PolicyUpdated", "Medium", $"Security policy {request.Name} updated", 
            "SecurityService", "UpdatePolicy", "Success", new { PolicyId = policyId, PolicyName = request.Name }, 
            correlationId, "", "");

        return policy.Adapt<SecurityPolicyResponse>();
    }

    public async Task DeleteSecurityPolicyAsync(Guid policyId, Guid userId, string correlationId)
    {
        var policy = await _context.SecurityPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.UserId == userId);

        if (policy == null)
        {
            throw new ArgumentException($"Security policy {policyId} not found for user {userId}");
        }

        if (policy.IsRequired)
        {
            throw new InvalidOperationException("Cannot delete required security policy");
        }

        _context.SecurityPolicies.Remove(policy);
        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "PolicyDeleted", "Medium", $"Security policy {policy.Name} deleted", 
            "SecurityService", "DeletePolicy", "Success", new { PolicyId = policyId, PolicyName = policy.Name }, 
            correlationId, "", "");
    }

    public async Task<bool> ValidateSecurityPolicyAsync(Guid userId, string policyType, object context)
    {
        var policy = await _context.SecurityPolicies
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PolicyType == policyType && p.IsActive);

        if (policy == null)
        {
            return true; // No policy means no restriction
        }

        // Policy validation logic would go here based on policy type and configuration
        // This is a simplified implementation
        return true;
    }

    #endregion

    #region Multi-Signature Management

    public async Task<MultiSignatureResponse> CreateMultiSigRequestAsync(Guid userId, CreateMultiSigRequest request, string correlationId)
    {
        _logger.LogInformation("Creating multi-sig request for user {UserId} and account {AccountId}", userId, request.AccountId);

        try
        {
            // Validate account ownership
            var account = await _accountService.GetAccountAsync(request.AccountId);
            if (account.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not own the specified account");
            }

            var multiSigRequest = new MultiSignatureRequest
            {
                AccountId = request.AccountId,
                RequestedBy = userId,
                OperationType = request.OperationType,
                Description = request.Description,
                Amount = request.Amount,
                Currency = request.Currency,
                DestinationAddress = request.DestinationAddress,
                OperationData = JsonSerializer.Serialize(request.OperationData),
                RequiredSignatures = request.RequiredSignatures,
                ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddHours(24),
                CorrelationId = correlationId
            };

            _context.MultiSignatureRequests.Add(multiSigRequest);
            await _context.SaveChangesAsync();

            await LogSecurityEventAsync(userId, "MultiSigRequestCreated", "Medium", 
                $"Multi-signature request created for {request.OperationType}", 
                "SecurityService", "CreateMultiSigRequest", "Success", 
                new { RequestId = multiSigRequest.Id, OperationType = request.OperationType, Amount = request.Amount }, 
                correlationId, "", "");

            return multiSigRequest.Adapt<MultiSignatureResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multi-sig request for user {UserId}", userId);
            
            await LogSecurityEventAsync(userId, "MultiSigRequestFailed", "High", 
                $"Failed to create multi-signature request: {ex.Message}", 
                "SecurityService", "CreateMultiSigRequest", "Failure", new { Error = ex.Message }, 
                correlationId, "", "");
            
            throw;
        }
    }

    public async Task<MultiSignatureResponse> GetMultiSigRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _context.MultiSignatureRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            throw new ArgumentException($"Multi-signature request {requestId} not found");
        }

        // Validate user has access to this request
        var account = await _accountService.GetAccountAsync(request.AccountId);
        if (account.UserId != userId && request.RequestedBy != userId)
        {
            throw new UnauthorizedAccessException("User does not have access to this multi-signature request");
        }

        var response = request.Adapt<MultiSignatureResponse>();
        response.Approvals = request.Approvals.Adapt<List<MultiSignatureApprovalResponse>>();

        return response;
    }

    public async Task<MultiSignatureListResponse> GetUserMultiSigRequestsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 50)
    {
        var query = _context.MultiSignatureRequests
            .Where(r => r.RequestedBy == userId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync();
        var requests = await query
            .Include(r => r.Approvals)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var requestResponses = requests.Select(r =>
        {
            var response = r.Adapt<MultiSignatureResponse>();
            response.Approvals = r.Approvals.Adapt<List<MultiSignatureApprovalResponse>>();
            return response;
        }).ToList();

        return new MultiSignatureListResponse
        {
            Requests = requestResponses,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<MultiSignatureResponse> ApproveMultiSigRequestAsync(Guid userId, ApproveMultiSigRequest request, string correlationId, string ipAddress, string userAgent)
    {
        var multiSigRequest = await _context.MultiSignatureRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId);

        if (multiSigRequest == null)
        {
            throw new ArgumentException($"Multi-signature request {request.RequestId} not found");
        }

        if (multiSigRequest.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot approve request with status {multiSigRequest.Status}");
        }

        if (multiSigRequest.ExpiresAt.HasValue && multiSigRequest.ExpiresAt < DateTime.UtcNow)
        {
            multiSigRequest.Status = "Expired";
            await _context.SaveChangesAsync();
            throw new InvalidOperationException("Multi-signature request has expired");
        }

        // Check if user already approved/rejected this request
        var existingApproval = multiSigRequest.Approvals.FirstOrDefault(a => a.ApprovedBy == userId);
        if (existingApproval != null)
        {
            throw new InvalidOperationException("User has already provided approval for this request");
        }

        var approval = new MultiSignatureApproval
        {
            RequestId = request.RequestId,
            ApprovedBy = userId,
            Status = request.Status,
            Comments = request.Comments,
            SignatureData = request.SignatureData,
            SignatureMethod = request.SignatureMethod,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _context.MultiSignatureApprovals.Add(approval);

        if (request.Status == "Approved")
        {
            multiSigRequest.CurrentSignatures++;
        }

        // Check if we have enough signatures
        if (multiSigRequest.CurrentSignatures >= multiSigRequest.RequiredSignatures)
        {
            multiSigRequest.Status = "Approved";
        }
        else if (request.Status == "Rejected")
        {
            multiSigRequest.Status = "Rejected";
        }

        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "MultiSigApproval", "Medium", 
            $"Multi-signature request {request.Status.ToLower()}", 
            "SecurityService", "ApproveMultiSigRequest", "Success", 
            new { RequestId = request.RequestId, Status = request.Status, CurrentSignatures = multiSigRequest.CurrentSignatures }, 
            correlationId, ipAddress, userAgent);

        var response = multiSigRequest.Adapt<MultiSignatureResponse>();
        response.Approvals = multiSigRequest.Approvals.Adapt<List<MultiSignatureApprovalResponse>>();

        return response;
    }

    public async Task<MultiSignatureResponse> ExecuteMultiSigRequestAsync(Guid requestId, Guid userId, string correlationId)
    {
        var request = await _context.MultiSignatureRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            throw new ArgumentException($"Multi-signature request {requestId} not found");
        }

        if (request.Status != "Approved")
        {
            throw new InvalidOperationException($"Cannot execute request with status {request.Status}");
        }

        if (request.RequestedBy != userId)
        {
            throw new UnauthorizedAccessException("Only the request creator can execute the request");
        }

        // Execute the operation based on operation type
        // This would integrate with other services to perform the actual operation
        // For now, we'll just mark it as executed

        request.Status = "Executed";
        request.ExecutedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "MultiSigExecuted", "High", 
            $"Multi-signature request executed for {request.OperationType}", 
            "SecurityService", "ExecuteMultiSigRequest", "Success", 
            new { RequestId = requestId, OperationType = request.OperationType, Amount = request.Amount }, 
            correlationId, "", "");

        var response = request.Adapt<MultiSignatureResponse>();
        response.Approvals = request.Approvals.Adapt<List<MultiSignatureApprovalResponse>>();

        return response;
    }

    public async Task<bool> ValidateMultiSigRequirementAsync(Guid accountId, string operationType, decimal? amount = null)
    {
        // Get account details
        var account = await _accountService.GetAccountAsync(accountId);
        
        // Check if multi-sig is required for this operation type and amount
        // This would be based on security policies and account settings
        // For now, we'll implement basic logic

        if (operationType == "Transfer" && amount.HasValue && amount > 10000)
        {
            return true; // Require multi-sig for large transfers
        }

        if (operationType == "Withdrawal" && amount.HasValue && amount > 5000)
        {
            return true; // Require multi-sig for large withdrawals
        }

        return false;
    }

    #endregion

    #region MFA Management

    public async Task<string> GenerateMfaTokenAsync(Guid userId, string tokenType, string purpose, string correlationId, string ipAddress, string userAgent)
    {
        _logger.LogInformation("Generating MFA token for user {UserId}, type {TokenType}, purpose {Purpose}", userId, tokenType, purpose);

        try
        {
            string token;
            DateTime expiresAt;

            switch (tokenType.ToUpper())
            {
                case "TOTP":
                    token = GenerateTotpToken();
                    expiresAt = DateTime.UtcNow.AddMinutes(5);
                    break;
                case "SMS":
                case "EMAIL":
                    token = GenerateNumericToken(6);
                    expiresAt = DateTime.UtcNow.AddMinutes(10);
                    break;
                case "BACKUPCODE":
                    token = GenerateBackupCode();
                    expiresAt = DateTime.UtcNow.AddYears(1);
                    break;
                default:
                    throw new ArgumentException($"Unsupported token type: {tokenType}");
            }

            var tokenHash = HashToken(token);

            var mfaToken = new MfaToken
            {
                UserId = userId,
                TokenType = tokenType,
                TokenHash = tokenHash,
                Purpose = purpose,
                ExpiresAt = expiresAt,
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.MfaTokens.Add(mfaToken);
            await _context.SaveChangesAsync();

            await LogSecurityEventAsync(userId, "MfaTokenGenerated", "Low", 
                $"MFA token generated for {purpose}", 
                "SecurityService", "GenerateMfaToken", "Success", 
                new { TokenType = tokenType, Purpose = purpose }, 
                correlationId, ipAddress, userAgent);

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating MFA token for user {UserId}", userId);
            
            await LogSecurityEventAsync(userId, "MfaTokenGenerationFailed", "Medium", 
                $"Failed to generate MFA token: {ex.Message}", 
                "SecurityService", "GenerateMfaToken", "Failure", new { Error = ex.Message }, 
                correlationId, ipAddress, userAgent);
            
            throw;
        }
    }

    public async Task<bool> ValidateMfaTokenAsync(Guid userId, ValidateMfaRequest request, string correlationId, string ipAddress, string userAgent)
    {
        _logger.LogInformation("Validating MFA token for user {UserId}, type {TokenType}", userId, request.TokenType);

        try
        {
            var tokenHash = HashToken(request.Token);

            var mfaToken = await _context.MfaTokens
                .FirstOrDefaultAsync(t => t.UserId == userId 
                    && t.TokenType == request.TokenType 
                    && t.TokenHash == tokenHash 
                    && t.Purpose == request.Purpose
                    && !t.IsUsed 
                    && !t.IsBlocked 
                    && t.ExpiresAt > DateTime.UtcNow);

            if (mfaToken == null)
            {
                // Check for existing token to increment attempt count
                var existingToken = await _context.MfaTokens
                    .FirstOrDefaultAsync(t => t.UserId == userId 
                        && t.TokenType == request.TokenType 
                        && t.Purpose == request.Purpose
                        && !t.IsUsed 
                        && t.ExpiresAt > DateTime.UtcNow);

                if (existingToken != null)
                {
                    existingToken.AttemptCount++;
                    if (existingToken.AttemptCount >= existingToken.MaxAttempts)
                    {
                        existingToken.IsBlocked = true;
                        existingToken.BlockedAt = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                }

                await LogSecurityEventAsync(userId, "MfaValidationFailed", "Medium", 
                    "Invalid MFA token provided", 
                    "SecurityService", "ValidateMfaToken", "Failure", 
                    new { TokenType = request.TokenType, Purpose = request.Purpose }, 
                    correlationId, ipAddress, userAgent);

                return false;
            }

            // Mark token as used
            mfaToken.IsUsed = true;
            mfaToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await LogSecurityEventAsync(userId, "MfaValidationSuccess", "Low", 
                "MFA token validated successfully", 
                "SecurityService", "ValidateMfaToken", "Success", 
                new { TokenType = request.TokenType, Purpose = request.Purpose }, 
                correlationId, ipAddress, userAgent);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA token for user {UserId}", userId);
            
            await LogSecurityEventAsync(userId, "MfaValidationError", "High", 
                $"Error validating MFA token: {ex.Message}", 
                "SecurityService", "ValidateMfaToken", "Failure", new { Error = ex.Message }, 
                correlationId, ipAddress, userAgent);
            
            return false;
        }
    }

    public async Task<List<string>> GenerateBackupCodesAsync(Guid userId, string correlationId)
    {
        var backupCodes = new List<string>();

        for (int i = 0; i < 10; i++)
        {
            var code = GenerateBackupCode();
            backupCodes.Add(code);

            var tokenHash = HashToken(code);

            var mfaToken = new MfaToken
            {
                UserId = userId,
                TokenType = "BackupCode",
                TokenHash = tokenHash,
                Purpose = "BackupAccess",
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                CorrelationId = correlationId
            };

            _context.MfaTokens.Add(mfaToken);
        }

        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "BackupCodesGenerated", "Medium", 
            "Backup codes generated", 
            "SecurityService", "GenerateBackupCodes", "Success", 
            new { CodeCount = backupCodes.Count }, 
            correlationId, "", "");

        return backupCodes;
    }

    public async Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode, string correlationId, string ipAddress, string userAgent)
    {
        var tokenHash = HashToken(backupCode);

        var mfaToken = await _context.MfaTokens
            .FirstOrDefaultAsync(t => t.UserId == userId 
                && t.TokenType == "BackupCode" 
                && t.TokenHash == tokenHash 
                && !t.IsUsed 
                && t.ExpiresAt > DateTime.UtcNow);

        if (mfaToken == null)
        {
            await LogSecurityEventAsync(userId, "BackupCodeValidationFailed", "Medium", 
                "Invalid backup code provided", 
                "SecurityService", "ValidateBackupCode", "Failure", 
                new { }, correlationId, ipAddress, userAgent);

            return false;
        }

        // Mark backup code as used
        mfaToken.IsUsed = true;
        mfaToken.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "BackupCodeValidationSuccess", "Medium", 
            "Backup code validated successfully", 
            "SecurityService", "ValidateBackupCode", "Success", 
            new { }, correlationId, ipAddress, userAgent);

        return true;
    }

    public async Task InvalidateUserMfaTokensAsync(Guid userId, string tokenType, string correlationId)
    {
        var tokens = await _context.MfaTokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType && !t.IsUsed)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await LogSecurityEventAsync(userId, "MfaTokensInvalidated", "Low", 
            $"MFA tokens of type {tokenType} invalidated", 
            "SecurityService", "InvalidateMfaTokens", "Success", 
            new { TokenType = tokenType, TokenCount = tokens.Count }, 
            correlationId, "", "");
    }

    #endregion

    #region Security Event Management

    public async Task LogSecurityEventAsync(Guid userId, string eventType, string severity, string description, string source, string action, string result, object eventData, string correlationId, string ipAddress, string userAgent)
    {
        try
        {
            var securityEvent = new SecurityEvent
            {
                UserId = userId,
                EventType = eventType,
                Severity = severity,
                Description = description,
                Source = source,
                Action = action,
                Result = result,
                EventData = JsonSerializer.Serialize(eventData),
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequiresInvestigation = severity == "Critical" || severity == "High"
            };

            _context.SecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Security event logged: {EventType} for user {UserId} with severity {Severity}", eventType, userId, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event for user {UserId}", userId);
            // Don't throw here to avoid breaking the main operation
        }
    }

    public async Task<List<SecurityEvent>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, string? severity = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        var query = _context.SecurityEvents
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            query = query.Where(e => e.Severity == severity);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SecurityEvent>> GetCriticalSecurityEventsAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        var query = _context.SecurityEvents
            .Where(e => e.Severity == "Critical" && e.RequiresInvestigation && !e.IsResolved);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task ResolveSecurityEventAsync(Guid eventId, Guid resolvedBy, string resolutionNotes, string correlationId)
    {
        var securityEvent = await _context.SecurityEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (securityEvent == null)
        {
            throw new ArgumentException($"Security event {eventId} not found");
        }

        securityEvent.IsResolved = true;
        securityEvent.ResolvedAt = DateTime.UtcNow;
        securityEvent.ResolvedBy = resolvedBy.ToString();
        securityEvent.ResolutionNotes = resolutionNotes;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Security event {EventId} resolved by {ResolvedBy}", eventId, resolvedBy);
    }

    #endregion

    #region Security Validation

    public async Task<bool> ValidateUserAccessAsync(Guid userId, string resource, string action)
    {
        // This would integrate with the UserService to check roles and permissions
        // For now, we'll implement basic validation
        try
        {
            var user = await _userService.GetUserAsync(userId);
            return user != null && user.IsActive;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsUserSecurityComplianceValidAsync(Guid userId)
    {
        try
        {
            // Check if user has required security policies
            var requiredPolicies = await _context.SecurityPolicies
                .Where(p => p.UserId == userId && p.IsRequired && p.IsActive)
                .CountAsync();

            // Check for recent security violations
            var recentViolations = await _context.SecurityEvents
                .Where(e => e.UserId == userId 
                    && e.Severity == "Critical" 
                    && e.Timestamp > DateTime.UtcNow.AddDays(-30)
                    && !e.IsResolved)
                .CountAsync();

            return recentViolations == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetUserSecurityStatusAsync(Guid userId)
    {
        try
        {
            var activePolicies = await _context.SecurityPolicies
                .Where(p => p.UserId == userId && p.IsActive)
                .CountAsync();

            var pendingMultiSigRequests = await _context.MultiSignatureRequests
                .Where(r => r.RequestedBy == userId && r.Status == "Pending")
                .CountAsync();

            var recentSecurityEvents = await _context.SecurityEvents
                .Where(e => e.UserId == userId && e.Timestamp > DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            var criticalEvents = await _context.SecurityEvents
                .Where(e => e.UserId == userId && e.Severity == "Critical" && !e.IsResolved)
                .CountAsync();

            return new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["activePolicies"] = activePolicies,
                ["pendingMultiSigRequests"] = pendingMultiSigRequests,
                ["recentSecurityEvents"] = recentSecurityEvents,
                ["criticalEvents"] = criticalEvents,
                ["isCompliant"] = criticalEvents == 0,
                ["lastUpdated"] = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security status for user {UserId}", userId);
            return new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["error"] = ex.Message,
                ["lastUpdated"] = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Helper Methods

    private string GenerateTotpToken()
    {
        // Generate a 6-digit TOTP token
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string GenerateNumericToken(int length)
    {
        var random = new Random();
        var token = "";
        for (int i = 0; i < length; i++)
        {
            token += random.Next(0, 10).ToString();
        }
        return token;
    }

    private string GenerateBackupCode()
    {
        // Generate a backup code in format: XXXX-XXXX
        var random = new Random();
        var part1 = random.Next(1000, 9999).ToString();
        var part2 = random.Next(1000, 9999).ToString();
        return $"{part1}-{part2}";
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }

    #endregion
}
