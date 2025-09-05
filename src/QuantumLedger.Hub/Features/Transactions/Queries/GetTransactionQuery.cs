using Microsoft.Extensions.Logging;
using QuantumLedger.Models;
using QuantumLedger.Models.Interfaces;
using QuantumLedger.Hub.Features.CQRS;

namespace QuantumLedger.Hub.Features.Transactions.Queries;

public record GetTransactionQuery(string TransactionId) : IRequest<GetTransactionResult>;

public record GetTransactionResult(
    bool Success,
    Request? Transaction = null,
    string? ErrorMessage = null
);

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, GetTransactionResult>
{
    private readonly IRepository<Request> _requestRepository;
    private readonly ILogger<GetTransactionQueryHandler> _logger;

    public GetTransactionQueryHandler(
        IRepository<Request> requestRepository,
        ILogger<GetTransactionQueryHandler> logger)
    {
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetTransactionResult> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _requestRepository.GetByIdAsync(query.TransactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found: {TransactionId}", query.TransactionId);
                return new GetTransactionResult(false, ErrorMessage: "Transaction not found");
            }

            _logger.LogInformation("Transaction retrieved successfully: {TransactionId}", query.TransactionId);
            return new GetTransactionResult(true, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction: {TransactionId}", query.TransactionId);
            return new GetTransactionResult(false, ErrorMessage: "Internal error retrieving transaction");
        }
    }
}
