using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuantumLedger.Data.Storage
{
    /// <summary>
    /// Provides serialization and deserialization functionality for the data store.
    /// </summary>
    public static class SerializationHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes an object to a byte array using JSON serialization.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A byte array containing the serialized data.</returns>
        public static byte[] SerializeToBytes<T>(T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserializes a byte array to an object using JSON deserialization.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="data">The byte array containing the serialized data.</param>
        /// <returns>The deserialized object, or null if deserialization fails.</returns>
        public static T DeserializeFromBytes<T>(byte[] data) where T : class
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                var json = Encoding.UTF8.GetString(data);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error deserializing data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Asynchronously serializes an object to a byte array.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the serialized data.</returns>
        public static Task<byte[]> SerializeToBytesAsync<T>(T obj) where T : class
        {
            // While serialization itself is synchronous, wrap it in a Task for API consistency
            return Task.FromResult(SerializeToBytes(obj));
        }

        /// <summary>
        /// Asynchronously deserializes a byte array to an object.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="data">The byte array containing the serialized data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object.</returns>
        public static Task<T> DeserializeFromBytesAsync<T>(byte[] data) where T : class
        {
            // While deserialization itself is synchronous, wrap it in a Task for API consistency
            return Task.FromResult(DeserializeFromBytes<T>(data));
        }

        /// <summary>
        /// Generates a storage key for an entity with the given ID.
        /// </summary>
        /// <param name="entityType">The type of entity.</param>
        /// <param name="id">The entity's identifier.</param>
        /// <returns>A storage key in the format "entityType:id".</returns>
        public static string GenerateKey(string entityType, string id)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("Entity type cannot be null or whitespace", nameof(entityType));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));

            return $"{entityType.ToLowerInvariant()}:{id}";
        }
    }
}
