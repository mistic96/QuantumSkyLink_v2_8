using Microsoft.EntityFrameworkCore;
using Mapster;
using TokenService.Data;
using TokenService.Data.Entities;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Services;

public class TokenServiceImplementation : ITokenService
{
    private readonly TokenDbContext _context;
    private readonly IQuantumLedgerClient _quantumLedgerClient;
    private readonly IQuantumLedgerSignatureService _signatureService;
    private readonly IAiComplianceService _aiComplianceService;
    private readonly IAssetVerificationService _assetVerificationService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TokenServiceImplementation> _logger;
    private readonly IConfiguration _configuration;

    public TokenServiceImplementation(
        TokenDbContext context,
        IQuantumLedgerClient quantumLedgerClient,
        IQuantumLedgerSignatureService signatureService,
        IAiComplianceService aiComplianceService,
        IAssetVerificationService assetVerificationService,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        ILogger<TokenServiceImplementation> logger,
        IConfiguration configuration)
    {
        _context = context;
        _quantumLedgerClient = quantumLedgerClient;
        _signatureService = signatureService;
        _aiComplianceService = aiComplianceService;
        _assetVerificationService = assetVerificationService;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
        _configuration = configuration;
    }

    #region Token Submission and Creation

    public async Task<TokenSubmissionResponse> SubmitTokenAsync(TokenSubmissionRequest request)
    {
        _logger.LogInformation("Processing token submission for creator {CreatorId}", request.CreatorId);

        try
        {
            // Validate user exists
            var userExists = await _userServiceClient.UserExistsAsync(request.CreatorId);
            if (!userExists)
            {
                throw new ArgumentException($"User {request.CreatorId} does not exist");
            }

            // Calculate AI compliance score
            var complianceScore = await _aiComplianceService.CalculateComplianceScoreAsync(request);
            var recommendations = await _aiComplianceService.GetRecommendationsAsync(request);
            var redFlags = await _aiComplianceService.DetectRedFlagsAsync(request);

            // Create token submission entity
            var submission = new TokenSubmission
            {
                CreatorId = request.CreatorId,
                TokenPurpose = request.TokenPurpose,
                UseCase = request.UseCase,
                ConfigurationJson = System.Text.Json.JsonSerializer.Serialize(request.Configuration),
                AiComplianceScore = complianceScore.OverallScore,
                AiRecommendations = System.Text.Json.JsonSerializer.Serialize(recommendations),
                AiRedFlags = System.Text.Json.JsonSerializer.Serialize(redFlags),
                AssetType = request.AssetDetails?.AssetType,
                AssetVerificationId = request.AssetDetails?.AssetId
            };

            // Auto-approve if score is high enough
            var autoApprovalThreshold = _configuration.GetValue<decimal>("TokenService:AutoApprovalThreshold", 90.0m);
            if (complianceScore.OverallScore >= autoApprovalThreshold)
            {
                submission.ApprovalStatus = "Approved";
                submission.ReviewedAt = DateTime.UtcNow;
                submission.ReviewedBy = "AI_AUTO_APPROVAL";
                submission.ReviewComments = "Automatically approved based on high compliance score";
            }

            // Initiate asset verification if required
            if (request.AssetDetails != null)
            {
                try
                {
                    var verificationRequest = request.AssetDetails.Adapt<AssetVerificationRequest>();
                    var verificationId = await _assetVerificationService.InitiateVerificationAsync(verificationRequest);
                    submission.AssetVerificationId = verificationId;
                    submission.AssetVerificationStatus = "Pending";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Asset verification initiation failed for submission");
                    submission.AssetVerificationStatus = "Failed";
                }
            }

            _context.TokenSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
                {
                    UserId = request.CreatorId,
                    Type = "TokenSubmission",
                    Title = "Token Submission Received",
                    Message = $"Your token submission '{request.Configuration.Name}' has been received and is under review.",
                    Data = new Dictionary<string, object>
                    {
                        ["submissionId"] = submission.Id,
                        ["complianceScore"] = complianceScore.OverallScore,
                        ["status"] = submission.ApprovalStatus
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for token submission {SubmissionId}", submission.Id);
            }

            _logger.LogInformation("Token submission {SubmissionId} created with compliance score {Score}", 
                submission.Id, complianceScore.OverallScore);

            return new TokenSubmissionResponse
            {
                SubmissionId = submission.Id,
                AiComplianceScore = complianceScore.OverallScore,
                Status = submission.ApprovalStatus,
                Recommendations = recommendations,
                RedFlags = redFlags,
                SubmissionDate = submission.SubmissionDate,
                AssetVerificationStatus = submission.AssetVerificationStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing token submission for creator {CreatorId}", request.CreatorId);
            throw;
        }
    }

    public async Task<TokenCreationResponse> CreateTokenAsync(Guid submissionId)
    {
        _logger.LogInformation("Creating token for submission {SubmissionId}", submissionId);

        try
        {
            var submission = await _context.TokenSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new ArgumentException($"Token submission {submissionId} not found");
            }

            if (submission.ApprovalStatus != "Approved")
            {
                throw new InvalidOperationException($"Token submission {submissionId} is not approved");
            }

            if (submission.TokenId.HasValue)
            {
                throw new InvalidOperationException($"Token already created for submission {submissionId}");
            }

            // Deserialize configuration
            var config = System.Text.Json.JsonSerializer.Deserialize<TokenConfiguration>(submission.ConfigurationJson);
            if (config == null)
            {
                throw new InvalidOperationException("Invalid token configuration");
            }

            // Create QuantumLedger account for the token
            var qlAccountRequest = new CreateQuantumLedgerAccountRequest
            {
                ExternalOwnerId = $"token_{Guid.NewGuid()}",
                OwnerIdType = "TokenId",
                VendorSystem = "TokenService",
                OwnerType = "System",
                Algorithms = new List<string> { "Dilithium", "Falcon", "EC256" },
                GenerateSubstitutionKey = true
            };

            var qlAccountResponse = await _quantumLedgerClient.CreateAccountAsync(qlAccountRequest);
            if (!qlAccountResponse.Success || qlAccountResponse.Account == null)
            {
                throw new InvalidOperationException("Failed to create QuantumLedger account");
            }

            // Create token entity
            var token = new Token
            {
                Name = config.Name,
                Symbol = config.Symbol,
                TotalSupply = config.TotalSupply,
                Decimals = config.Decimals,
                TokenType = config.TokenType,
                CreatorId = submission.CreatorId,
                QuantumLedgerAccountId = qlAccountResponse.Account.AccountId,
                QuantumLedgerExternalOwnerId = qlAccountResponse.Account.ExternalOwnerId,
                QuantumLedgerSubstitutionKeyId = qlAccountResponse.SubstitutionKey?.SubstitutionKeyId,
                Status = "Active",
                ApprovalStatus = "Approved",
                AssetType = config.AssetType,
                AssetMetadata = config.AssetMetadata != null ? 
                    System.Text.Json.JsonSerializer.Serialize(config.AssetMetadata) : null,
                CrossChainEnabled = config.CrossChainEnabled,
                Network = config.Network,
                Description = config.Description,
                ApprovedAt = DateTime.UtcNow,
                ApprovedBy = submission.ReviewedBy
            };

            // Store substitution key securely
            if (qlAccountResponse.SubstitutionKey != null)
            {
                await _signatureService.StoreSubstitutionKeyAsync(
                    token.Id,
                    qlAccountResponse.SubstitutionKey.SubstitutionKeyId,
                    qlAccountResponse.SubstitutionKey.PrivateKey);
            }

            // Create initial balance for creator
            var creatorBalance = new TokenBalance
            {
                TokenId = token.Id,
                AccountId = submission.CreatorId,
                Balance = config.TotalSupply,
                LockedBalance = 0
            };

            // Deploy to MultiChain (mocked for now)
            string? multiChainAssetName = null;
            try
            {
                multiChainAssetName = await DeployToMultiChainAsync(token.Id);
                token.MultiChainAssetName = multiChainAssetName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MultiChain deployment failed for token {TokenId}", token.Id);
            }

            _context.Tokens.Add(token);
            _context.TokenBalances.Add(creatorBalance);

            // Update submission with token reference
            submission.TokenId = token.Id;

            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
                {
                    UserId = submission.CreatorId,
                    Type = "TokenCreated",
                    Title = "Token Created Successfully",
                    Message = $"Your token '{config.Name}' ({config.Symbol}) has been created successfully.",
                    Data = new Dictionary<string, object>
                    {
                        ["tokenId"] = token.Id,
                        ["symbol"] = config.Symbol,
                        ["totalSupply"] = config.TotalSupply,
                        ["quantumLedgerAccountId"] = qlAccountResponse.Account.AccountId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for token creation {TokenId}", token.Id);
            }

            _logger.LogInformation("Token {TokenId} created successfully for submission {SubmissionId}", 
                token.Id, submissionId);

            return new TokenCreationResponse
            {
                TokenId = token.Id,
                QuantumLedgerAccountId = qlAccountResponse.Account.AccountId,
                QuantumLedgerTransactionId = Guid.NewGuid(), // Mock for now
                QuantumLedgerSubstitutionKeyId = qlAccountResponse.SubstitutionKey?.SubstitutionKeyId,
                MultiChainAssetName = multiChainAssetName,
                Status = token.Status,
                CreatedAt = token.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating token for submission {SubmissionId}", submissionId);
            throw;
        }
    }

    #endregion

    #region Token Retrieval

    public async Task<TokenResponse> GetTokenAsync(Guid tokenId)
    {
        var token = await _context.Tokens
            .FirstOrDefaultAsync(t => t.Id == tokenId);

        if (token == null)
        {
            throw new ArgumentException($"Token {tokenId} not found");
        }

        return token.Adapt<TokenResponse>();
    }

    public async Task<List<TokenResponse>> GetTokensByCreatorAsync(Guid creatorId)
    {
        var tokens = await _context.Tokens
            .Where(t => t.CreatorId == creatorId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tokens.Adapt<List<TokenResponse>>();
    }

    public async Task<List<TokenResponse>> GetAllTokensAsync(int page = 1, int pageSize = 20)
    {
        var tokens = await _context.Tokens
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return tokens.Adapt<List<TokenResponse>>();
    }

    #endregion

    #region Token Submission Management - IMPLEMENTED

    public async Task<List<TokenSubmission>> GetPendingSubmissionsAsync()
    {
        try
        {
            var submissions = await _context.TokenSubmissions
                .Where(s => s.ApprovalStatus == "Pending")
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            return submissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending token submissions");
            throw;
        }
    }

    public async Task<TokenSubmission> GetSubmissionAsync(Guid submissionId)
    {
        try
        {
            var submission = await _context.TokenSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new ArgumentException($"Token submission {submissionId} not found");
            }

            return submission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token submission {SubmissionId}", submissionId);
            throw;
        }
    }

    public async Task<bool> ApproveTokenSubmissionAsync(TokenApprovalRequest request)
    {
        try
        {
            var submission = await _context.TokenSubmissions
                .FirstOrDefaultAsync(s => s.Id == request.SubmissionId);

            if (submission == null)
            {
                throw new ArgumentException($"Token submission {request.SubmissionId} not found");
            }

            if (submission.ApprovalStatus != "Pending")
            {
                throw new InvalidOperationException($"Token submission {request.SubmissionId} is not pending approval");
            }

            submission.ApprovalStatus = "Approved";
            submission.ReviewedAt = DateTime.UtcNow;
            submission.ReviewedBy = request.ReviewedBy;
            submission.ReviewComments = request.Comments;

            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
                {
                    UserId = submission.CreatorId,
                    Type = "TokenApproved",
                    Title = "Token Submission Approved",
                    Message = "Your token submission has been approved and is ready for creation.",
                    Data = new Dictionary<string, object>
                    {
                        ["submissionId"] = submission.Id,
                        ["reviewComments"] = request.Comments ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send approval notification for submission {SubmissionId}", submission.Id);
            }

            _logger.LogInformation("Token submission {SubmissionId} approved by {ReviewedBy}", 
                submission.Id, request.ReviewedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving token submission {SubmissionId}", request.SubmissionId);
            throw;
        }
    }

    public async Task<bool> RejectTokenSubmissionAsync(TokenApprovalRequest request)
    {
        try
        {
            var submission = await _context.TokenSubmissions
                .FirstOrDefaultAsync(s => s.Id == request.SubmissionId);

            if (submission == null)
            {
                throw new ArgumentException($"Token submission {request.SubmissionId} not found");
            }

            if (submission.ApprovalStatus != "Pending")
            {
                throw new InvalidOperationException($"Token submission {request.SubmissionId} is not pending approval");
            }

            submission.ApprovalStatus = "Rejected";
            submission.ReviewedAt = DateTime.UtcNow;
            submission.ReviewedBy = request.ReviewedBy;
            submission.ReviewComments = request.Comments;

            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
                {
                    UserId = submission.CreatorId,
                    Type = "TokenRejected",
                    Title = "Token Submission Rejected",
                    Message = "Your token submission has been rejected. Please review the comments and resubmit if needed.",
                    Data = new Dictionary<string, object>
                    {
                        ["submissionId"] = submission.Id,
                        ["reviewComments"] = request.Comments ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection notification for submission {SubmissionId}", submission.Id);
            }

            _logger.LogInformation("Token submission {SubmissionId} rejected by {ReviewedBy}", 
                submission.Id, request.ReviewedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting token submission {SubmissionId}", request.SubmissionId);
            throw;
        }
    }

    #endregion

    #region Token Balance Management - IMPLEMENTED

    public async Task<TokenBalanceResponse> GetTokenBalanceAsync(Guid accountId, Guid tokenId)
    {
        try
        {
            var balance = await _context.TokenBalances
                .FirstOrDefaultAsync(b => b.AccountId == accountId && b.TokenId == tokenId);

            if (balance == null)
            {
                return new TokenBalanceResponse
                {
                    AccountId = accountId,
                    TokenId = tokenId,
                    Balance = 0,
                    LockedBalance = 0,
                    AvailableBalance = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }

            return new TokenBalanceResponse
            {
                AccountId = balance.AccountId,
                TokenId = balance.TokenId,
                Balance = balance.Balance,
                LockedBalance = balance.LockedBalance,
                AvailableBalance = balance.Balance - balance.LockedBalance,
                LastUpdated = balance.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token balance for account {AccountId} and token {TokenId}", accountId, tokenId);
            throw;
        }
    }

    public async Task<List<TokenBalanceResponse>> GetAllBalancesAsync(Guid accountId)
    {
        try
        {
            var balances = await _context.TokenBalances
                .Where(b => b.AccountId == accountId && b.Balance > 0)
                .Include(b => b.Token)
                .ToListAsync();

            return balances.Select(b => new TokenBalanceResponse
            {
                AccountId = b.AccountId,
                TokenId = b.TokenId,
                Balance = b.Balance,
                LockedBalance = b.LockedBalance,
                AvailableBalance = b.Balance - b.LockedBalance,
                LastUpdated = b.LastUpdated,
                TokenSymbol = b.Token?.Symbol,
                TokenName = b.Token?.Name
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all balances for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> SyncBalanceWithQuantumLedgerAsync(Guid accountId, Guid tokenId)
    {
        try
        {
            var token = await _context.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId);
            if (token == null)
            {
                throw new ArgumentException($"Token {tokenId} not found");
            }

            // Get balance from QuantumLedger
            var qlBalance = await _quantumLedgerClient.GetAccountBalanceAsync(token.QuantumLedgerAccountId);
            
            // Update local balance
            var localBalance = await _context.TokenBalances
                .FirstOrDefaultAsync(b => b.AccountId == accountId && b.TokenId == tokenId);

            if (localBalance == null)
            {
                localBalance = new TokenBalance
                {
                    AccountId = accountId,
                    TokenId = tokenId,
                    Balance = qlBalance.Balance,
                    LockedBalance = 0
                };
                _context.TokenBalances.Add(localBalance);
            }
            else
            {
                localBalance.Balance = qlBalance.Balance;
                localBalance.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Synced balance for account {AccountId} and token {TokenId}: {Balance}", 
                accountId, tokenId, qlBalance.Balance);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing balance for account {AccountId} and token {TokenId}", accountId, tokenId);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetSignatureForTokenAsync(Guid tokenId, object requestBody)
    {
        var keyInfo = await _signatureService.GetSubstitutionKeyAsync(tokenId);
        if (keyInfo == null)
        {
            throw new InvalidOperationException($"No substitution key found for token {tokenId}");
        }

        var (signature, _) = await _signatureService.SignRequestAsync(
            requestBody, keyInfo.Value.substitutionKeyId, keyInfo.Value.privateKeyPem);
        
        return signature;
    }

    public async Task<string> DeployToMultiChainAsync(Guid tokenId)
    {
        // Mock implementation for Phase 2
        _logger.LogInformation("Mock: Deploying token {TokenId} to MultiChain", tokenId);
        await Task.Delay(100); // Simulate deployment time
        return $"multichain_asset_{tokenId:N}";
    }

    #endregion

    #region Not Yet Implemented - Will be completed in next phase

    public Task<TokenTransferResponse> TransferTokenAsync(TokenTransferRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<TokenTransfer> GetTransferAsync(Guid transferId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<List<TokenTransfer>> GetTransferHistoryAsync(Guid accountId, Guid? tokenId = null)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<ComplianceScore> CalculateComplianceScoreAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<List<string>> GetAiRecommendationsAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<List<string>> DetectRedFlagsAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<AssetVerificationResult> VerifyAssetAsync(AssetVerificationRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<OwnershipVerificationResult> VerifyOwnershipAsync(string assetId, Guid userId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<QuantumLedgerAccountCreationResponse> CreateQuantumLedgerAccountAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<QuantumLedgerTransactionResponse> CreateQuantumLedgerTransactionAsync(Guid tokenId, CreateQuantumLedgerTransactionRequest request)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<bool> SyncWithQuantumLedgerAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<string> TransferOnMultiChainAsync(Guid transferId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<bool> BurnOnMultiChainAsync(Guid tokenId, decimal amount)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<bool> SuspendTokenAsync(Guid tokenId, string reason)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<bool> ReactivateTokenAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<bool> BurnTokenAsync(Guid tokenId, decimal amount)
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    public Task<List<Token>> GetTokensRequiringReviewAsync()
    {
        throw new NotImplementedException("Will be implemented in next phase");
    }

    #endregion
}
