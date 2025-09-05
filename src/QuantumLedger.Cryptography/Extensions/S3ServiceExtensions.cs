using Amazon.S3;
using Amazon.S3.Model;

namespace QuantumLedger.Cryptography.Extensions;

/// <summary>
/// Extension methods for IAmazonS3 service
/// </summary>
public static class S3ServiceExtensions
{
    /// <summary>
    /// Gets an object from S3 asynchronously
    /// </summary>
    /// <param name="s3Client">The S3 client</param>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The object data as a stream</returns>
    public static async Task<Stream> GetObjectAsync(
        this IAmazonS3 s3Client,
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        var response = await s3Client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    /// <summary>
    /// Deletes an object from S3 asynchronously
    /// </summary>
    /// <param name="s3Client">The S3 client</param>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task DeleteObjectAsync(
        this IAmazonS3 s3Client,
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        await s3Client.DeleteObjectAsync(request, cancellationToken);
    }
}
