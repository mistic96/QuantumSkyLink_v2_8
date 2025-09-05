using Microsoft.EntityFrameworkCore;
using Mapster;
using TokenService.Data;
using TokenService.Data.Entities;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;
using System.Security.Claims;

namespace TokenService.Services;

public class TokenService : ITokenService
{
    private readonly TokenDbContext _context;
    private readonly IQuantumLedgerClient _quantumLedgerClient;
    private readonly IQuantumLedgerSignatureService _signatureService;
    private readonly IAiComplianceService _aiComplianceService;
    private readonly IAssetVerificationService _assetVerificationService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TokenService> _logger;
    private readonly IConfiguration _configuration;

    public TokenService(
        TokenDbContext context,
        IQuantumLedgerClient quantumLedgerClient,
        IQuantumLedgerSignatureService signatureService,
        IAiComplianceService aiComplianceService,
        IAssetVerificationService assetVerificationService,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        ILogger<TokenService> logger,
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

            // Create QuantumLedger transaction for token creation
            var qlTransactionRequest = new CreateQuantumLedgerTransactionRequest
            {
                AccountId = qlAccountResponse.Account.AccountId,
                Type = "TokenCreation",
                Amount = config.TotalSupply,
                Currency = config.Symbol,
                Description = $"Token creation: {config.Name} ({config.Symbol})",
                Metadata = new Dictionary<string, object>
                {
                    ["tokenId"] = token.Id,
                    ["tokenType"] = config.TokenType,
                    ["creatorId"] = submission.CreatorId,
                    ["multiChainAsset"] = multiChainAssetName ?? "pending"
                }
            };

            var qlTransactionResponse = await _quantumLedgerClient.CreateTransactionAsync(
                qlTransactionRequest,
                await GetSignatureForTokenAsync(token.Id, qlTransactionRequest),
                qlAccountResponse.SubstitutionKey?.SubstitutionKeyId ?? "");

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
                QuantumLedgerTransactionId = qlTransactionResponse.TransactionId,
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

    #endregion

    #region Not Yet Implemented (Phase 3)

    public Task<bool> ApproveTokenSubmissionAsync(TokenApprovalRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> RejectTokenSubmissionAsync(TokenApprovalRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<TokenSubmission>> GetPendingSubmissionsAsync()
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<TokenSubmission> GetSubmissionAsync(Guid submissionId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<TokenTransferResponse> TransferTokenAsync(TokenTransferRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<TokenTransfer> GetTransferAsync(Guid transferId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<TokenTransfer>> GetTransferHistoryAsync(Guid accountId, Guid? tokenId = null)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<TokenBalanceResponse> GetTokenBalanceAsync(Guid accountId, Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<TokenBalanceResponse>> GetAllBalancesAsync(Guid accountId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> SyncBalanceWithQuantumLedgerAsync(Guid accountId, Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<ComplianceScore> CalculateComplianceScoreAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<string>> GetAiRecommendationsAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<string>> DetectRedFlagsAsync(TokenSubmissionRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<AssetVerificationResult> VerifyAssetAsync(AssetVerificationRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<OwnershipVerificationResult> VerifyOwnershipAsync(string assetId, Guid userId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<QuantumLedgerAccountCreationResponse> CreateQuantumLedgerAccountAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<QuantumLedgerTransactionResponse> CreateQuantumLedgerTransactionAsync(Guid tokenId, CreateQuantumLedgerTransactionRequest request)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> SyncWithQuantumLedgerAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<string> DeployToMultiChainAsync(Guid tokenId)
    {
        // Mock implementation for Phase 2
        _logger.LogInformation("Mock: Deploying token {TokenId} to MultiChain", tokenId);
        return Task.FromResult($"multichain_asset_{tokenId:N}");
    }

    public Task<string> TransferOnMultiChainAsync(Guid transferId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> BurnOnMultiChainAsync(Guid tokenId, decimal amount)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> SuspendTokenAsync(Guid tokenId, string reason)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> ReactivateTokenAsync(Guid tokenId)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<bool> BurnTokenAsync(Guid tokenId, decimal amount)
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    public Task<List<Token>> GetTokensRequiringReviewAsync()
    {
        throw new NotImplementedException("Will be implemented in Phase 3");
    }

    #endregion
}
