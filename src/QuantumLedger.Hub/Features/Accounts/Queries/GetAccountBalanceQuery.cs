using Microsoft.Extensions.Logging;
using QuantumLedger.Models;
using QuantumLedger.Models.Interfaces;
using QuantumLedger.Hub.Features.CQRS;

namespace QuantumLedger.Hub.Features.Accounts.Queries;

public record GetAccountBalanceQuery(string Address) : IRequest<GetAccountBalanceResult>;

public record GetAccountBalanceResult(
    bool Success,
    AccountBalance? Balance = null,
    string? ErrorMessage = null
);

public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResult>
{
    private readonly IRepository<AccountBalance> _accountBalanceRepository;
    private readonly ILogger<GetAccountBalanceQueryHandler> _logger;

    public GetAccountBalanceQueryHandler(
        IRepository<AccountBalance> accountBalanceRepository,
        ILogger<GetAccountBalanceQueryHandler> logger)
    {
        _accountBalanceRepository = accountBalanceRepository ?? throw new ArgumentNullException(nameof(accountBalanceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetAccountBalanceResult> Handle(GetAccountBalanceQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var balance = await _accountBalanceRepository.GetByIdAsync(query.Address);
            if (balance == null)
            {
                _logger.LogWarning("Account balance not found: {Address}", query.Address);
                return new GetAccountBalanceResult(false, ErrorMessage: "Account not found");
            }

            _logger.LogInformation("Account balance retrieved successfully: {Address}", query.Address);
            return new GetAccountBalanceResult(true, balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account balance: {Address}", query.Address);
            return new GetAccountBalanceResult(false, ErrorMessage: "Internal error retrieving account balance");
        }
    }
}
