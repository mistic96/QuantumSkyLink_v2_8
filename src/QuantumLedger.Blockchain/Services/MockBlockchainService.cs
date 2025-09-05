using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuantumLedger.Blockchain.Models;

namespace QuantumLedger.Blockchain.Services
{
    /// <summary>
    /// Mock implementation of the blockchain service for MVP development.
    /// </summary>
    public class MockBlockchainService : IBlockchainService
    {
        private readonly ConcurrentDictionary<string, Transaction> _transactions;
        private readonly ConcurrentDictionary<string, TransactionStatus> _transactionStatuses;
        private readonly ConcurrentDictionary<string, AccountState> _accountStates;
        private readonly ConcurrentDictionary<string, BlockInfo> _blocks;
        private long _currentBlockNumber;
        private readonly Random _random;

        public MockBlockchainService()
        {
            _transactions = new ConcurrentDictionary<string, Transaction>();
            _transactionStatuses = new ConcurrentDictionary<string, TransactionStatus>();
            _accountStates = new ConcurrentDictionary<string, AccountState>();
            _blocks = new ConcurrentDictionary<string, BlockInfo>();
            _currentBlockNumber = 0;
            _random = new Random();
        }

        /// <inheritdoc/>
        public async Task<Result<string>> SubmitTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                return Result<string>.Failure("Transaction cannot be null");

            // Validate transaction
            if (string.IsNullOrWhiteSpace(transaction.FromAddress) || 
                string.IsNullOrWhiteSpace(transaction.ToAddress) || 
                transaction.Amount <= 0)
            {
                return Result<string>.Failure("Invalid transaction parameters");
            }

            // Generate transaction ID
            transaction.Id = Guid.NewGuid().ToString("N");
            transaction.Timestamp = DateTime.UtcNow;
            transaction.Metadata ??= new Dictionary<string, string>();

            // Store transaction
            if (!_transactions.TryAdd(transaction.Id, transaction))
            {
                return Result<string>.Failure("Failed to store transaction");
            }

            // Create initial status
            var status = new TransactionStatus
            {
                TransactionId = transaction.Id,
                Status = TransactionStatuses.Pending,
                LastUpdated = DateTime.UtcNow
            };
            _transactionStatuses.TryAdd(transaction.Id, status);

            // Simulate async processing
            _ = Task.Run(async () =>
            {
                await Task.Delay(_random.Next(1000, 3000)); // Random delay
                await ProcessTransactionAsync(transaction);
            });

            return Result<string>.Success(transaction.Id);
        }

        /// <inheritdoc/>
        public Task<Result<TransactionStatus>> GetTransactionStatusAsync(string txId)
        {
            if (string.IsNullOrWhiteSpace(txId))
                return Task.FromResult(Result<TransactionStatus>.Failure("Transaction ID cannot be null or empty"));

            if (_transactionStatuses.TryGetValue(txId, out var status))
                return Task.FromResult(Result<TransactionStatus>.Success(status));

            return Task.FromResult(Result<TransactionStatus>.Failure("Transaction not found"));
        }

        /// <inheritdoc/>
        public Task<Result<BlockInfo>> GetBlockInfoAsync(string blockId)
        {
            if (string.IsNullOrWhiteSpace(blockId))
                return Task.FromResult(Result<BlockInfo>.Failure("Block ID cannot be null or empty"));

            if (_blocks.TryGetValue(blockId, out var block))
                return Task.FromResult(Result<BlockInfo>.Success(block));

            return Task.FromResult(Result<BlockInfo>.Failure("Block not found"));
        }

        /// <inheritdoc/>
        public Task<Result<AccountState>> GetAccountStateAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return Task.FromResult(Result<AccountState>.Failure("Address cannot be null or empty"));

            if (_accountStates.TryGetValue(address, out var state))
                return Task.FromResult(Result<AccountState>.Success(state));

            // Create new account state if it doesn't exist
            var newState = new AccountState
            {
                Address = address,
                Balance = 0,
                Nonce = 0,
                LastUpdated = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>()
            };
            _accountStates.TryAdd(address, newState);

            return Task.FromResult(Result<AccountState>.Success(newState));
        }

        private async Task ProcessTransactionAsync(Transaction transaction)
        {
            try
            {
                // Get or create account states
                var fromResult = await GetAccountStateAsync(transaction.FromAddress);
                var toResult = await GetAccountStateAsync(transaction.ToAddress);

                if (!fromResult.IsSuccess || !toResult.IsSuccess)
                {
                    UpdateTransactionStatus(transaction.Id, TransactionStatuses.Failed, "Invalid account state");
                    return;
                }

                var fromState = fromResult.Data;
                var toState = toResult.Data;

                // Check balance
                if (fromState.Balance < transaction.Amount)
                {
                    UpdateTransactionStatus(transaction.Id, TransactionStatuses.Rejected, "Insufficient balance");
                    return;
                }

                // Update balances
                fromState.Balance -= transaction.Amount;
                toState.Balance += transaction.Amount;
                fromState.Nonce++;
                fromState.LastUpdated = DateTime.UtcNow;
                toState.LastUpdated = DateTime.UtcNow;

                // Create new block
                var blockNumber = Interlocked.Increment(ref _currentBlockNumber);
                var block = new BlockInfo
                {
                    Number = blockNumber,
                    Hash = Guid.NewGuid().ToString("N"),
                    PreviousHash = _blocks.Values.OrderByDescending(b => b.Number).FirstOrDefault()?.Hash,
                    Timestamp = DateTime.UtcNow,
                    TransactionIds = new List<string> { transaction.Id }
                };

                _blocks.TryAdd(block.Hash, block);

                // Update transaction status
                UpdateTransactionStatus(transaction.Id, TransactionStatuses.Confirmed, null, blockNumber);
            }
            catch (Exception ex)
            {
                UpdateTransactionStatus(transaction.Id, TransactionStatuses.Failed, ex.Message);
            }
        }

        private void UpdateTransactionStatus(string txId, string status, string errorMessage = null, long? blockNumber = null)
        {
            if (_transactionStatuses.TryGetValue(txId, out var currentStatus))
            {
                currentStatus.Status = status;
                currentStatus.ErrorMessage = errorMessage;
                currentStatus.BlockNumber = blockNumber;
                currentStatus.LastUpdated = DateTime.UtcNow;
            }
        }
    }
}
