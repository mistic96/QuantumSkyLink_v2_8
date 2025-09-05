using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Blockchain.Repositories;

namespace QuantumLedger.Blockchain
{
    /// <summary>
    /// Default implementation of the blockchain event handler.
    /// </summary>
    public class DefaultBlockchainEventHandler : IBlockchainEventHandler
    {
        private readonly ILogger<DefaultBlockchainEventHandler> _logger;
        private readonly IBlockchainEventRepository _eventRepository;
        private readonly ITransactionEventRepository _transactionRepository;
        private readonly IBlockEventRepository _blockRepository;
        private readonly IAccountStateEventRepository _accountStateRepository;

        /// <summary>
        /// Initializes a new instance of the DefaultBlockchainEventHandler class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="eventRepository">The repository for all blockchain events.</param>
        /// <param name="transactionRepository">The repository for transaction events.</param>
        /// <param name="blockRepository">The repository for block events.</param>
        /// <param name="accountStateRepository">The repository for account state events.</param>
        public DefaultBlockchainEventHandler(
            ILogger<DefaultBlockchainEventHandler> logger,
            IBlockchainEventRepository eventRepository,
            ITransactionEventRepository transactionRepository,
            IBlockEventRepository blockRepository,
            IAccountStateEventRepository accountStateRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _blockRepository = blockRepository ?? throw new ArgumentNullException(nameof(blockRepository));
            _accountStateRepository = accountStateRepository ?? throw new ArgumentNullException(nameof(accountStateRepository));
        }

        /// <inheritdoc/>
        public async Task HandleTransactionSubmittedAsync(TransactionSubmittedEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _logger.LogInformation(
                "Transaction submitted: {TransactionId} from {FromAddress} to {ToAddress} amount {Amount}",
                evt.Transaction.Id,
                evt.Transaction.FromAddress,
                evt.Transaction.ToAddress,
                evt.Transaction.Amount);

            // Store in both repositories
            await _eventRepository.AddAsync(evt);
            await _transactionRepository.AddAsync(evt);
        }

        /// <inheritdoc/>
        public async Task HandleTransactionStatusChangedAsync(TransactionStatusChangedEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _logger.LogInformation(
                "Transaction {TransactionId} status changed from {PreviousStatus} to {NewStatus}",
                evt.TransactionId,
                evt.PreviousStatus,
                evt.NewStatus);

            if (evt.BlockNumber.HasValue)
            {
                _logger.LogInformation(
                    "Transaction {TransactionId} included in block {BlockNumber}",
                    evt.TransactionId,
                    evt.BlockNumber.Value);
            }

            // Store in the general event repository
            await _eventRepository.AddAsync(evt);
        }

        /// <inheritdoc/>
        public async Task HandleBlockCreatedAsync(BlockCreatedEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _logger.LogInformation(
                "Block created: Number {BlockNumber}, Hash {BlockHash}, Transactions {TransactionCount}",
                evt.Block.Number,
                evt.Block.Hash,
                evt.Block.TransactionIds.Count);

            // Store in both repositories
            await _eventRepository.AddAsync(evt);
            await _blockRepository.AddAsync(evt);
        }

        /// <inheritdoc/>
        public async Task HandleAccountStateChangedAsync(AccountStateChangedEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _logger.LogInformation(
                "Account {Address} balance changed from {PreviousBalance} to {NewBalance} by transaction {TransactionId}",
                evt.Address,
                evt.PreviousBalance,
                evt.NewBalance,
                evt.TransactionId);

            // Store in both repositories
            await _eventRepository.AddAsync(evt);
            await _accountStateRepository.AddAsync(evt);
        }

        /// <inheritdoc/>
        public async Task HandleBlockchainErrorAsync(BlockchainErrorEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _logger.LogError(
                "Blockchain error: Code {ErrorCode}, Message {ErrorMessage}, Transaction {TransactionId}",
                evt.ErrorCode,
                evt.ErrorMessage,
                evt.RelatedTransactionId);

            // Store in the general event repository
            await _eventRepository.AddAsync(evt);
        }

    }
}
