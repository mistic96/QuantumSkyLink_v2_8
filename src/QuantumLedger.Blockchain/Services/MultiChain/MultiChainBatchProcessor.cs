using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumLedger.Blockchain.Models;

namespace QuantumLedger.Blockchain.Services.MultiChain
{
    /// <summary>
    /// Configuration options for batch processing.
    /// </summary>
    public class BatchProcessingOptions
    {
        /// <summary>
        /// Gets or sets the maximum batch size for transactions.
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the batch timeout in milliseconds.
        /// </summary>
        public int BatchTimeoutMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the maximum number of concurrent batches.
        /// </summary>
        public int MaxConcurrentBatches { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to enable batch processing.
        /// </summary>
        public bool EnableBatchProcessing { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum batch size to trigger processing.
        /// </summary>
        public int MinBatchSize { get; set; } = 10;
    }

    /// <summary>
    /// Represents a batch operation request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class BatchRequest<TRequest, TResponse>
    {
        /// <summary>
        /// Gets or sets the unique identifier for this request.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the request data.
        /// </summary>
        public TRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the task completion source for the response.
        /// </summary>
        public TaskCompletionSource<TResponse> CompletionSource { get; set; } = new();

        /// <summary>
        /// Gets or sets the timestamp when the request was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the priority of the request (higher values = higher priority).
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Manages batch processing for MultiChain operations to optimize performance.
    /// </summary>
    public class MultiChainBatchProcessor : IDisposable
    {
        private readonly ILogger<MultiChainBatchProcessor> _logger;
        private readonly BatchProcessingOptions _options;
        private readonly Timer _batchTimer;
        private readonly SemaphoreSlim _batchSemaphore;
        private readonly ConcurrentQueue<BatchRequest<Transaction, Result<string>>> _transactionQueue;
        private readonly ConcurrentQueue<BatchRequest<string, Result<TransactionStatus>>> _statusQueue;
        private readonly ConcurrentQueue<BatchRequest<string, Result<AccountState>>> _accountStateQueue;
        private bool _disposed;

        // Statistics
        private long _totalBatches;
        private long _totalRequests;
        private long _averageBatchSize;
        private long _totalProcessingTime;

        /// <summary>
        /// Initializes a new instance of the MultiChainBatchProcessor class.
        /// </summary>
        /// <param name="options">Batch processing options.</param>
        /// <param name="logger">Logger instance.</param>
        public MultiChainBatchProcessor(
            IOptions<BatchProcessingOptions> options,
            ILogger<MultiChainBatchProcessor> logger)
        {
            _options = options?.Value ?? new BatchProcessingOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _batchSemaphore = new SemaphoreSlim(_options.MaxConcurrentBatches, _options.MaxConcurrentBatches);
            
            _transactionQueue = new ConcurrentQueue<BatchRequest<Transaction, Result<string>>>();
            _statusQueue = new ConcurrentQueue<BatchRequest<string, Result<TransactionStatus>>>();
            _accountStateQueue = new ConcurrentQueue<BatchRequest<string, Result<AccountState>>>();

            // Set up batch processing timer
            _batchTimer = new Timer(ProcessBatches, null, 
                TimeSpan.FromMilliseconds(_options.BatchTimeoutMilliseconds),
                TimeSpan.FromMilliseconds(_options.BatchTimeoutMilliseconds));

            _logger.LogInformation("MultiChain batch processor initialized with max batch size: {MaxBatchSize}, timeout: {Timeout}ms",
                _options.MaxBatchSize, _options.BatchTimeoutMilliseconds);
        }

        /// <summary>
        /// Queues a transaction for batch processing.
        /// </summary>
        /// <param name="transaction">The transaction to process.</param>
        /// <param name="priority">The priority of the request.</param>
        /// <returns>A task that completes when the transaction is processed.</returns>
        public async Task<Result<string>> QueueTransactionAsync(Transaction transaction, int priority = 0)
        {
            if (!_options.EnableBatchProcessing)
            {
                // If batch processing is disabled, process immediately
                return await ProcessSingleTransactionAsync(transaction);
            }

            var batchRequest = new BatchRequest<Transaction, Result<string>>
            {
                Request = transaction,
                Priority = priority
            };

            _transactionQueue.Enqueue(batchRequest);
            Interlocked.Increment(ref _totalRequests);

            _logger.LogDebug("Transaction queued for batch processing: {TransactionId}", transaction.Id);

            // Trigger immediate processing if batch is full
            if (_transactionQueue.Count >= _options.MaxBatchSize)
            {
                _ = Task.Run(() => ProcessTransactionBatch());
            }

            return await batchRequest.CompletionSource.Task;
        }

        /// <summary>
        /// Queues a transaction status request for batch processing.
        /// </summary>
        /// <param name="transactionId">The transaction ID to check.</param>
        /// <param name="priority">The priority of the request.</param>
        /// <returns>A task that completes when the status is retrieved.</returns>
        public async Task<Result<TransactionStatus>> QueueTransactionStatusAsync(string transactionId, int priority = 0)
        {
            if (!_options.EnableBatchProcessing)
            {
                // If batch processing is disabled, process immediately
                return await ProcessSingleTransactionStatusAsync(transactionId);
            }

            var batchRequest = new BatchRequest<string, Result<TransactionStatus>>
            {
                Request = transactionId,
                Priority = priority
            };

            _statusQueue.Enqueue(batchRequest);
            Interlocked.Increment(ref _totalRequests);

            _logger.LogDebug("Transaction status request queued for batch processing: {TransactionId}", transactionId);

            // Trigger immediate processing if batch is full
            if (_statusQueue.Count >= _options.MaxBatchSize)
            {
                _ = Task.Run(() => ProcessTransactionStatusBatch());
            }

            return await batchRequest.CompletionSource.Task;
        }

        /// <summary>
        /// Queues an account state request for batch processing.
        /// </summary>
        /// <param name="address">The account address to check.</param>
        /// <param name="priority">The priority of the request.</param>
        /// <returns>A task that completes when the account state is retrieved.</returns>
        public async Task<Result<AccountState>> QueueAccountStateAsync(string address, int priority = 0)
        {
            if (!_options.EnableBatchProcessing)
            {
                // If batch processing is disabled, process immediately
                return await ProcessSingleAccountStateAsync(address);
            }

            var batchRequest = new BatchRequest<string, Result<AccountState>>
            {
                Request = address,
                Priority = priority
            };

            _accountStateQueue.Enqueue(batchRequest);
            Interlocked.Increment(ref _totalRequests);

            _logger.LogDebug("Account state request queued for batch processing: {Address}", address);

            // Trigger immediate processing if batch is full
            if (_accountStateQueue.Count >= _options.MaxBatchSize)
            {
                _ = Task.Run(() => ProcessAccountStateBatch());
            }

            return await batchRequest.CompletionSource.Task;
        }

        /// <summary>
        /// Timer callback to process batches periodically.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private async void ProcessBatches(object state)
        {
            try
            {
                var tasks = new List<Task>();

                // Process transaction batches
                if (_transactionQueue.Count >= _options.MinBatchSize)
                {
                    tasks.Add(Task.Run(() => ProcessTransactionBatch()));
                }

                // Process status batches
                if (_statusQueue.Count >= _options.MinBatchSize)
                {
                    tasks.Add(Task.Run(() => ProcessTransactionStatusBatch()));
                }

                // Process account state batches
                if (_accountStateQueue.Count >= _options.MinBatchSize)
                {
                    tasks.Add(Task.Run(() => ProcessAccountStateBatch()));
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch processing");
            }
        }

        /// <summary>
        /// Processes a batch of transactions.
        /// </summary>
        private async Task ProcessTransactionBatch()
        {
            if (!await _batchSemaphore.WaitAsync(100)) // Quick timeout to avoid blocking
                return;

            try
            {
                var batch = DequeueBatch(_transactionQueue, _options.MaxBatchSize);
                if (!batch.Any()) return;

                var startTime = DateTime.UtcNow;
                _logger.LogDebug("Processing transaction batch of size: {BatchSize}", batch.Count);

                // Sort by priority (higher priority first)
                var sortedBatch = batch.OrderByDescending(r => r.Priority).ToList();

                // Process transactions in parallel with controlled concurrency
                var semaphore = new SemaphoreSlim(Math.Min(10, batch.Count));
                var tasks = sortedBatch.Select(async request =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var result = await ProcessSingleTransactionAsync(request.Request);
                        request.CompletionSource.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        request.CompletionSource.SetException(ex);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                UpdateBatchStatistics(batch.Count, processingTime);

                _logger.LogDebug("Transaction batch processed in {ProcessingTime}ms", processingTime);
            }
            finally
            {
                _batchSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes a batch of transaction status requests.
        /// </summary>
        private async Task ProcessTransactionStatusBatch()
        {
            if (!await _batchSemaphore.WaitAsync(100))
                return;

            try
            {
                var batch = DequeueBatch(_statusQueue, _options.MaxBatchSize);
                if (!batch.Any()) return;

                var startTime = DateTime.UtcNow;
                _logger.LogDebug("Processing transaction status batch of size: {BatchSize}", batch.Count);

                // Group by unique transaction IDs to avoid duplicate requests
                var uniqueRequests = batch.GroupBy(r => r.Request)
                    .Select(g => g.OrderByDescending(r => r.Priority).First())
                    .ToList();

                var tasks = uniqueRequests.Select(async request =>
                {
                    try
                    {
                        var result = await ProcessSingleTransactionStatusAsync(request.Request);
                        
                        // Set result for all requests with the same transaction ID
                        foreach (var req in batch.Where(r => r.Request == request.Request))
                        {
                            req.CompletionSource.SetResult(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (var req in batch.Where(r => r.Request == request.Request))
                        {
                            req.CompletionSource.SetException(ex);
                        }
                    }
                });

                await Task.WhenAll(tasks);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                UpdateBatchStatistics(batch.Count, processingTime);

                _logger.LogDebug("Transaction status batch processed in {ProcessingTime}ms", processingTime);
            }
            finally
            {
                _batchSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes a batch of account state requests.
        /// </summary>
        private async Task ProcessAccountStateBatch()
        {
            if (!await _batchSemaphore.WaitAsync(100))
                return;

            try
            {
                var batch = DequeueBatch(_accountStateQueue, _options.MaxBatchSize);
                if (!batch.Any()) return;

                var startTime = DateTime.UtcNow;
                _logger.LogDebug("Processing account state batch of size: {BatchSize}", batch.Count);

                // Group by unique addresses to avoid duplicate requests
                var uniqueRequests = batch.GroupBy(r => r.Request)
                    .Select(g => g.OrderByDescending(r => r.Priority).First())
                    .ToList();

                var tasks = uniqueRequests.Select(async request =>
                {
                    try
                    {
                        var result = await ProcessSingleAccountStateAsync(request.Request);
                        
                        // Set result for all requests with the same address
                        foreach (var req in batch.Where(r => r.Request == request.Request))
                        {
                            req.CompletionSource.SetResult(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (var req in batch.Where(r => r.Request == request.Request))
                        {
                            req.CompletionSource.SetException(ex);
                        }
                    }
                });

                await Task.WhenAll(tasks);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                UpdateBatchStatistics(batch.Count, processingTime);

                _logger.LogDebug("Account state batch processed in {ProcessingTime}ms", processingTime);
            }
            finally
            {
                _batchSemaphore.Release();
            }
        }

        /// <summary>
        /// Dequeues a batch of requests from the specified queue.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="queue">The queue to dequeue from.</param>
        /// <param name="maxSize">The maximum batch size.</param>
        /// <returns>A list of batch requests.</returns>
        private static List<BatchRequest<TRequest, TResponse>> DequeueBatch<TRequest, TResponse>(
            ConcurrentQueue<BatchRequest<TRequest, TResponse>> queue, int maxSize)
        {
            var batch = new List<BatchRequest<TRequest, TResponse>>();
            
            for (int i = 0; i < maxSize && queue.TryDequeue(out var request); i++)
            {
                batch.Add(request);
            }

            return batch;
        }

        /// <summary>
        /// Updates batch processing statistics.
        /// </summary>
        /// <param name="batchSize">The size of the processed batch.</param>
        /// <param name="processingTime">The processing time in milliseconds.</param>
        private void UpdateBatchStatistics(int batchSize, double processingTime)
        {
            Interlocked.Increment(ref _totalBatches);
            Interlocked.Add(ref _totalProcessingTime, (long)processingTime);
            
            // Update average batch size
            var currentAverage = _averageBatchSize;
            var newAverage = (currentAverage * (_totalBatches - 1) + batchSize) / _totalBatches;
            Interlocked.Exchange(ref _averageBatchSize, newAverage);
        }

        /// <summary>
        /// Gets batch processing statistics.
        /// </summary>
        /// <returns>Batch processing statistics.</returns>
        public BatchProcessingStatistics GetStatistics()
        {
            return new BatchProcessingStatistics
            {
                TotalBatches = _totalBatches,
                TotalRequests = _totalRequests,
                AverageBatchSize = _averageBatchSize,
                AverageProcessingTime = _totalBatches > 0 ? (double)_totalProcessingTime / _totalBatches : 0,
                QueuedTransactions = _transactionQueue.Count,
                QueuedStatusRequests = _statusQueue.Count,
                QueuedAccountStateRequests = _accountStateQueue.Count
            };
        }

        // Placeholder methods for actual processing (these would call the real MultiChain provider)
        private async Task<Result<string>> ProcessSingleTransactionAsync(Transaction transaction)
        {
            // This would call the actual MultiChain provider
            await Task.Delay(10); // Simulate processing time
            return Result<string>.Success($"tx_{Guid.NewGuid():N}");
        }

        private async Task<Result<TransactionStatus>> ProcessSingleTransactionStatusAsync(string transactionId)
        {
            // This would call the actual MultiChain provider
            await Task.Delay(5); // Simulate processing time
            return Result<TransactionStatus>.Success(new TransactionStatus
            {
                TransactionId = transactionId,
                Status = TransactionStatuses.Confirmed,
                LastUpdated = DateTime.UtcNow
            });
        }

        private async Task<Result<AccountState>> ProcessSingleAccountStateAsync(string address)
        {
            // This would call the actual MultiChain provider
            await Task.Delay(8); // Simulate processing time
            return Result<AccountState>.Success(new AccountState
            {
                Address = address,
                Balance = 100.0m,
                LastUpdated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Disposes the batch processor.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _batchTimer?.Dispose();
            _batchSemaphore?.Dispose();
            _disposed = true;

            _logger.LogInformation("MultiChain batch processor disposed");
        }
    }

    /// <summary>
    /// Represents batch processing statistics.
    /// </summary>
    public class BatchProcessingStatistics
    {
        /// <summary>
        /// Gets or sets the total number of batches processed.
        /// </summary>
        public long TotalBatches { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests processed.
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Gets or sets the average batch size.
        /// </summary>
        public long AverageBatchSize { get; set; }

        /// <summary>
        /// Gets or sets the average processing time per batch in milliseconds.
        /// </summary>
        public double AverageProcessingTime { get; set; }

        /// <summary>
        /// Gets or sets the number of queued transactions.
        /// </summary>
        public int QueuedTransactions { get; set; }

        /// <summary>
        /// Gets or sets the number of queued status requests.
        /// </summary>
        public int QueuedStatusRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of queued account state requests.
        /// </summary>
        public int QueuedAccountStateRequests { get; set; }
    }
}
