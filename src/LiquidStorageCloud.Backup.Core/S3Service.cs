using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LiquidStorageCloud.Backup.Core;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task UploadFileAsync(string filePath, string key)
    {
        try
        {
            var bucketName = _configuration["AWS:BucketName"];
            var request = new PutObjectRequest
            {
                FilePath = filePath,
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("Successfully uploaded {Key} to S3", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading {Key} to S3", key);
            throw;
        }
    }
}
