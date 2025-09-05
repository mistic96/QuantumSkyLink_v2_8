using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumLedger.Blockchain.Services.MultiChain
{
    /// <summary>
    /// Configuration options for serialization optimization.
    /// </summary>
    public class SerializationOptions
    {
        /// <summary>
        /// Gets or sets whether to enable serialization optimization.
        /// </summary>
        public bool EnableOptimization { get; set; } = true;

        /// <summary>
        /// Gets or sets the buffer pool size for serialization.
        /// </summary>
        public int BufferPoolSize { get; set; } = 1024 * 1024; // 1MB

        /// <summary>
        /// Gets or sets whether to use memory pooling for large objects.
        /// </summary>
        public bool UseMemoryPooling { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum cached serializers count.
        /// </summary>
        public int MaxCachedSerializers { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether to enable compression for large payloads.
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum size in bytes to trigger compression.
        /// </summary>
        public int CompressionThreshold { get; set; } = 1024; // 1KB
    }

    /// <summary>
    /// Optimized JSON serialization service for MultiChain operations.
    /// </summary>
    public class MultiChainSerializationOptimizer : IDisposable
    {
        private readonly ILogger<MultiChainSerializationOptimizer> _logger;
        private readonly SerializationOptions _options;
        private readonly ArrayPool<byte> _byteArrayPool;
        private readonly ConcurrentDictionary<Type, JsonSerializerOptions> _serializerCache;
        private readonly Timer _cleanupTimer;
        private bool _disposed;

        // Performance statistics
        private long _totalSerializations;
        private long _totalDeserializations;
        private long _totalBytesProcessed;
        private long _compressionSavings;

        /// <summary>
        /// Gets the optimized JSON serializer options for MultiChain operations.
        /// </summary>
        public static JsonSerializerOptions OptimizedOptions { get; } = CreateOptimizedOptions();

        /// <summary>
        /// Initializes a new instance of the MultiChainSerializationOptimizer class.
        /// </summary>
        /// <param name="options">Serialization options.</param>
        /// <param name="logger">Logger instance.</param>
        public MultiChainSerializationOptimizer(
            IOptions<SerializationOptions> options,
            ILogger<MultiChainSerializationOptimizer> logger)
        {
            _options = options?.Value ?? new SerializationOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _byteArrayPool = ArrayPool<byte>.Create(_options.BufferPoolSize, 50);
            _serializerCache = new ConcurrentDictionary<Type, JsonSerializerOptions>();

            // Set up cleanup timer to run every 5 minutes
            _cleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

            _logger.LogInformation("MultiChain serialization optimizer initialized with buffer pool size: {BufferPoolSize} bytes",
                _options.BufferPoolSize);
        }

        /// <summary>
        /// Serializes an object to JSON with optimization.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serialized JSON string.</returns>
        public string SerializeOptimized<T>(T value)
        {
            if (!_options.EnableOptimization)
            {
                return JsonSerializer.Serialize(value, OptimizedOptions);
            }

            try
            {
                var serializerOptions = GetCachedSerializerOptions<T>();
                
                if (_options.UseMemoryPooling)
                {
                    return SerializeWithPooling(value, serializerOptions);
                }
                else
                {
                    var result = JsonSerializer.Serialize(value, serializerOptions);
                    UpdateSerializationStats(result.Length, false);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during optimized serialization for type {Type}", typeof(T).Name);
                // Fallback to standard serialization
                return JsonSerializer.Serialize(value, OptimizedOptions);
            }
        }

        /// <summary>
        /// Deserializes JSON to an object with optimization.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T DeserializeOptimized<T>(string json)
        {
            if (!_options.EnableOptimization)
            {
                return JsonSerializer.Deserialize<T>(json, OptimizedOptions);
            }

            try
            {
                var serializerOptions = GetCachedSerializerOptions<T>();
                
                if (_options.UseMemoryPooling)
                {
                    return DeserializeWithPooling<T>(json, serializerOptions);
                }
                else
                {
                    var result = JsonSerializer.Deserialize<T>(json, serializerOptions);
                    UpdateDeserializationStats(json.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during optimized deserialization for type {Type}", typeof(T).Name);
                // Fallback to standard deserialization
                return JsonSerializer.Deserialize<T>(json, OptimizedOptions);
            }
        }

        /// <summary>
        /// Serializes an object to a byte array with optimization and optional compression.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serialized byte array.</returns>
        public byte[] SerializeToBytes<T>(T value)
        {
            try
            {
                var serializerOptions = GetCachedSerializerOptions<T>();
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(value, serializerOptions);

                if (_options.EnableCompression && jsonBytes.Length > _options.CompressionThreshold)
                {
                    var compressedBytes = CompressBytes(jsonBytes);
                    var savings = jsonBytes.Length - compressedBytes.Length;
                    Interlocked.Add(ref _compressionSavings, savings);
                    
                    _logger.LogDebug("Compressed serialization: {OriginalSize} -> {CompressedSize} bytes ({Savings} bytes saved)",
                        jsonBytes.Length, compressedBytes.Length, savings);
                    
                    UpdateSerializationStats(jsonBytes.Length, true);
                    return compressedBytes;
                }

                UpdateSerializationStats(jsonBytes.Length, false);
                return jsonBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during byte serialization for type {Type}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Deserializes a byte array to an object with optimization and decompression.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array to deserialize.</param>
        /// <param name="isCompressed">Whether the byte array is compressed.</param>
        /// <returns>The deserialized object.</returns>
        public T DeserializeFromBytes<T>(byte[] bytes, bool isCompressed = false)
        {
            try
            {
                var serializerOptions = GetCachedSerializerOptions<T>();
                byte[] jsonBytes = bytes;

                if (isCompressed)
                {
                    jsonBytes = DecompressBytes(bytes);
                    _logger.LogDebug("Decompressed data: {CompressedSize} -> {DecompressedSize} bytes",
                        bytes.Length, jsonBytes.Length);
                }

                var result = JsonSerializer.Deserialize<T>(jsonBytes, serializerOptions);
                UpdateDeserializationStats(jsonBytes.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during byte deserialization for type {Type}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Serializes using memory pooling for better performance.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Serializer options.</param>
        /// <returns>The serialized JSON string.</returns>
        private string SerializeWithPooling<T>(T value, JsonSerializerOptions options)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = false,
                SkipValidation = true // Skip validation for better performance
            });

            JsonSerializer.Serialize(writer, value, options);
            writer.Flush();

            var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            UpdateSerializationStats(result.Length, false);
            return result;
        }

        /// <summary>
        /// Deserializes using memory pooling for better performance.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="options">Serializer options.</param>
        /// <returns>The deserialized object.</returns>
        private T DeserializeWithPooling<T>(string json, JsonSerializerOptions options)
        {
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(jsonBytes, new JsonReaderOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            var result = JsonSerializer.Deserialize<T>(ref reader, options);
            UpdateDeserializationStats(json.Length);
            return result;
        }

        /// <summary>
        /// Gets cached serializer options for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get options for.</typeparam>
        /// <returns>Cached or new serializer options.</returns>
        private JsonSerializerOptions GetCachedSerializerOptions<T>()
        {
            var type = typeof(T);
            
            if (_serializerCache.TryGetValue(type, out var cachedOptions))
            {
                return cachedOptions;
            }

            // Create type-specific optimized options
            var options = CreateTypeSpecificOptions<T>();
            
            // Cache if we haven't exceeded the limit
            if (_serializerCache.Count < _options.MaxCachedSerializers)
            {
                _serializerCache.TryAdd(type, options);
            }

            return options;
        }

        /// <summary>
        /// Creates optimized JSON serializer options.
        /// </summary>
        /// <returns>Optimized JSON serializer options.</returns>
        private static JsonSerializerOptions CreateOptimizedOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true,
                // Performance optimizations
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
                ReferenceHandler = null // Disable reference handling for better performance
            };
        }

        /// <summary>
        /// Creates type-specific optimized options.
        /// </summary>
        /// <typeparam name="T">The type to create options for.</typeparam>
        /// <returns>Type-specific serializer options.</returns>
        private static JsonSerializerOptions CreateTypeSpecificOptions<T>()
        {
            var options = new JsonSerializerOptions(OptimizedOptions);
            
            // Add type-specific optimizations
            var type = typeof(T);
            
            if (type.Name.Contains("MultiChain"))
            {
                // MultiChain-specific optimizations
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.WriteIndented = false;
            }
            
            return options;
        }

        /// <summary>
        /// Compresses a byte array using a simple compression algorithm.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>The compressed data.</returns>
        private static byte[] CompressBytes(byte[] data)
        {
            // For this example, we'll use a simple compression simulation
            // In a real implementation, you would use System.IO.Compression.GZipStream
            // or another compression library
            
            using var output = new MemoryStream();
            using var compressor = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress);
            compressor.Write(data, 0, data.Length);
            compressor.Close();
            return output.ToArray();
        }

        /// <summary>
        /// Decompresses a byte array.
        /// </summary>
        /// <param name="compressedData">The compressed data.</param>
        /// <returns>The decompressed data.</returns>
        private static byte[] DecompressBytes(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var decompressor = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
            using var output = new MemoryStream();
            decompressor.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// Updates serialization statistics.
        /// </summary>
        /// <param name="bytes">Number of bytes processed.</param>
        /// <param name="wasCompressed">Whether compression was used.</param>
        private void UpdateSerializationStats(int bytes, bool wasCompressed)
        {
            Interlocked.Increment(ref _totalSerializations);
            Interlocked.Add(ref _totalBytesProcessed, bytes);
        }

        /// <summary>
        /// Updates deserialization statistics.
        /// </summary>
        /// <param name="bytes">Number of bytes processed.</param>
        private void UpdateDeserializationStats(int bytes)
        {
            Interlocked.Increment(ref _totalDeserializations);
            Interlocked.Add(ref _totalBytesProcessed, bytes);
        }

        /// <summary>
        /// Gets serialization performance statistics.
        /// </summary>
        /// <returns>Serialization statistics.</returns>
        public SerializationStatistics GetStatistics()
        {
            return new SerializationStatistics
            {
                TotalSerializations = _totalSerializations,
                TotalDeserializations = _totalDeserializations,
                TotalBytesProcessed = _totalBytesProcessed,
                CompressionSavings = _compressionSavings,
                CachedSerializers = _serializerCache.Count,
                CompressionRatio = _totalBytesProcessed > 0 ? (double)_compressionSavings / _totalBytesProcessed * 100 : 0
            };
        }

        /// <summary>
        /// Cleanup timer callback to manage cache size.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private void CleanupCache(object state)
        {
            try
            {
                var stats = GetStatistics();
                _logger.LogDebug("Serialization stats - Serializations: {Serializations}, Deserializations: {Deserializations}, " +
                    "Bytes: {Bytes}, Compression savings: {CompressionSavings} bytes ({CompressionRatio:F2}%)",
                    stats.TotalSerializations, stats.TotalDeserializations, stats.TotalBytesProcessed,
                    stats.CompressionSavings, stats.CompressionRatio);

                // Clear cache if it gets too large
                if (_serializerCache.Count > _options.MaxCachedSerializers * 1.5)
                {
                    _serializerCache.Clear();
                    _logger.LogInformation("Serializer cache cleared due to size limit");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during serialization cache cleanup");
            }
        }

        /// <summary>
        /// Disposes the serialization optimizer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cleanupTimer?.Dispose();
            _disposed = true;

            _logger.LogInformation("MultiChain serialization optimizer disposed");
        }
    }

    /// <summary>
    /// Represents serialization performance statistics.
    /// </summary>
    public class SerializationStatistics
    {
        /// <summary>
        /// Gets or sets the total number of serializations performed.
        /// </summary>
        public long TotalSerializations { get; set; }

        /// <summary>
        /// Gets or sets the total number of deserializations performed.
        /// </summary>
        public long TotalDeserializations { get; set; }

        /// <summary>
        /// Gets or sets the total number of bytes processed.
        /// </summary>
        public long TotalBytesProcessed { get; set; }

        /// <summary>
        /// Gets or sets the total bytes saved through compression.
        /// </summary>
        public long CompressionSavings { get; set; }

        /// <summary>
        /// Gets or sets the number of cached serializers.
        /// </summary>
        public int CachedSerializers { get; set; }

        /// <summary>
        /// Gets or sets the compression ratio as a percentage.
        /// </summary>
        public double CompressionRatio { get; set; }
    }
}
