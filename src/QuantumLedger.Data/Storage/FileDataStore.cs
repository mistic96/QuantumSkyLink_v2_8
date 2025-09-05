using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QuantumLedger.Data.Storage
{
    /// <summary>
    /// File-based implementation of the immutable data store.
    /// </summary>
    public class FileDataStore : IDataStore, IDisposable
    {
        private readonly string _baseDirectory;
        private readonly string _dataDirectory;
        private readonly string _indexDirectory;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        private readonly ConcurrentDictionary<string, long> _index;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the FileDataStore.
        /// </summary>
        /// <param name="baseDirectory">The base directory for storing data and index files.</param>
        public FileDataStore(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            _dataDirectory = Path.Combine(_baseDirectory, "data");
            _indexDirectory = Path.Combine(_baseDirectory, "index");
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _index = new ConcurrentDictionary<string, long>();

            InitializeStore();
        }

        private void InitializeStore()
        {
            // Create directories if they don't exist
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_indexDirectory);

            // Load existing index
            LoadIndex();
        }

        private void LoadIndex()
        {
            foreach (var indexFile in Directory.GetFiles(_indexDirectory, "*.idx"))
            {
                var key = Path.GetFileNameWithoutExtension(indexFile);
                if (long.TryParse(File.ReadAllText(indexFile), out var position))
                {
                    _index.TryAdd(key, position);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AppendAsync(string key, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                var dataFile = Path.Combine(_dataDirectory, "data.log");
                var indexFile = Path.Combine(_indexDirectory, $"{key}.idx");

                // Append data atomically
                using (var stream = new FileStream(dataFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    // Write length prefix and data
                    var length = BitConverter.GetBytes(data.Length);
                    await stream.WriteAsync(length);
                    await stream.WriteAsync(data);
                    await stream.FlushAsync();

                    // Update index
                    var position = stream.Position - data.Length - length.Length;
                    _index[key] = position;
                    await File.WriteAllTextAsync(indexFile, position.ToString());

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error appending data for key {key}: {ex.Message}");
                return false;
            }
            finally
            {
                lockObj.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> RetrieveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

            if (!_index.TryGetValue(key, out var position))
                return null;

            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                var dataFile = Path.Combine(_dataDirectory, "data.log");
                using var stream = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Position = position;

                // Read length prefix
                var lengthBuffer = new byte[sizeof(int)];
                await stream.ReadAsync(lengthBuffer);
                var length = BitConverter.ToInt32(lengthBuffer);

                // Read data
                var data = new byte[length];
                await stream.ReadAsync(data);
                return data;
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error retrieving data for key {key}: {ex.Message}");
                return null;
            }
            finally
            {
                lockObj.Release();
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListKeysAsync(string prefix)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));

            return Task.FromResult<IEnumerable<string>>(
                _index.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            );
        }

        /// <summary>
        /// Disposes the FileDataStore instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var lockObj in _locks.Values)
            {
                lockObj.Dispose();
            }

            _locks.Clear();
            _index.Clear();
            _disposed = true;
        }
    }
}
