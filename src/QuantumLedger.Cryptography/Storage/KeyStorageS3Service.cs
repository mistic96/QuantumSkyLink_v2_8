using System.Text;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Extensions;

namespace QuantumLedger.Cryptography.Storage
{
    /// <summary>
    /// Specialized S3 service for secure key storage
    /// </summary>
    public class KeyStorageS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<KeyStorageS3Service> _logger;
        private readonly string _kmsKeyId;
        private readonly string _bucketName;
        private readonly string _bucketPrefix;

        public KeyStorageS3Service(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<KeyStorageS3Service> logger)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _kmsKeyId = configuration["AWS:KMS:MasterKeyId"] 
                ?? throw new InvalidOperationException("AWS:KMS:MasterKeyId configuration is required");
            _bucketName = configuration["AWS:S3:BucketName"]
                ?? throw new InvalidOperationException("AWS:S3:BucketName configuration is required");
            _bucketPrefix = configuration["AWS:S3:KeyStoragePrefix"] ?? "keys/";
        }

        /// <summary>
        /// Stores encrypted key data in S3
        /// </summary>
        public async Task<string> StoreKeyAsync(string identifier, byte[] encryptedKeyData)
        {
            try
            {
                var key = $"{_bucketPrefix}{identifier}";
                
                using var stream = new MemoryStream(encryptedKeyData);
                var putRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ServerSideEncryptionMethod = Amazon.S3.ServerSideEncryptionMethod.AWSKMS,
                    ServerSideEncryptionKeyManagementServiceKeyId = _kmsKeyId
                };

                await _s3Client.PutObjectAsync(putRequest);
                _logger.LogInformation("Successfully stored key {Identifier} in S3", identifier);
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key {Identifier} in S3", identifier);
                throw;
            }
        }

        /// <summary>
        /// Retrieves encrypted key data from S3
        /// </summary>
        public async Task<byte[]> RetrieveKeyAsync(string key)
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(_bucketName, key);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key from S3: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// Deletes a key from S3
        /// </summary>
        public async Task DeleteKeyAsync(string key)
        {
            try
            {
                await _s3Client.DeleteObjectAsync(_bucketName, key);
                _logger.LogInformation("Successfully deleted key from S3: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key from S3: {Key}", key);
                throw;
            }
        }
    }
}
