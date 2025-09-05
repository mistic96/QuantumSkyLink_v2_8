using Microsoft.Extensions.Logging;
using QuantumLedger.Models;
using QuantumLedger.Models.Validation;
using QuantumLedger.Models.Interfaces;
using QuantumLedger.Hub.Features.CQRS;

namespace QuantumLedger.Hub.Features.Transactions.Commands;

public record ProcessTransactionCommand(Request Request) : IRequest<ProcessTransactionResult>;

public record ProcessTransactionResult(
    bool Success,
    string? TransactionId = null,
    string? ErrorMessage = null
);

public class ProcessTransactionCommandHandler : IRequestHandler<ProcessTransactionCommand, ProcessTransactionResult>
{
    private readonly IRequestValidator _validator;
    private readonly IRepository<Request> _requestRepository;
    private readonly ILogger<ProcessTransactionCommandHandler> _logger;

    public ProcessTransactionCommandHandler(
        IRequestValidator validator,
        IRepository<Request> requestRepository,
        ILogger<ProcessTransactionCommandHandler> logger)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessTransactionResult> Handle(ProcessTransactionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(command.Request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Transaction validation failed: {Errors}", 
                    string.Join(", ", validationResult.Errors));
                return new ProcessTransactionResult(false, ErrorMessage: validationResult.Errors.First());
            }

            // Store the request
            await _requestRepository.AddAsync(command.Request);

            _logger.LogInformation("Transaction processed successfully: {TransactionId}", command.Request.Id);
            return new ProcessTransactionResult(true, command.Request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction");
            return new ProcessTransactionResult(false, ErrorMessage: "Internal error processing transaction");
        }
    }
}
