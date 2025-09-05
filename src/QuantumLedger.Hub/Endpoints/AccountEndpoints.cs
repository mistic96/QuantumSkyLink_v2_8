using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace QuantumLedger.Hub.Endpoints
{
    /// <summary>
    /// Account management endpoints using Minimal APIs.
    /// Provides multi-cloud account creation, management, and key operations.
    /// </summary>
    public static class AccountEndpoints
    {
        /// <summary>
        /// Maps all account-related endpoints to the application.
        /// </summary>
        public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
        {
            var accountGroup = app.MapGroup("/api/accounts")
                .WithTags("Accounts")
                .WithOpenApi();

            // Account creation endpoint
            accountGroup.MapPost("/", CreateAccountAsync)
                .WithName("CreateAccount")
                .WithSummary("Create a new account with multi-cloud key storage")
                .WithDescription("Creates a new account with external owner ID mapping and multi-algorithm key generation")
                .Produces<CreateAccountResponse>(201)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500);

            // Get account by ID
            accountGroup.MapGet("/{accountId:guid}", GetAccountAsync)
                .WithName("GetAccount")
                .WithSummary("Get account details by ID")
                .WithDescription("Retrieves account information including public keys and metadata")
                .Produces<AccountResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get account by external owner ID
            accountGroup.MapGet("/external/{externalOwnerId}", GetAccountByExternalOwnerIdAsync)
                .WithName("GetAccountByExternalOwnerId")
                .WithSummary("Get account by external owner ID")
                .WithDescription("Retrieves account information using external owner ID and vendor system")
                .Produces<AccountResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // List accounts with pagination
            accountGroup.MapGet("/", ListAccountsAsync)
                .WithName("ListAccounts")
                .WithSummary("List accounts with pagination")
                .WithDescription("Retrieves a paginated list of accounts with optional filtering")
                .Produces<ListAccountsResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500);

            // Update account status
            accountGroup.MapPut("/{accountId:guid}/status", UpdateAccountStatusAsync)
                .WithName("UpdateAccountStatus")
                .WithSummary("Update account status")
                .WithDescription("Updates the status of an account (Active, Suspended, Closed)")
                .Produces<AccountResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get account keys
            accountGroup.MapGet("/{accountId:guid}/keys", GetAccountKeysAsync)
                .WithName("GetAccountKeys")
                .WithSummary("Get account cryptographic keys")
                .WithDescription("Retrieves public keys and key metadata for an account")
                .Produces<AccountKeysResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Rotate account keys
            accountGroup.MapPost("/{accountId:guid}/keys/rotate", RotateAccountKeysAsync)
                .WithName("RotateAccountKeys")
                .WithSummary("Rotate account cryptographic keys")
                .WithDescription("Generates new keys for an account and updates the public key registry")
                .Produces<RotateKeysResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get account statistics
            accountGroup.MapGet("/{accountId:guid}/stats", GetAccountStatsAsync)
                .WithName("GetAccountStats")
                .WithSummary("Get account usage statistics")
                .WithDescription("Retrieves usage statistics and metrics for an account")
                .Produces<AccountStatsResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Verify account signature
            accountGroup.MapPost("/{accountId:guid}/verify", VerifyAccountSignatureAsync)
                .WithName("VerifyAccountSignature")
                .WithSummary("Verify a signature for an account")
                .WithDescription("Verifies a digital signature using the account's public keys")
                .Produces<SignatureVerificationResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);
        }

        /// <summary>
        /// Creates a new account with multi-cloud key storage.
        /// </summary>
        private static async Task<IResult> CreateAccountAsync(
            CreateAccountRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Creating account for external owner {ExternalOwnerId} in system {VendorSystem}", 
                    request.ExternalOwnerId, request.VendorSystem);

                // Mock implementation - replace with actual service call
                await Task.Delay(50); // Simulate account creation time

                var accountId = Guid.NewGuid();
                var internalReferenceId = $"QL-{DateTime.UtcNow:yyyyMMdd}-{accountId.ToString()[..8].ToUpper()}";

                var keys = new List<KeyInfo>
                {
                    new KeyInfo
                    {
                        Algorithm = "Dilithium",
                        PublicKey = "mock_dilithium_public_key_base64",
                        CloudProvider = "GoogleCloud",
                        CreatedAt = DateTime.UtcNow
                    },
                    new KeyInfo
                    {
                        Algorithm = "Falcon",
                        PublicKey = "mock_falcon_public_key_base64",
                        CloudProvider = "Azure",
                        CreatedAt = DateTime.UtcNow
                    },
                    new KeyInfo
                    {
                        Algorithm = "EC256",
                        PublicKey = "mock_ec256_public_key_base64",
                        CloudProvider = "AWS",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                logger.LogInformation("Successfully created account {AccountId} for external owner {ExternalOwnerId}", 
                    accountId, request.ExternalOwnerId);

                return Results.Created($"/api/accounts/{accountId}", new CreateAccountResponse
                {
                    AccountId = accountId,
                    ExternalOwnerId = request.ExternalOwnerId,
                    VendorSystem = request.VendorSystem,
                    InternalReferenceId = internalReferenceId,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    Keys = keys
                });
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("Invalid account creation request: {Message}", ex.Message);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating account for external owner {ExternalOwnerId}", request.ExternalOwnerId);
                return Results.Problem(
                    title: "Account Creation Error",
                    detail: "An error occurred while creating the account",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets account details by account ID.
        /// </summary>
        private static async Task<IResult> GetAccountAsync(
            Guid accountId,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving account {AccountId}", accountId);

                // Mock implementation
                await Task.Delay(10);

                var account = new AccountResponse
                {
                    AccountId = accountId,
                    ExternalOwnerId = "mock_external_id",
                    VendorSystem = "MockSystem",
                    OwnerType = "Client",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    LastActivity = DateTime.UtcNow.AddHours(-2)
                };

                return Results.Ok(account);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving account {AccountId}", accountId);
                return Results.Problem(
                    title: "Account Retrieval Error",
                    detail: "An error occurred while retrieving the account",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets account by external owner ID.
        /// </summary>
        private static async Task<IResult> GetAccountByExternalOwnerIdAsync(
            string externalOwnerId,
            string? vendorSystem,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving account for external owner {ExternalOwnerId} in system {VendorSystem}", 
                    externalOwnerId, vendorSystem);

                // Mock implementation
                await Task.Delay(10);

                var account = new AccountResponse
                {
                    AccountId = Guid.NewGuid(),
                    ExternalOwnerId = externalOwnerId,
                    VendorSystem = vendorSystem ?? "DefaultSystem",
                    OwnerType = "Client",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    LastActivity = DateTime.UtcNow.AddMinutes(-30)
                };

                return Results.Ok(account);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving account for external owner {ExternalOwnerId}", externalOwnerId);
                return Results.Problem(
                    title: "Account Retrieval Error",
                    detail: "An error occurred while retrieving the account",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Lists accounts with pagination.
        /// </summary>
        private static async Task<IResult> ListAccountsAsync(
            ILogger<string> logger,
            int page = 1,
            int pageSize = 20,
            string? status = null,
            string? vendorSystem = null)
        {
            try
            {
                logger.LogDebug("Listing accounts: page {Page}, size {PageSize}, status {Status}, vendor {VendorSystem}", 
                    page, pageSize, status, vendorSystem);

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Mock implementation
                await Task.Delay(20);

                var accounts = Enumerable.Range(1, pageSize).Select(i => new AccountSummary
                {
                    AccountId = Guid.NewGuid(),
                    ExternalOwnerId = $"external_owner_{i}",
                    VendorSystem = vendorSystem ?? "MockSystem",
                    OwnerType = "Client",
                    Status = status ?? "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    LastActivity = DateTime.UtcNow.AddHours(-i)
                }).ToList();

                var response = new ListAccountsResponse
                {
                    Accounts = accounts,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 1000, // Mock total
                    TotalPages = (int)Math.Ceiling(1000.0 / pageSize)
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error listing accounts");
                return Results.Problem(
                    title: "Account Listing Error",
                    detail: "An error occurred while listing accounts",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Updates account status.
        /// </summary>
        private static async Task<IResult> UpdateAccountStatusAsync(
            Guid accountId,
            UpdateAccountStatusRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Updating account {AccountId} status to {Status}", accountId, request.Status);

                // Validate status
                var validStatuses = new[] { "Active", "Suspended", "Closed" };
                if (!validStatuses.Contains(request.Status))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_status",
                        ErrorDescription = $"Status must be one of: {string.Join(", ", validStatuses)}",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Mock implementation
                await Task.Delay(15);

                var account = new AccountResponse
                {
                    AccountId = accountId,
                    ExternalOwnerId = "mock_external_id",
                    VendorSystem = "MockSystem",
                    OwnerType = "Client",
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    LastActivity = DateTime.UtcNow,
                    StatusUpdatedAt = DateTime.UtcNow,
                    StatusUpdatedBy = request.UpdatedBy
                };

                return Results.Ok(account);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating account {AccountId} status", accountId);
                return Results.Problem(
                    title: "Account Update Error",
                    detail: "An error occurred while updating the account status",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets account cryptographic keys.
        /// </summary>
        private static async Task<IResult> GetAccountKeysAsync(
            Guid accountId,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving keys for account {AccountId}", accountId);

                // Mock implementation
                await Task.Delay(10);

                var keys = new List<KeyInfo>
                {
                    new KeyInfo
                    {
                        Algorithm = "Dilithium",
                        PublicKey = "mock_dilithium_public_key_base64",
                        CloudProvider = "GoogleCloud",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        Status = "Active",
                        UsageCount = 150
                    },
                    new KeyInfo
                    {
                        Algorithm = "Falcon",
                        PublicKey = "mock_falcon_public_key_base64",
                        CloudProvider = "Azure",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        Status = "Active",
                        UsageCount = 75
                    },
                    new KeyInfo
                    {
                        Algorithm = "EC256",
                        PublicKey = "mock_ec256_public_key_base64",
                        CloudProvider = "AWS",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        Status = "Active",
                        UsageCount = 200
                    }
                };

                var response = new AccountKeysResponse
                {
                    AccountId = accountId,
                    Keys = keys,
                    TotalKeys = keys.Count,
                    ActiveKeys = keys.Count(k => k.Status == "Active")
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving keys for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Key Retrieval Error",
                    detail: "An error occurred while retrieving account keys",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Rotates account cryptographic keys.
        /// </summary>
        private static async Task<IResult> RotateAccountKeysAsync(
            Guid accountId,
            RotateKeysRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Rotating keys for account {AccountId}, algorithms: {Algorithms}", 
                    accountId, string.Join(", ", request.Algorithms ?? new[] { "All" }));

                // Mock implementation
                await Task.Delay(100); // Simulate key generation time

                var newKeys = new List<KeyInfo>
                {
                    new KeyInfo
                    {
                        Algorithm = "Dilithium",
                        PublicKey = "new_dilithium_public_key_base64",
                        CloudProvider = "GoogleCloud",
                        CreatedAt = DateTime.UtcNow,
                        Status = "Active",
                        UsageCount = 0
                    },
                    new KeyInfo
                    {
                        Algorithm = "Falcon",
                        PublicKey = "new_falcon_public_key_base64",
                        CloudProvider = "Azure",
                        CreatedAt = DateTime.UtcNow,
                        Status = "Active",
                        UsageCount = 0
                    },
                    new KeyInfo
                    {
                        Algorithm = "EC256",
                        PublicKey = "new_ec256_public_key_base64",
                        CloudProvider = "AWS",
                        CreatedAt = DateTime.UtcNow,
                        Status = "Active",
                        UsageCount = 0
                    }
                };

                var response = new RotateKeysResponse
                {
                    AccountId = accountId,
                    NewKeys = newKeys,
                    RotatedAt = DateTime.UtcNow,
                    RotatedBy = request.RequestedBy,
                    PreviousKeysStatus = "Deprecated"
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rotating keys for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Key Rotation Error",
                    detail: "An error occurred while rotating account keys",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets account usage statistics.
        /// </summary>
        private static async Task<IResult> GetAccountStatsAsync(
            Guid accountId,
            ILogger<string> logger,
            int days = 30)
        {
            try
            {
                logger.LogDebug("Retrieving stats for account {AccountId} for {Days} days", accountId, days);

                // Mock implementation
                await Task.Delay(15);

                var stats = new AccountStatsResponse
                {
                    AccountId = accountId,
                    PeriodDays = days,
                    TotalTransactions = 1250,
                    TotalSignatures = 1250,
                    SignaturesByAlgorithm = new Dictionary<string, int>
                    {
                        ["Dilithium"] = 500,
                        ["Falcon"] = 300,
                        ["EC256"] = 450
                    },
                    AverageTransactionsPerDay = 1250.0 / days,
                    LastTransactionAt = DateTime.UtcNow.AddHours(-2),
                    MostUsedAlgorithm = "Dilithium",
                    SecurityScore = 95.5
                };

                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stats for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Stats Retrieval Error",
                    detail: "An error occurred while retrieving account statistics",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Verifies a signature for an account.
        /// </summary>
        private static async Task<IResult> VerifyAccountSignatureAsync(
            Guid accountId,
            VerifySignatureRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Verifying signature for account {AccountId} using algorithm {Algorithm}", 
                    accountId, request.Algorithm);

                // Mock implementation
                await Task.Delay(25); // Simulate signature verification time

                var response = new SignatureVerificationResponse
                {
                    IsValid = true,
                    AccountId = accountId,
                    Algorithm = request.Algorithm,
                    VerifiedAt = DateTime.UtcNow,
                    Message = "Signature verification successful",
                    PublicKeyUsed = $"mock_{request.Algorithm.ToLower()}_public_key",
                    VerificationTimeMs = 25
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying signature for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Signature Verification Error",
                    detail: "An error occurred while verifying the signature",
                    statusCode: 500);
            }
        }
    }

    #region Request/Response Models

    public class CreateAccountRequest
    {
        [Required]
        public string ExternalOwnerId { get; set; } = string.Empty;
        
        public string? OwnerIdType { get; set; }
        
        [Required]
        public string VendorSystem { get; set; } = string.Empty;
        
        [Required]
        public string OwnerType { get; set; } = "Client";
        
        public string? InternalReferenceId { get; set; }
        
        public string[]? PreferredCloudProviders { get; set; }
        
        public string[]? RequiredAlgorithms { get; set; }
    }

    public class CreateAccountResponse
    {
        public Guid AccountId { get; set; }
        public string ExternalOwnerId { get; set; } = string.Empty;
        public string VendorSystem { get; set; } = string.Empty;
        public string? InternalReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<KeyInfo> Keys { get; set; } = new();
    }

    public class AccountResponse
    {
        public Guid AccountId { get; set; }
        public string ExternalOwnerId { get; set; } = string.Empty;
        public string VendorSystem { get; set; } = string.Empty;
        public string OwnerType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedBy { get; set; }
    }

    public class AccountSummary
    {
        public Guid AccountId { get; set; }
        public string ExternalOwnerId { get; set; } = string.Empty;
        public string VendorSystem { get; set; } = string.Empty;
        public string OwnerType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class ListAccountsResponse
    {
        public List<AccountSummary> Accounts { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateAccountStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Reason { get; set; }
        
        public string? UpdatedBy { get; set; }
    }

    public class KeyInfo
    {
        public string Algorithm { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string CloudProvider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
    }

    public class AccountKeysResponse
    {
        public Guid AccountId { get; set; }
        public List<KeyInfo> Keys { get; set; } = new();
        public int TotalKeys { get; set; }
        public int ActiveKeys { get; set; }
    }

    public class RotateKeysRequest
    {
        public string[]? Algorithms { get; set; }
        public string? Reason { get; set; }
        public string? RequestedBy { get; set; }
    }

    public class RotateKeysResponse
    {
        public Guid AccountId { get; set; }
        public List<KeyInfo> NewKeys { get; set; } = new();
        public DateTime RotatedAt { get; set; }
        public string? RotatedBy { get; set; }
        public string PreviousKeysStatus { get; set; } = string.Empty;
    }

    public class AccountStatsResponse
    {
        public Guid AccountId { get; set; }
        public int PeriodDays { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalSignatures { get; set; }
        public Dictionary<string, int> SignaturesByAlgorithm { get; set; } = new();
        public double AverageTransactionsPerDay { get; set; }
        public DateTime? LastTransactionAt { get; set; }
        public string MostUsedAlgorithm { get; set; } = string.Empty;
        public double SecurityScore { get; set; }
    }

    public class VerifySignatureRequest
    {
        [Required]
        public string Algorithm { get; set; } = string.Empty;
        
        [Required]
        public string Signature { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? Nonce { get; set; }
    }

    public class SignatureVerificationResponse
    {
        public bool IsValid { get; set; }
        public Guid AccountId { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? PublicKeyUsed { get; set; }
        public double VerificationTimeMs { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? ErrorCode { get; set; }
    }

    #endregion
}
