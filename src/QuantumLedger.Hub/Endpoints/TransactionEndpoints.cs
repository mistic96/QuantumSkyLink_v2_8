using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuantumLedger.Hub.Models;
using QuantumLedger.Hub.Features.CQRS;
using QuantumLedger.Hub.Features.Transactions.Commands;
using QuantumLedger.Hub.Features.Transactions.Queries;
using QuantumLedger.Models;
using System.Text.RegularExpressions;

namespace QuantumLedger.Hub.Endpoints
{
    /// <summary>
    /// Transaction endpoints using Minimal APIs.
    /// Handles core ledger transaction operations with CQRS integration.
    /// </summary>
    public static class TransactionEndpoints
    {
        /// <summary>
        /// Maps all transaction-related endpoints to the application.
        /// </summary>
        public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
        {
            var transactionGroup = app.MapGroup("/api/transactions")
                .WithTags("Transactions")
                .WithOpenApi();

            // Legacy endpoint for CQRS compatibility
            transactionGroup.MapPost("/", SubmitTransactionAsync)
                .WithName("SubmitTransactionLegacy")
                .WithSummary("Submit a new transaction")
                .WithDescription("Submits a transaction using the existing CQRS infrastructure")
                .Produces<TransactionResponse>(201)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500);

            // Get specific transaction
            transactionGroup.MapGet("/{transactionId}", GetTransactionAsync)
                .WithName("GetTransactionDetails")
                .WithSummary("Get transaction details")
                .WithDescription("Retrieves details for a specific transaction using CQRS")
                .Produces<TransactionResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Create transaction (modern API)
            transactionGroup.MapPost("/create", CreateTransactionAsync)
                .WithName("CreateTransaction")
                .WithSummary("Create a new transaction")
                .WithDescription("Creates a new transaction with modern request structure")
                .Produces<TransactionResponse>(201)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(401)
                .Produces<ErrorResponse>(500);

            // Validate transaction
            transactionGroup.MapPost("/validate", ValidateTransactionAsync)
                .WithName("ValidateTransaction")
                .WithSummary("Validate a transaction")
                .WithDescription("Validates a transaction without executing it")
                .Produces<ValidationResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500);

            // Cancel transaction
            transactionGroup.MapDelete("/{transactionId}", CancelTransactionAsync)
                .WithName("CancelTransaction")
                .WithSummary("Cancel a pending transaction")
                .WithDescription("Cancels a pending transaction if possible")
                .Produces<CancelTransactionResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);
        }

        /// <summary>
        /// Submits a transaction using the legacy CQRS infrastructure.
        /// </summary>
        private static async Task<IResult> SubmitTransactionAsync(
            LegacyTransactionRequest request,
            IRequestHandler<ProcessTransactionCommand, ProcessTransactionResult> processTransactionHandler,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Received legacy transaction request: {RequestId}", request.Id);

                // Enhanced input validation
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request",
                        ErrorDescription = "Request ID cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (request.Id.Length > 100)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request",
                        ErrorDescription = "Request ID too long",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate request ID format (alphanumeric with hyphens)
                if (!IsValidRequestId(request.Id))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_format",
                        ErrorDescription = "Invalid request ID format",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Sanitize input to prevent injection attacks
                request.Id = SanitizeInput(request.Id);

                // Convert to legacy Request model for CQRS
                var legacyRequest = new Request
                {
                    Id = request.Id,
                    // Map other properties as needed
                };

                var command = new ProcessTransactionCommand(legacyRequest);
                var result = await processTransactionHandler.Handle(command, CancellationToken.None);

                if (!result.Success)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "transaction_failed",
                        ErrorDescription = result.ErrorMessage ?? "Failed to process transaction",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Convert result to modern response format
                var response = new TransactionResponse
                {
                    TransactionId = Guid.NewGuid(), // Mock - should come from result
                    Type = request.Type ?? "Unknown",
                    Amount = request.Amount,
                    Currency = "QLT",
                    Status = "Submitted",
                    CreatedAt = DateTime.UtcNow,
                    TransactionFee = request.Amount * 0.001m
                };

                logger.LogInformation("Successfully processed legacy transaction {RequestId}", request.Id);

                return Results.Created($"/api/transactions/{response.TransactionId}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing legacy transaction {RequestId}", request.Id);
                return Results.Problem(
                    title: "Transaction Processing Error",
                    detail: "An error occurred while processing the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets transaction details using CQRS infrastructure.
        /// </summary>
        private static async Task<IResult> GetTransactionAsync(
            string transactionId,
            IRequestHandler<GetTransactionQuery, GetTransactionResult> getTransactionHandler,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving transaction: {TransactionId}", transactionId);

                // Enhanced input validation
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request",
                        ErrorDescription = "Transaction ID cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (transactionId.Length > 100)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request",
                        ErrorDescription = "Transaction ID too long",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate transaction ID format
                if (!IsValidRequestId(transactionId))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_format",
                        ErrorDescription = "Invalid transaction ID format",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Sanitize input
                transactionId = SanitizeInput(transactionId);

                var query = new GetTransactionQuery(transactionId);
                var result = await getTransactionHandler.Handle(query, CancellationToken.None);

                if (!result.Success)
                {
                    return Results.NotFound(new ErrorResponse
                    {
                        Error = "transaction_not_found",
                        ErrorDescription = result.ErrorMessage ?? "Transaction not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Convert result to modern response format
                var response = new TransactionResponse
                {
                    TransactionId = Guid.NewGuid(), // Mock - should come from result
                    Type = "Transfer",
                    Amount = 500.00m,
                    Currency = "QLT",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ProcessedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(5),
                    BlockHash = $"0x{Guid.NewGuid():N}",
                    BlockNumber = 12345678,
                    TransactionFee = 0.50m,
                    GasUsed = 21000,
                    GasPrice = 20,
                    Confirmations = 15,
                    Description = "Transaction retrieved via CQRS"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving transaction {TransactionId}", transactionId);
                return Results.Problem(
                    title: "Transaction Retrieval Error",
                    detail: "An error occurred while retrieving the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Creates a new transaction with modern request structure.
        /// </summary>
        private static async Task<IResult> CreateTransactionAsync(
            CreateTransactionRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Creating {Type} transaction for account {AccountId}", 
                    request.Type, request.AccountId);

                // Validate transaction request
                if (request.Amount <= 0)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_amount",
                        ErrorDescription = "Transaction amount must be greater than zero",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Mock implementation
                await Task.Delay(75);

                var transactionId = Guid.NewGuid();
                var blockHash = $"0x{Guid.NewGuid():N}";

                var transaction = new TransactionResponse
                {
                    TransactionId = transactionId,
                    Type = request.Type,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "QLT",
                    Status = "Pending",
                    FromAccountId = request.Type == "Withdrawal" ? request.AccountId : null,
                    ToAccountId = request.Type == "Deposit" ? request.AccountId : request.ToAccountId,
                    CreatedAt = DateTime.UtcNow,
                    BlockHash = blockHash,
                    TransactionFee = request.Amount * 0.001m,
                    Description = request.Description,
                    Metadata = request.Metadata ?? new Dictionary<string, object>()
                };

                logger.LogInformation("Successfully created transaction {TransactionId}", transactionId);

                return Results.Created($"/api/transactions/{transactionId}", transaction);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating transaction for account {AccountId}", request.AccountId);
                return Results.Problem(
                    title: "Transaction Creation Error",
                    detail: "An error occurred while creating the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Validates a transaction without executing it.
        /// </summary>
        private static async Task<IResult> ValidateTransactionAsync(
            ValidateTransactionRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Validating transaction for account {AccountId}", request.AccountId);

                // Mock validation logic
                await Task.Delay(30);

                var validationErrors = new List<string>();
                var warnings = new List<string>();

                // Simulate validation checks
                if (request.Amount <= 0)
                    validationErrors.Add("Amount must be greater than zero");

                if (request.Amount > 1000000)
                    warnings.Add("Large transaction amount detected");

                var isValid = !validationErrors.Any();
                var estimatedFee = request.Amount * 0.001m;
                var estimatedGas = 21000;

                var response = new ValidationResponse
                {
                    IsValid = isValid,
                    ValidationErrors = validationErrors,
                    Warnings = warnings,
                    EstimatedFee = estimatedFee,
                    EstimatedGas = estimatedGas,
                    EstimatedProcessingTime = TimeSpan.FromMinutes(2),
                    ValidatedAt = DateTime.UtcNow
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating transaction for account {AccountId}", request.AccountId);
                return Results.Problem(
                    title: "Validation Error",
                    detail: "An error occurred while validating the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Cancels a pending transaction.
        /// </summary>
        private static async Task<IResult> CancelTransactionAsync(
            Guid transactionId,
            ILogger<string> logger,
            string? reason = null)
        {
            try
            {
                logger.LogInformation("Cancelling transaction {TransactionId}", transactionId);

                // Mock implementation
                await Task.Delay(25);

                var response = new CancelTransactionResponse
                {
                    TransactionId = transactionId,
                    Status = "Cancelled",
                    CancelledAt = DateTime.UtcNow,
                    Reason = reason ?? "User requested cancellation",
                    RefundAmount = 250.00m,
                    RefundFee = 0.25m
                };

                logger.LogInformation("Successfully cancelled transaction {TransactionId}", transactionId);

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cancelling transaction {TransactionId}", transactionId);
                return Results.Problem(
                    title: "Transaction Cancellation Error",
                    detail: "An error occurred while cancelling the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Validates if a request ID has a valid format (alphanumeric with hyphens and underscores)
        /// </summary>
        private static bool IsValidRequestId(string requestId)
        {
            // Allow alphanumeric characters, hyphens, and underscores
            return Regex.IsMatch(requestId, @"^[a-zA-Z0-9\-_]+$");
        }

        /// <summary>
        /// Sanitizes input to prevent injection attacks
        /// </summary>
        private static string SanitizeInput(string input)
        {
            // Remove potentially dangerous characters, keep only alphanumeric, hyphens, and underscores
            return Regex.Replace(input, @"[^\w\-]", "");
        }
    }
}
