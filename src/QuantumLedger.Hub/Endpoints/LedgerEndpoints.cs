using Microsoft.AspNetCore.Mvc;
using QuantumLedger.Models;
using QuantumLedger.Hub.Features.CQRS;
using QuantumLedger.Hub.Features.Accounts.Queries;
using QuantumLedger.Hub.Features.Transactions.Commands;
using QuantumLedger.Hub.Features.Transactions.Queries;
using QuantumLedger.Hub.Models;
using System.Text.RegularExpressions;

namespace QuantumLedger.Hub.Endpoints
{
    /// <summary>
    /// Core ledger endpoints using Minimal APIs.
    /// Direct conversion of LedgerController to maintain exact same functionality.
    /// </summary>
    public static class LedgerEndpoints
    {
        /// <summary>
        /// Maps all core ledger endpoints to the application.
        /// </summary>
        public static void MapLedgerEndpoints(this IEndpointRouteBuilder app)
        {
            var ledgerGroup = app.MapGroup("/api/v1/ledger")
                .WithTags("Ledger")
                .WithOpenApi();

            // Submit transaction endpoint
            ledgerGroup.MapPost("/transactions", SubmitTransactionAsync)
                .WithName("SubmitTransaction")
                .WithSummary("Submit a new transaction")
                .WithDescription("Processes a new transaction request using CQRS pattern")
                .Produces<ProcessTransactionResult>(201)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500);

            // Get transaction by ID endpoint
            ledgerGroup.MapGet("/transactions/{id}", GetTransactionAsync)
                .WithName("GetTransaction")
                .WithSummary("Get transaction by ID")
                .WithDescription("Retrieves transaction details by transaction ID")
                .Produces<GetTransactionResult>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get account balance by address endpoint
            ledgerGroup.MapGet("/accounts/{address}", GetAccountBalanceAsync)
                .WithName("GetAccountBalance")
                .WithSummary("Get account balance by blockchain address")
                .WithDescription("Retrieves account balance using blockchain address")
                .Produces<GetAccountBalanceResult>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);
        }

        /// <summary>
        /// Submits a new transaction for processing.
        /// </summary>
        private static async Task<IResult> SubmitTransactionAsync(
            Request request,
            IRequestHandler<ProcessTransactionCommand, ProcessTransactionResult> processTransactionHandler,
            ILogger<string> logger,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Enhanced input validation
                if (request == null)
                {
                    logger.LogWarning("Received null transaction request");
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request",
                        ErrorDescription = "Request cannot be null",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    logger.LogWarning("Received transaction request with empty ID");
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request_id",
                        ErrorDescription = "Request ID cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (request.Id.Length > 100) // Reasonable limit
                {
                    logger.LogWarning("Received transaction request with ID too long: {RequestId}", request.Id);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request_id",
                        ErrorDescription = "Request ID too long",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate request ID format (alphanumeric with hyphens)
                if (!IsValidRequestId(request.Id))
                {
                    logger.LogWarning("Received transaction request with invalid ID format: {RequestId}", request.Id);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_request_id_format",
                        ErrorDescription = "Invalid request ID format",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Sanitize input to prevent injection attacks
                request.Id = SanitizeInput(request.Id);

                logger.LogInformation("Received transaction request: {RequestId}", request.Id);

                var command = new ProcessTransactionCommand(request);
                var result = await processTransactionHandler.Handle(command, cancellationToken);

                if (!result.Success)
                {
                    logger.LogWarning("Transaction processing failed for request {RequestId}: {ErrorMessage}", 
                        request.Id, result.ErrorMessage);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "transaction_processing_failed",
                        ErrorDescription = result.ErrorMessage ?? "Failed to process transaction",
                        Timestamp = DateTime.UtcNow
                    });
                }

                logger.LogInformation("Transaction processed successfully for request {RequestId}", request.Id);
                return Results.Created($"/api/v1/ledger/transactions/{request.Id}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing transaction request");
                return Results.Problem(
                    title: "Transaction Processing Error",
                    detail: "An error occurred while processing the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets transaction details by transaction ID.
        /// </summary>
        private static async Task<IResult> GetTransactionAsync(
            string id,
            IRequestHandler<GetTransactionQuery, GetTransactionResult> getTransactionHandler,
            ILogger<string> logger,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Enhanced input validation
                if (string.IsNullOrWhiteSpace(id))
                {
                    logger.LogWarning("Received get transaction request with empty ID");
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_transaction_id",
                        ErrorDescription = "Transaction ID cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (id.Length > 100) // Reasonable limit
                {
                    logger.LogWarning("Received get transaction request with ID too long: {TransactionId}", id);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_transaction_id",
                        ErrorDescription = "Transaction ID too long",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate transaction ID format
                if (!IsValidRequestId(id))
                {
                    logger.LogWarning("Received get transaction request with invalid ID format: {TransactionId}", id);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_transaction_id_format",
                        ErrorDescription = "Invalid transaction ID format",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Sanitize input to prevent injection attacks
                id = SanitizeInput(id);

                logger.LogInformation("Retrieving transaction: {TransactionId}", id);

                var query = new GetTransactionQuery(id);
                var result = await getTransactionHandler.Handle(query, cancellationToken);

                if (!result.Success)
                {
                    logger.LogWarning("Transaction not found: {TransactionId}", id);
                    return Results.NotFound(new ErrorResponse
                    {
                        Error = "transaction_not_found",
                        ErrorDescription = result.ErrorMessage ?? "Transaction not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                logger.LogDebug("Transaction retrieved successfully: {TransactionId}", id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
                return Results.Problem(
                    title: "Transaction Retrieval Error",
                    detail: "An error occurred while retrieving the transaction",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets account balance by blockchain address.
        /// </summary>
        private static async Task<IResult> GetAccountBalanceAsync(
            string address,
            IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResult> getAccountBalanceHandler,
            ILogger<string> logger,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Enhanced input validation
                if (string.IsNullOrWhiteSpace(address))
                {
                    logger.LogWarning("Received get balance request with empty address");
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_address",
                        ErrorDescription = "Address cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (address.Length > 100) // Reasonable limit
                {
                    logger.LogWarning("Received get balance request with address too long: {Address}", address);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_address",
                        ErrorDescription = "Address too long",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate address format (blockchain-specific)
                if (!IsValidBlockchainAddress(address))
                {
                    logger.LogWarning("Received get balance request with invalid address format: {Address}", address);
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_address_format",
                        ErrorDescription = "Invalid address format",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Sanitize input to prevent injection attacks
                address = SanitizeInput(address);

                logger.LogInformation("Retrieving balance for account: {Address}", address);

                var query = new GetAccountBalanceQuery(address);
                var result = await getAccountBalanceHandler.Handle(query, cancellationToken);

                if (!result.Success)
                {
                    logger.LogWarning("Account not found: {Address}", address);
                    return Results.NotFound(new ErrorResponse
                    {
                        Error = "account_not_found",
                        ErrorDescription = result.ErrorMessage ?? "Account not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                logger.LogDebug("Balance retrieved successfully for account: {Address}", address);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving balance for address {Address}", address);
                return Results.Problem(
                    title: "Balance Retrieval Error",
                    detail: "An error occurred while retrieving the account balance",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Validates if a request ID has a valid format (alphanumeric with hyphens and underscores)
        /// </summary>
        /// <param name="requestId">The request ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidRequestId(string requestId)
        {
            // Allow alphanumeric characters, hyphens, and underscores
            return Regex.IsMatch(requestId, @"^[a-zA-Z0-9\-_]+$");
        }

        /// <summary>
        /// Sanitizes input to prevent injection attacks
        /// </summary>
        /// <param name="input">The input to sanitize</param>
        /// <returns>Sanitized input</returns>
        private static string SanitizeInput(string input)
        {
            // Remove potentially dangerous characters, keep only alphanumeric, hyphens, and underscores
            return Regex.Replace(input, @"[^\w\-]", "");
        }

        /// <summary>
        /// Validates if a blockchain address has a valid format
        /// </summary>
        /// <param name="address">The address to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidBlockchainAddress(string address)
        {
            // Basic blockchain address validation (alphanumeric, reasonable length)
            return address.All(c => char.IsLetterOrDigit(c)) && 
                   address.Length >= 26 && address.Length <= 62;
        }
    }
}
