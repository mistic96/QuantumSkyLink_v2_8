using GovernanceService.Services.Interfaces;
using Refit;

namespace GovernanceService.Services.Interfaces;

public interface ITokenServiceClient
{
    [Get("/api/Token/user/{userId}/holdings")]
    Task<ApiResponse<List<TokenHolding>>> GetUserTokenHoldingsAsync(Guid userId);

    [Get("/api/Token/user/{userId}/balance")]
    Task<ApiResponse<decimal>> GetUserTokenBalanceAsync(Guid userId);

    [Get("/api/Token/governance-tokens")]
    Task<ApiResponse<List<GovernanceToken>>> GetGovernanceTokensAsync();

    [Get("/api/Token/total-supply")]
    Task<ApiResponse<decimal>> GetTotalTokenSupplyAsync();

    [Post("/api/Token/lock")]
    Task<ApiResponse<TokenLockResponse>> LockTokensForVotingAsync([Body] LockTokensRequest request);

    [Post("/api/Token/unlock")]
    Task<ApiResponse<bool>> UnlockTokensAsync([Body] UnlockTokensRequest request);
}

public class GovernanceToken
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal VotingWeight { get; set; }
    public decimal TotalSupply { get; set; }
    public bool IsActive { get; set; }
}

public class LockTokensRequest
{
    public Guid UserId { get; set; }
    public Guid TokenId { get; set; }
    public decimal Amount { get; set; }
    public Guid ProposalId { get; set; }
    public DateTime LockUntil { get; set; }
}

public class UnlockTokensRequest
{
    public Guid UserId { get; set; }
    public Guid TokenId { get; set; }
    public decimal Amount { get; set; }
    public Guid ProposalId { get; set; }
}

public class TokenLockResponse
{
    public Guid LockId { get; set; }
    public decimal LockedAmount { get; set; }
    public DateTime LockedUntil { get; set; }
    public bool Success { get; set; }
}
