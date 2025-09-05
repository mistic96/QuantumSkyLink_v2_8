using TokenService.Models.Requests;
using TokenService.Models.Responses;

namespace TokenService.Services.Interfaces;

public interface IAssetVerificationService
{
    /// <summary>
    /// Verifies the authenticity and ownership of an asset
    /// </summary>
    Task<AssetVerificationResult> VerifyAssetAsync(AssetVerificationRequest request);
    
    /// <summary>
    /// Verifies ownership of an asset by a specific user
    /// </summary>
    Task<OwnershipVerificationResult> VerifyOwnershipAsync(string assetId, Guid userId);
    
    /// <summary>
    /// Validates asset documents and their authenticity
    /// </summary>
    Task<bool> ValidateDocumentsAsync(List<AssetDocument> documents);
    
    /// <summary>
    /// Gets supported asset types for tokenization
    /// </summary>
    Task<List<string>> GetSupportedAssetTypesAsync();
    
    /// <summary>
    /// Initiates asynchronous verification process
    /// </summary>
    Task<string> InitiateVerificationAsync(AssetVerificationRequest request);
    
    /// <summary>
    /// Gets verification status by verification ID
    /// </summary>
    Task<AssetVerificationResult> GetVerificationStatusAsync(string verificationId);
    
    /// <summary>
    /// Handles verification webhook callbacks
    /// </summary>
    Task HandleVerificationCallbackAsync(string verificationId, Dictionary<string, object> callbackData);
    
    /// <summary>
    /// Estimates verification time for asset type
    /// </summary>
    Task<TimeSpan> EstimateVerificationTimeAsync(string assetType);
}
