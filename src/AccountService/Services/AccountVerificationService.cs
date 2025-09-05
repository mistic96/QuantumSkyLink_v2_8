using AccountService.Data;
using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace AccountService.Services;

public class AccountVerificationService : IAccountVerificationService
{
    private readonly AccountDbContext _context;
    private readonly ILogger<AccountVerificationService> _logger;

    public AccountVerificationService(
        AccountDbContext context,
        ILogger<AccountVerificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountVerificationResponse> CreateVerificationAsync(CreateAccountVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var verification = new AccountVerification
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            VerificationType = Enum.Parse<VerificationType>(request.VerificationType),
            Status = VerificationStatus.Pending,
            ExternalVerificationId = request.ExternalVerificationId,
            DocumentType = request.DocumentType,
            DocumentPath = request.DocumentPath,
            Notes = request.Notes,
            CorrelationId = request.CorrelationId,
            Metadata = request.Metadata,
            SubmittedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AccountVerifications.Add(verification);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification created: {VerificationId} for account {AccountId}", verification.Id, request.AccountId);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<AccountVerificationResponse> GetVerificationAsync(Guid verificationId, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .Include(v => v.Account)
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<IEnumerable<AccountVerificationResponse>> GetAccountVerificationsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var verifications = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId)
            .OrderByDescending(v => v.SubmittedAt)
            .ToListAsync(cancellationToken);

        return verifications.Adapt<IEnumerable<AccountVerificationResponse>>();
    }

    public async Task<IEnumerable<AccountVerificationResponse>> GetUserVerificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var verifications = await _context.AccountVerifications
            .Include(v => v.Account)
            .Where(v => v.Account.UserId == userId)
            .OrderByDescending(v => v.SubmittedAt)
            .ToListAsync(cancellationToken);

        return verifications.Adapt<IEnumerable<AccountVerificationResponse>>();
    }

    public async Task<AccountVerificationResponse?> GetLatestVerificationAsync(Guid accountId, VerificationType verificationType, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId && v.VerificationType == verificationType)
            .OrderByDescending(v => v.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return verification?.Adapt<AccountVerificationResponse>();
    }

    public async Task<AccountVerificationResponse> UpdateVerificationStatusAsync(Guid verificationId, VerificationStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        verification.Status = status;
        verification.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            verification.Notes = notes;
        }

        if (status == VerificationStatus.Approved || status == VerificationStatus.Rejected)
        {
            verification.ReviewedAt = DateTime.UtcNow;
        }

        if (status == VerificationStatus.Approved)
        {
            verification.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification status updated: {VerificationId} -> {Status}", verificationId, status);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<AccountVerificationResponse> ApproveVerificationAsync(Guid verificationId, string approvedBy, string? notes = null, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        verification.Status = VerificationStatus.Approved;
        verification.ReviewedBy = approvedBy;
        verification.ReviewedAt = DateTime.UtcNow;
        verification.CompletedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            verification.Notes = notes;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification approved: {VerificationId} by {ApprovedBy}", verificationId, approvedBy);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<AccountVerificationResponse> RejectVerificationAsync(Guid verificationId, string rejectedBy, string reason, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        verification.Status = VerificationStatus.Rejected;
        verification.ReviewedBy = rejectedBy;
        verification.RejectionReason = reason;
        verification.ReviewedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification rejected: {VerificationId} by {RejectedBy}, Reason: {Reason}", verificationId, rejectedBy, reason);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<AccountVerificationResponse> UploadDocumentAsync(Guid verificationId, string documentUrl, string documentType, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        verification.DocumentPath = documentUrl;
        verification.DocumentType = documentType;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document uploaded for verification: {VerificationId}", verificationId);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<bool> DeleteDocumentAsync(Guid verificationId, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            return false;
        }

        verification.DocumentPath = null;
        verification.DocumentType = null;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document deleted for verification: {VerificationId}", verificationId);

        return true;
    }

    public async Task<bool> VerificationExistsAsync(Guid verificationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountVerifications
            .AnyAsync(v => v.Id == verificationId, cancellationToken);
    }

    public async Task<bool> UserOwnsVerificationAsync(Guid userId, Guid verificationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountVerifications
            .Include(v => v.Account)
            .AnyAsync(v => v.Id == verificationId && v.Account.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsAccountVerifiedAsync(Guid accountId, VerificationType verificationType, CancellationToken cancellationToken = default)
    {
        return await _context.AccountVerifications
            .AnyAsync(v => v.AccountId == accountId && v.VerificationType == verificationType && v.Status == VerificationStatus.Approved, cancellationToken);
    }

    public async Task<bool> IsAccountFullyVerifiedAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        // Get required verification types based on account type
        var requiredTypes = GetRequiredVerificationTypes(account.AccountType);

        foreach (var type in requiredTypes)
        {
            var isVerified = await IsAccountVerifiedAsync(accountId, type, cancellationToken);
            if (!isVerified)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<IEnumerable<VerificationRequirementResponse>> GetVerificationRequirementsAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        var requirements = new List<VerificationRequirementResponse>();
        var requiredTypes = GetRequiredVerificationTypes(accountType);

        foreach (var type in requiredTypes)
        {
            requirements.Add(new VerificationRequirementResponse
            {
                VerificationType = type.ToString(),
                IsRequired = true,
                Description = GetVerificationTypeDescription(type)
            });
        }

        return requirements;
    }

    public async Task<VerificationStatusSummaryResponse> GetAccountVerificationStatusAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID {accountId} not found");
        }

        var verifications = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var requiredTypes = GetRequiredVerificationTypes(account.AccountType);
        var verificationStatuses = new List<VerificationTypeStatusResponse>();

        foreach (var type in requiredTypes)
        {
            var latestVerification = verifications
                .Where(v => v.VerificationType == type)
                .OrderByDescending(v => v.SubmittedAt)
                .FirstOrDefault();

            verificationStatuses.Add(new VerificationTypeStatusResponse
            {
                VerificationType = type.ToString(),
                Status = (latestVerification?.Status ?? VerificationStatus.Pending).ToString(),
                IsRequired = true,
                LastUpdated = latestVerification?.UpdatedAt ?? DateTime.UtcNow
            });
        }

        return new VerificationStatusSummaryResponse
        {
            AccountId = accountId,
            IsFullyVerified = await IsAccountFullyVerifiedAsync(accountId, cancellationToken),
            VerificationStatuses = verificationStatuses.Select(vs => vs.VerificationType).ToList()
        };
    }

    public async Task<IEnumerable<AccountVerificationResponse>> GetPendingVerificationsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var verifications = await _context.AccountVerifications
            .Where(v => v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.InReview)
            .OrderBy(v => v.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return verifications.Adapt<IEnumerable<AccountVerificationResponse>>();
    }

    public async Task<AccountVerificationResponse> InitiateKycVerificationAsync(Guid accountId, KycVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var verificationRequest = new CreateAccountVerificationRequest
        {
            AccountId = accountId,
            VerificationType = "KYC",
            DocumentType = "KYC Package",
            Notes = $"KYC verification initiated for {request.FirstName} {request.LastName}",
            Metadata = System.Text.Json.JsonSerializer.Serialize(request),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        return await CreateVerificationAsync(verificationRequest, cancellationToken);
    }

    public async Task<AccountVerificationResponse> ProcessKycResultAsync(Guid verificationId, KycResultRequest request, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        verification.Status = request.IsApproved ? VerificationStatus.Approved : VerificationStatus.Rejected;
        verification.ReviewedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;
        verification.ExternalVerificationId = request.ExternalVerificationId;

        if (!request.IsApproved)
        {
            verification.RejectionReason = request.RejectionReason;
        }
        else
        {
            verification.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("KYC verification processed: {VerificationId} -> {Status}", verificationId, verification.Status);

        return verification.Adapt<AccountVerificationResponse>();
    }

    public async Task<KycStatusResponse> GetKycStatusAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var kycVerification = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId && v.VerificationType == VerificationType.KYC)
            .OrderByDescending(v => v.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new KycStatusResponse
        {
            AccountId = accountId,
            Status = (kycVerification?.Status ?? VerificationStatus.Pending).ToString(),
            SubmittedAt = kycVerification?.SubmittedAt,
            ReviewedAt = kycVerification?.ReviewedAt,
            CompletedAt = kycVerification?.CompletedAt,
            RejectionReason = kycVerification?.RejectionReason
        };
    }

    public async Task<ComplianceCheckResponse> PerformComplianceCheckAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID {accountId} not found");
        }

        var verifications = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var issues = new List<string>();
        var requiredTypes = GetRequiredVerificationTypes(account.AccountType);

        foreach (var type in requiredTypes)
        {
            var isVerified = verifications.Any(v => v.VerificationType == type && v.Status == VerificationStatus.Approved);
            if (!isVerified)
            {
                issues.Add($"Missing {type.ToString()} verification");
            }
        }

        return new ComplianceCheckResponse
        {
            AccountId = accountId,
            IsCompliant = issues.Count == 0,
            Issues = issues,
            CheckedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> IsAccountCompliantAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var complianceCheck = await PerformComplianceCheckAsync(accountId, cancellationToken);
        return complianceCheck.IsCompliant;
    }

    public async Task<IEnumerable<ComplianceIssueResponse>> GetComplianceIssuesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var complianceCheck = await PerformComplianceCheckAsync(accountId, cancellationToken);
        
        return complianceCheck.Issues.Select(issue => new ComplianceIssueResponse
        {
            AccountId = accountId,
            Issue = issue,
            Severity = "High",
            IdentifiedAt = DateTime.UtcNow
        });
    }

    // Additional KYC Workflow Methods
    public async Task<KycWorkflowResponse> InitiateKycWorkflowAsync(Guid accountId, string kycLevel, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID {accountId} not found");
        }

        var workflowId = Guid.NewGuid();
        
        // Create KYC verification record
        var verification = new AccountVerification
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            VerificationType = VerificationType.KYC,
            Status = VerificationStatus.InReview,
            DocumentType = "KYC Workflow",
            Notes = $"KYC workflow initiated with level: {kycLevel}",
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { WorkflowId = workflowId, KycLevel = kycLevel }),
            SubmittedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AccountVerifications.Add(verification);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("KYC workflow initiated: {WorkflowId} for account {AccountId} with level {KycLevel}", workflowId, accountId, kycLevel);

        return new KycWorkflowResponse
        {
            Id = workflowId,
            AccountId = accountId,
            WorkflowType = $"KYC Level: {kycLevel}",
            Status = VerificationStatus.InReview.ToString(),
            CurrentStep = "Initiated",
            TotalSteps = 5,
            CompletedSteps = 1,
            ProgressPercentage = 20,
            Description = $"KYC workflow initiated with level: {kycLevel}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            RequiresManualReview = false
        };
    }

    public async Task<KycWorkflowResponse> GetKycWorkflowStatusAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var kycVerification = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId && v.VerificationType == VerificationType.KYC)
            .OrderByDescending(v => v.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (kycVerification == null)
        {
            throw new InvalidOperationException($"No KYC workflow found for account {accountId}");
        }

        var metadata = string.IsNullOrEmpty(kycVerification.Metadata) 
            ? new { WorkflowId = Guid.NewGuid(), KycLevel = "Standard" }
            : System.Text.Json.JsonSerializer.Deserialize<dynamic>(kycVerification.Metadata);

        return new KycWorkflowResponse
        {
            Id = kycVerification.Id,
            AccountId = accountId,
            WorkflowType = "KYC Standard",
            Status = kycVerification.Status.ToString(),
            CurrentStep = kycVerification.Status == VerificationStatus.Approved ? "Completed" : "In Progress",
            TotalSteps = 5,
            CompletedSteps = kycVerification.Status == VerificationStatus.Approved ? 5 : 3,
            ProgressPercentage = kycVerification.Status == VerificationStatus.Approved ? 100 : 60,
            Description = "KYC workflow status",
            CreatedAt = kycVerification.CreatedAt,
            UpdatedAt = kycVerification.UpdatedAt,
            CompletedAt = kycVerification.CompletedAt,
            ExpiryDate = kycVerification.ExpiresAt,
            RequiresManualReview = kycVerification.Status == VerificationStatus.InReview
        };
    }

    public async Task<KycProcessingResultResponse> ProcessKycResultsAsync(Guid accountId, Dictionary<string, object> kycResults, Dictionary<string, object>? complianceData, CancellationToken cancellationToken = default)
    {
        var kycVerification = await _context.AccountVerifications
            .Where(v => v.AccountId == accountId && v.VerificationType == VerificationType.KYC)
            .OrderByDescending(v => v.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (kycVerification == null)
        {
            throw new InvalidOperationException($"No KYC verification found for account {accountId}");
        }

        // Process KYC results
        var isApproved = kycResults.ContainsKey("approved") && Convert.ToBoolean(kycResults["approved"]);
        var riskScore = kycResults.ContainsKey("riskScore") ? Convert.ToDecimal(kycResults["riskScore"]) : 0;

        kycVerification.Status = isApproved ? VerificationStatus.Approved : VerificationStatus.Rejected;
        kycVerification.ReviewedAt = DateTime.UtcNow;
        kycVerification.UpdatedAt = DateTime.UtcNow;
        kycVerification.Metadata = System.Text.Json.JsonSerializer.Serialize(new { KycResults = kycResults, ComplianceData = complianceData });

        if (isApproved)
        {
            kycVerification.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            kycVerification.RejectionReason = kycResults.ContainsKey("rejectionReason") 
                ? kycResults["rejectionReason"].ToString() 
                : "KYC verification failed";
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("KYC results processed for account {AccountId}: {Status}", accountId, kycVerification.Status);

        return new KycProcessingResultResponse
        {
            Id = Guid.NewGuid(),
            WorkflowId = kycVerification.Id,
            AccountId = accountId,
            Status = isApproved ? "Approved" : "Rejected",
            RiskLevel = riskScore > 70 ? "High" : riskScore > 30 ? "Medium" : "Low",
            RiskScore = riskScore,
            Results = System.Text.Json.JsonSerializer.Serialize(kycResults),
            ProcessedAt = DateTime.UtcNow,
            IsSuccessful = isApproved,
            CompletionPercentage = 100
        };
    }

    // Additional Status and Compliance Methods
    public async Task<ComplianceStatusResponse> GetComplianceStatusAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var complianceCheck = await PerformComplianceCheckAsync(accountId, cancellationToken);
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID {accountId} not found");
        }

        return new ComplianceStatusResponse
        {
            AccountId = accountId,
            OverallStatus = complianceCheck.IsCompliant ? "Compliant" : "NonCompliant",
            RiskLevel = complianceCheck.IsCompliant ? "Low" : "High",
            ComplianceScore = complianceCheck.IsCompliant ? 100 : 50,
            LastAssessmentDate = complianceCheck.CheckedAt,
            NextAssessmentDue = DateTime.UtcNow.AddMonths(6),
            TotalChecks = complianceCheck.Issues.Count + 5,
            PassedChecks = complianceCheck.IsCompliant ? complianceCheck.Issues.Count + 5 : 5,
            FailedChecks = complianceCheck.IsCompliant ? 0 : complianceCheck.Issues.Count,
            PendingChecks = 0,
            CompletedRequirements = complianceCheck.IsCompliant ? new List<string> { "Identity", "Address", "KYC" } : new List<string>(),
            PendingRequirements = complianceCheck.IsCompliant ? new List<string>() : complianceCheck.Issues.ToList(),
            FailedRequirements = new List<string>(),
            OpenIssues = complianceCheck.Issues.Count,
            CriticalIssues = complianceCheck.IsCompliant ? 0 : complianceCheck.Issues.Count,
            Summary = complianceCheck.IsCompliant ? "Account is fully compliant" : "Account has compliance issues",
            RequiresImmediateAttention = !complianceCheck.IsCompliant
        };
    }

    public async Task<VerificationStatusSummaryResponse> GetVerificationStatusSummaryAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetAccountVerificationStatusAsync(accountId, cancellationToken);
    }

    public async Task<VerificationRequirementsResponse> GetVerificationRequirementsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID {accountId} not found");
        }

        var requirements = await GetVerificationRequirementsAsync(account.AccountType, cancellationToken);

        return new VerificationRequirementsResponse
        {
            AccountId = accountId,
            AccountType = account.AccountType.ToString(),
            VerificationLevel = "Standard",
            RequiredDocuments = requirements.Select(r => r.VerificationType).ToList(),
            OptionalDocuments = new List<string>(),
            RequiredChecks = requirements.Select(r => r.Description).ToList(),
            CompletedRequirements = new List<string>(),
            PendingRequirements = requirements.Select(r => r.VerificationType).ToList(),
            FailedRequirements = new List<string>(),
            CompletionPercentage = 0,
            Instructions = "Please complete all required verification steps",
            NextSteps = "Upload required documents and complete verification process",
            GeneratedAt = DateTime.UtcNow,
            RequirementDetails = requirements.ToDictionary(r => r.VerificationType, r => r.Description)
        };
    }

    // Document Management with Extended Signatures
    public async Task<DocumentUploadResponse> UploadDocumentAsync(Guid verificationId, string documentType, byte[] documentData, string fileName, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            throw new InvalidOperationException($"Verification with ID {verificationId} not found");
        }

        // In a real implementation, you would save the document to storage
        var documentId = Guid.NewGuid();
        var documentPath = $"/documents/{verificationId}/{documentId}_{fileName}";

        verification.DocumentPath = documentPath;
        verification.DocumentType = documentType;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document uploaded for verification: {VerificationId}, Document: {FileName}", verificationId, fileName);

        return new DocumentUploadResponse
        {
            Id = documentId,
            AccountId = verification.AccountId,
            DocumentType = documentType,
            FileName = fileName,
            FileType = Path.GetExtension(fileName).TrimStart('.'),
            FileSize = documentData.Length,
            Description = $"Document uploaded for verification {verificationId}",
            UploadedAt = DateTime.UtcNow,
            Status = "Uploaded",
            IsConfidential = true,
            VerificationPurpose = "Account Verification",
            StorageLocation = documentPath
        };
    }

    public async Task<bool> DeleteDocumentAsync(Guid verificationId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var verification = await _context.AccountVerifications
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null)
        {
            return false;
        }

        // In a real implementation, you would delete the document from storage
        verification.DocumentPath = null;
        verification.DocumentType = null;
        verification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document deleted for verification: {VerificationId}, Document: {DocumentId}", verificationId, documentId);

        return true;
    }

    // Compliance Check with Extended Signature
    public async Task<ComplianceCheckResponse> PerformComplianceCheckAsync(Guid accountId, string checkType, Dictionary<string, object>? additionalData, CancellationToken cancellationToken = default)
    {
        var baseCheck = await PerformComplianceCheckAsync(accountId, cancellationToken);
        
        // Add additional checks based on checkType
        var additionalIssues = new List<string>();
        
        if (checkType == "Enhanced" && additionalData != null)
        {
            if (additionalData.ContainsKey("riskScore") && Convert.ToDecimal(additionalData["riskScore"]) > 70)
            {
                additionalIssues.Add("High risk score detected");
            }
        }

        return new ComplianceCheckResponse
        {
            AccountId = accountId,
            IsCompliant = baseCheck.IsCompliant && additionalIssues.Count == 0,
            Issues = baseCheck.Issues.Concat(additionalIssues).ToList(),
            CheckedAt = DateTime.UtcNow
        };
    }

    private static List<VerificationType> GetRequiredVerificationTypes(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Individual => new List<VerificationType> { VerificationType.Identity, VerificationType.Address, VerificationType.KYC },
            AccountType.Business => new List<VerificationType> { VerificationType.Business, VerificationType.KYC, VerificationType.AML },
            AccountType.Institutional => new List<VerificationType> { VerificationType.Business, VerificationType.Enhanced, VerificationType.AML },
            AccountType.Trading => new List<VerificationType> { VerificationType.Identity, VerificationType.Income, VerificationType.KYC },
            _ => new List<VerificationType> { VerificationType.Identity, VerificationType.KYC }
        };
    }

    private static string GetVerificationTypeDescription(VerificationType type)
    {
        return type switch
        {
            VerificationType.Identity => "Government-issued ID verification",
            VerificationType.Address => "Proof of address verification",
            VerificationType.Income => "Income verification documentation",
            VerificationType.Business => "Business registration and documentation",
            VerificationType.BankAccount => "Bank account verification",
            VerificationType.CreditCheck => "Credit history verification",
            VerificationType.KYC => "Know Your Customer verification",
            VerificationType.AML => "Anti-Money Laundering checks",
            VerificationType.Enhanced => "Enhanced due diligence verification",
            _ => "Standard verification process"
        };
    }
}
