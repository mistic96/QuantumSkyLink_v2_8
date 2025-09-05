using TokenService.Data.Entities;
using TokenService.Models.Requests;
using TokenService.Models.Responses;

namespace TokenService.Services.Interfaces;

public interface ITokenService
{
    // Token Submission and Creation
    Task<TokenSubmissionResponse> SubmitTokenAsync(TokenSubmissionRequest request);
    Task<TokenCreationResponse> CreateTokenAsync(Guid submissionId);
    Task<TokenResponse> GetTokenAsync(Guid tokenId);
    Task<List<TokenResponse>> GetTokensByCreatorAsync(Guid creatorId);
    Task<List<TokenResponse>> GetAllTokensAsync(int page = 1, int pageSize = 20);
    
    // Token Approval and Governance
    Task<bool> ApproveTokenSubmissionAsync(TokenApprovalRequest request);
    Task<bool> RejectTokenSubmissionAsync(TokenApprovalRequest request);
    Task<List<TokenSubmission>> GetPendingSubmissionsAsync();
    Task<TokenSubmission> GetSubmissionAsync(Guid submissionId);
    
    // Token Transfers
    Task<TokenTransferResponse> TransferTokenAsync(TokenTransferRequest request);
    Task<TokenTransfer> GetTransferAsync(Guid transferId);
    Task<List<TokenTransfer>> GetTransferHistoryAsync(Guid accountId, Guid? tokenId = null);
    
    // Token Balances
    Task<TokenBalanceResponse> GetTokenBalanceAsync(Guid accountId, Guid tokenId);
    Task<List<TokenBalanceResponse>> GetAllBalancesAsync(Guid accountId);
    Task<bool> SyncBalanceWithQuantumLedgerAsync(Guid accountId, Guid tokenId);
    
    // AI Compliance and Scoring
    Task<ComplianceScore> CalculateComplianceScoreAsync(TokenSubmissionRequest request);
    Task<List<string>> GetAiRecommendationsAsync(TokenSubmissionRequest request);
    Task<List<string>> DetectRedFlagsAsync(TokenSubmissionRequest request);
    
    // Asset Verification
    Task<AssetVerificationResult> VerifyAssetAsync(AssetVerificationRequest request);
    Task<OwnershipVerificationResult> VerifyOwnershipAsync(string assetId, Guid userId);
    
    // QuantumLedger Integration
    Task<QuantumLedgerAccountCreationResponse> CreateQuantumLedgerAccountAsync(Guid tokenId);
    Task<QuantumLedgerTransactionResponse> CreateQuantumLedgerTransactionAsync(
        Guid tokenId, 
        CreateQuantumLedgerTransactionRequest request);
    Task<bool> SyncWithQuantumLedgerAsync(Guid tokenId);
    
    // MultiChain Operations (Mock initially)
    Task<string> DeployToMultiChainAsync(Guid tokenId);
    Task<string> TransferOnMultiChainAsync(Guid transferId);
    Task<bool> BurnOnMultiChainAsync(Guid tokenId, decimal amount);
    
    // Admin Operations
    Task<bool> SuspendTokenAsync(Guid tokenId, string reason);
    Task<bool> ReactivateTokenAsync(Guid tokenId);
    Task<bool> BurnTokenAsync(Guid tokenId, decimal amount);
    Task<List<Token>> GetTokensRequiringReviewAsync();
}
