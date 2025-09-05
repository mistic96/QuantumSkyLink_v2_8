using Refit;

namespace TreasuryService.Services.Interfaces;

[Headers("Accept: application/json", "X-API-Version: 1.0")]
public interface ITokenServiceClient
{
    /// <summary>
    /// Get token information
    /// </summary>
    [Get("/api/tokens/{tokenId}")]
    Task<TokenInfo> GetTokenAsync(Guid tokenId);

    /// <summary>
    /// Get token balance for an account
    /// </summary>
    [Get("/api/token-balances/{accountId}/{tokenId}")]
    Task<TokenBalanceInfo> GetTokenBalanceAsync(Guid accountId, Guid tokenId);

    /// <summary>
    /// Get all token balances for an account
    /// </summary>
    [Get("/api/token-balances/{accountId}")]
    Task<List<TokenBalanceInfo>> GetAllTokenBalancesAsync(Guid accountId);

    /// <summary>
    /// Transfer tokens between accounts
    /// </summary>
    [Post("/api/token-transfers")]
    Task<TokenTransferResult> TransferTokensAsync([Body] TokenTransferRequest request);

    /// <summary>
    /// Get tokens created by a user
    /// </summary>
    [Get("/api/tokens/creator/{creatorId}")]
    Task<List<TokenInfo>> GetTokensByCreatorAsync(Guid creatorId);
}

public class TokenInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalSupply { get; set; }
    public int Decimals { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TokenBalanceInfo
{
    public Guid AccountId { get; set; }
    public Guid TokenId { get; set; }
    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? TokenSymbol { get; set; }
    public string? TokenName { get; set; }
}

public class TokenTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public Guid TokenId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class TokenTransferResult
{
    public Guid TransferId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
