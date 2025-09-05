using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;
using QuantumLedger.Data;
using QuantumLedger.Models.Account;

namespace QuantumLedger.Cryptography.Services
{
    /// <summary>
    /// Service for creating accounts with multi-cloud key management, external owner ID support, and substitution keys.
    /// Implements the revolutionary cost optimization strategy with hybrid encryption and delegation key system.
    /// </summary>
    public class AccountCreationService
    {
        private readonly AccountsContext _accountsContext;
        private readonly ICloudKeyVaultFactory _keyVaultFactory;
        private readonly ICloudStorageProvider _storageProvider;
        private readonly ISubstitutionKeyService _substitutionKeyService;
        private readonly ILogger<AccountCreationService> _logger;

        public AccountCreationService(
            AccountsContext accountsContext,
            ICloudKeyVaultFactory keyVaultFactory,
            ICloudStorageProvider storageProvider,
            ISubstitutionKeyService substitutionKeyService,
            ILogger<AccountCreationService> logger)
        {
            _accountsContext = accountsContext ?? throw new ArgumentNullException(nameof(accountsContext));
            _keyVaultFactory = keyVaultFactory ?? throw new ArgumentNullException(nameof(keyVaultFactory));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _substitutionKeyService = substitutionKeyService ?? throw new ArgumentNullException(nameof(substitutionKeyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new account with multi-algorithm key generation, cloud storage, and substitution keys.
        /// Supports flexible external owner ID mapping for multi-vendor scenarios.
        /// Implements the hybrid security model with custodial main keys and non-custodial substitution keys.
        /// </summary>
        /// <param name="request">The account creation request.</param>
        /// <returns>The created account with key information and substitution key.</returns>
        public async Task<EnhancedAccountCreationResult> CreateAccountAsync(CreateAccountRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.IsValid())
                throw new ArgumentException("Invalid account creation request", nameof(request));

            // Generate a unique address for the account (needed for error case too)
            var address = request.RequestedAddress ?? GenerateAccountAddress();

            try
            {
                _logger.LogInformation("Creating account for external owner {ExternalOwnerId} of type {OwnerIdType} from vendor {VendorSystem}",
                    request.ExternalOwnerId, request.OwnerIdType, request.VendorSystem);

                // Check if account already exists
                var existingAccount = await _accountsContext.Accounts
                    .FirstOrDefaultAsync(a => a.ExternalOwnerId == request.ExternalOwnerId && 
                                            a.VendorSystem == request.VendorSystem);

                if (existingAccount != null)
                {
                    throw new InvalidOperationException($"Account already exists for external owner {request.ExternalOwnerId} from vendor {request.VendorSystem}");
                }

                // Create the account
                var account = new Account
                {
                    AccountId = Guid.NewGuid(),
                    ExternalOwnerId = request.ExternalOwnerId,
                    OwnerIdType = request.OwnerIdType,
                    VendorSystem = request.VendorSystem,
                    InternalReferenceId = request.GenerateInternalReferenceId ? GenerateULID() : null,
                    OwnerType = request.OwnerType,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                // Add account to database
                _accountsContext.Accounts.Add(account);
                await _accountsContext.SaveChangesAsync();

                _logger.LogInformation("Account {AccountId} created successfully for external owner {ExternalOwnerId} with address {Address}",
                    account.AccountId, request.ExternalOwnerId, address);

                // Generate main keys for requested algorithms (system custody)
                var keyResults = new List<AccountKeyResult>();
                string? classicKeyId = null;
                string? quantumKeyId = null;

                foreach (var algorithm in request.Algorithms)
                {
                    try
                    {
                        var keyResult = await CreateAccountKeyAsync(account.AccountId, algorithm, address);
                        keyResults.Add(keyResult);

                        // Track key IDs for substitution key creation
                        if (algorithm == "EC256" && keyResult.Success)
                        {
                            classicKeyId = keyResult.KeyId?.ToString();
                        }
                        else if ((algorithm == "Dilithium" || algorithm == "Falcon") && keyResult.Success)
                        {
                            quantumKeyId = keyResult.KeyId?.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create key for algorithm {Algorithm} for account {AccountId}",
                            algorithm, account.AccountId);
                        
                        // Continue with other algorithms, but log the failure
                        keyResults.Add(new AccountKeyResult
                        {
                            Algorithm = algorithm,
                            Success = false,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                // Generate substitution key (user custody) if enabled
                SubstitutionKeyPair? substitutionKey = null;
                if (request.GenerateSubstitutionKey)
                {
                    try
                    {
                        substitutionKey = await _substitutionKeyService.GenerateSubstitutionKeyAsync(
                            address, 
                            request.SubstitutionKeyExpiresAt);

                        _logger.LogInformation(
                            "Generated substitution key {SubstitutionKeyId} for account {AccountId} (address: {Address})",
                            substitutionKey.SubstitutionKeyId, account.AccountId, address);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate substitution key for account {AccountId}", account.AccountId);
                        // Continue without substitution key - this is not a fatal error
                    }
                }

                return new EnhancedAccountCreationResult
                {
                    Address = address,
                    Account = account,
                    KeyResults = keyResults,
                    SubstitutionKey = substitutionKey,
                    ClassicKeyId = classicKeyId,
                    QuantumKeyId = quantumKeyId,
                    Success = true,
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>
                    {
                        ["HybridSecurity"] = "true",
                        ["MainKeysCount"] = keyResults.Count(k => k.Success).ToString(),
                        ["SubstitutionKeyGenerated"] = (substitutionKey != null).ToString(),
                        ["CostOptimized"] = "true"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account for external owner {ExternalOwnerId}",
                    request.ExternalOwnerId);
                
                return new EnhancedAccountCreationResult
                {
                    Address = address, // Provide the address even in error case
                    Success = false,
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Creates a cryptographic key for an account using the specified algorithm.
        /// Implements hybrid encryption with cloud storage for cost optimization.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="algorithm">The cryptographic algorithm.</param>
        /// <param name="address">The account address.</param>
        /// <returns>The key creation result.</returns>
        public async Task<AccountKeyResult> CreateAccountKeyAsync(Guid accountId, string algorithm, string address)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Account ID cannot be empty", nameof(accountId));
            if (string.IsNullOrWhiteSpace(algorithm))
                throw new ArgumentException("Algorithm cannot be null or empty", nameof(algorithm));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));
            if (!AccountKey.IsValidAlgorithm(algorithm))
                throw new ArgumentException($"Unsupported algorithm: {algorithm}", nameof(algorithm));

            try
            {
                _logger.LogDebug("Creating {Algorithm} key for account {AccountId} with address {Address}", 
                    algorithm, accountId, address);

                // Get optimal key vault provider using cost optimization
                var keyVaultProvider = _keyVaultFactory.GetOptimalProvider();

                // Generate key pair based on algorithm
                var (publicKey, privateKey) = await GenerateKeyPairAsync(algorithm);

                // Encrypt private key using optimal key vault provider
                var encryptedPrivateKey = await keyVaultProvider.EncryptAsync(
                    privateKey, accountId.ToString(), algorithm);

                // Store encrypted private key in cloud storage
                var storagePath = await _storageProvider.StoreAsync(
                    encryptedPrivateKey, 
                    accountId.ToString(), 
                    algorithm,
                    new Dictionary<string, string>
                    {
                        ["key-type"] = "private",
                        ["algorithm"] = algorithm,
                        ["account-id"] = accountId.ToString(),
                        ["address"] = address,
                        ["created-at"] = DateTime.UtcNow.ToString("O"),
                        ["provider"] = keyVaultProvider.ProviderName,
                        ["key-category"] = algorithm == "EC256" ? "Traditional" : "PostQuantum"
                    });

                // Create account key record
                var accountKey = new AccountKey
                {
                    KeyId = Guid.NewGuid(),
                    AccountId = accountId,
                    Algorithm = algorithm,
                    PublicKey = Convert.ToBase64String(publicKey),
                    CloudProvider = keyVaultProvider.ProviderName,
                    StoragePath = storagePath,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                // Add to database
                _accountsContext.AccountKeys.Add(accountKey);

                // Create public key registry entry for fast signature verification
                var registryEntry = accountKey.ToRegistryEntry();
                _accountsContext.PublicKeyRegistry.Add(registryEntry);

                await _accountsContext.SaveChangesAsync();

                _logger.LogInformation("Successfully created {Algorithm} key {KeyId} for account {AccountId} (address: {Address}) using {Provider} with storage path {StoragePath}",
                    algorithm, accountKey.KeyId, accountId, address, keyVaultProvider.ProviderName, storagePath);

                return new AccountKeyResult
                {
                    KeyId = accountKey.KeyId,
                    Algorithm = algorithm,
                    PublicKey = Convert.ToBase64String(publicKey),
                    StoragePath = storagePath,
                    CloudProvider = keyVaultProvider.ProviderName,
                    Success = true,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create {Algorithm} key for account {AccountId} (address: {Address})", 
                    algorithm, accountId, address);
                
                return new AccountKeyResult
                {
                    Algorithm = algorithm,
                    Success = false,
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Retrieves an account by external owner ID and vendor system.
        /// </summary>
        /// <param name="externalOwnerId">The external owner identifier.</param>
        /// <param name="vendorSystem">The vendor system (optional).</param>
        /// <returns>The account if found; otherwise, null.</returns>
        public async Task<Account?> GetAccountByExternalOwnerIdAsync(string externalOwnerId, string? vendorSystem = null)
        {
            if (string.IsNullOrWhiteSpace(externalOwnerId))
                throw new ArgumentException("External owner ID cannot be null or empty", nameof(externalOwnerId));

            try
            {
                var query = _accountsContext.Accounts
                    .Include(a => a.AccountKeys)
                    .Include(a => a.PublicKeyEntries)
                    .Where(a => a.ExternalOwnerId == externalOwnerId);

                if (!string.IsNullOrWhiteSpace(vendorSystem))
                {
                    query = query.Where(a => a.VendorSystem == vendorSystem);
                }

                var account = await query.FirstOrDefaultAsync();

                _logger.LogDebug("Account lookup for external owner {ExternalOwnerId} from vendor {VendorSystem}: {Found}",
                    externalOwnerId, vendorSystem ?? "Any", account != null ? "Found" : "Not Found");

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve account for external owner {ExternalOwnerId} from vendor {VendorSystem}",
                    externalOwnerId, vendorSystem);
                throw;
            }
        }

        /// <summary>
        /// Lists all accounts for a specific vendor system.
        /// </summary>
        /// <param name="vendorSystem">The vendor system.</param>
        /// <param name="pageSize">The page size for pagination.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <returns>A paginated list of accounts.</returns>
        public async Task<PaginatedResult<Account>> GetAccountsByVendorSystemAsync(
            string vendorSystem, int pageSize = 50, int pageNumber = 1)
        {
            if (string.IsNullOrWhiteSpace(vendorSystem))
                throw new ArgumentException("Vendor system cannot be null or empty", nameof(vendorSystem));
            if (pageSize <= 0 || pageSize > 1000)
                throw new ArgumentException("Page size must be between 1 and 1000", nameof(pageSize));
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

            try
            {
                var query = _accountsContext.Accounts
                    .Where(a => a.VendorSystem == vendorSystem)
                    .OrderBy(a => a.CreatedAt);

                var totalCount = await query.CountAsync();
                var accounts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Account>
                {
                    Items = accounts,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve accounts for vendor system {VendorSystem}", vendorSystem);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Generates a key pair for the specified algorithm.
        /// </summary>
        private async Task<(byte[] publicKey, byte[] privateKey)> GenerateKeyPairAsync(string algorithm)
        {
            // In production, this would use the actual cryptographic providers
            // For development, simulate key generation
            await Task.Delay(10); // Simulate key generation time

            var keySize = algorithm switch
            {
                "Dilithium" => 1312, // Dilithium2 public key size
                "Falcon" => 897,     // Falcon-512 public key size  
                "EC256" => 65,       // Uncompressed EC P-256 public key size
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };

            var privateKeySize = algorithm switch
            {
                "Dilithium" => 2528, // Dilithium2 private key size
                "Falcon" => 1281,    // Falcon-512 private key size
                "EC256" => 32,       // EC P-256 private key size
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };

            // Generate random keys for simulation
            var publicKey = new byte[keySize];
            var privateKey = new byte[privateKeySize];
            
            System.Security.Cryptography.RandomNumberGenerator.Fill(publicKey);
            System.Security.Cryptography.RandomNumberGenerator.Fill(privateKey);

            return (publicKey, privateKey);
        }

        /// <summary>
        /// Generates a ULID (Universally Unique Lexicographically Sortable Identifier).
        /// </summary>
        private static string GenerateULID()
        {
            // Simple ULID implementation for development
            // In production, use a proper ULID library
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var random = new byte[10];
            System.Security.Cryptography.RandomNumberGenerator.Fill(random);
            
            var ulid = $"{timestamp:X12}{Convert.ToHexString(random)}";
            return ulid.ToUpperInvariant();
        }

        /// <summary>
        /// Generates a unique account address.
        /// </summary>
        private static string GenerateAccountAddress()
        {
            // Generate a unique address for the account
            // In production, this would follow specific address format requirements
            var addressBytes = new byte[20]; // 160-bit address
            System.Security.Cryptography.RandomNumberGenerator.Fill(addressBytes);
            return $"ql{Convert.ToHexString(addressBytes).ToLowerInvariant()}";
        }

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// Request model for creating a new account with substitution key support.
    /// </summary>
    public class CreateAccountRequest
    {
        /// <summary>
        /// Gets or sets the external owner identifier.
        /// </summary>
        public string ExternalOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of external owner ID.
        /// </summary>
        public string? OwnerIdType { get; set; }

        /// <summary>
        /// Gets or sets the vendor system.
        /// </summary>
        public string? VendorSystem { get; set; }

        /// <summary>
        /// Gets or sets the owner type.
        /// </summary>
        public OwnerType OwnerType { get; set; } = OwnerType.Client;

        /// <summary>
        /// Gets or sets the algorithms to generate keys for.
        /// </summary>
        public List<string> Algorithms { get; set; } = new() { "Dilithium", "Falcon", "EC256" };

        /// <summary>
        /// Gets or sets whether to generate an internal reference ID (ULID).
        /// </summary>
        public bool GenerateInternalReferenceId { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to generate a substitution key for user-controlled delegation.
        /// </summary>
        public bool GenerateSubstitutionKey { get; set; } = true;

        /// <summary>
        /// Gets or sets the expiration date for the substitution key (defaults to 1 year).
        /// </summary>
        public DateTime? SubstitutionKeyExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the requested address (optional - will be generated if not provided).
        /// </summary>
        public string? RequestedAddress { get; set; }

        /// <summary>
        /// Validates the request.
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(ExternalOwnerId)) return false;
            if (ExternalOwnerId.Length > 500) return false;
            if (Algorithms == null || Algorithms.Count == 0) return false;
            if (Algorithms.Any(a => !AccountKey.IsValidAlgorithm(a))) return false;
            
            return true;
        }
    }

    /// <summary>
    /// Enhanced result model for account creation with substitution key support.
    /// </summary>
    public class EnhancedAccountCreationResult : AccountCreationResult
    {
        /// <summary>
        /// Gets or sets the account address.
        /// </summary>
        public required string Address { get; set; }

        /// <summary>
        /// Gets or sets the substitution key pair (given to user for delegation).
        /// </summary>
        public SubstitutionKeyPair? SubstitutionKey { get; set; }

        /// <summary>
        /// Gets or sets the classical key ID (kept by system).
        /// </summary>
        public string? ClassicKeyId { get; set; }

        /// <summary>
        /// Gets or sets the quantum key ID (kept by system).
        /// </summary>
        public string? QuantumKeyId { get; set; }

        /// <summary>
        /// Gets or sets additional metadata about the account creation.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Result model for account creation.
    /// </summary>
    public class AccountCreationResult
    {
        /// <summary>
        /// Gets or sets the created account.
        /// </summary>
        public Account? Account { get; set; }

        /// <summary>
        /// Gets or sets the key creation results.
        /// </summary>
        public List<AccountKeyResult> KeyResults { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the creation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if creation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets when the creation was attempted.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Result model for account key creation.
    /// </summary>
    public class AccountKeyResult
    {
        /// <summary>
        /// Gets or sets the key identifier.
        /// </summary>
        public Guid? KeyId { get; set; }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public key in base64 format.
        /// </summary>
        public string? PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the storage path for the private key.
        /// </summary>
        public string? StoragePath { get; set; }

        /// <summary>
        /// Gets or sets the cloud provider used.
        /// </summary>
        public string? CloudProvider { get; set; }

        /// <summary>
        /// Gets or sets whether the key creation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if creation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets when the key was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Generic paginated result model.
    /// </summary>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total count of items.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets whether there is a next page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Gets whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }

    #endregion
}
