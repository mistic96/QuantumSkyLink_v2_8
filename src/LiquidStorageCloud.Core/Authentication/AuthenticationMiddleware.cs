using System.Text;
using LiquidStorageCloud.Core.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace LiquidStorageCloud.Core.Authentication
{
    public sealed class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api/client"))
            {
                var nonce = context.Request.Headers["X-Nonce"].FirstOrDefault();
                var timestamp = context.Request.Headers["X-Timestamp"].FirstOrDefault();
                var signature = context.Request.Headers["X-Signature"].FirstOrDefault();
                var appid = context.Request.Headers["X-App-Id"].FirstOrDefault();

                if (string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(appid))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Authentication failed: Missing required headers");
                    return;
                }

                var clientPublicKey = _configuration["ClientPublicKey"];
                if (string.IsNullOrEmpty(clientPublicKey))
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Server configuration error: Client public key not found");
                    return;
                }

                var data = Encoding.UTF8.GetBytes($"{nonce}{timestamp}");
                var signatureBytes = Convert.FromBase64String(signature);
                var publicKeyBytes = Convert.FromBase64String(clientPublicKey);

                try
                {
                    if (!VerifySignature(publicKeyBytes, data, signatureBytes))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Authentication failed: Invalid signature");
                        return;
                    }

                    if (!VerifyTimestamp(timestamp))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Authentication failed: Invalid timestamp");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during authentication");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An error occurred during authentication");
                    return;
                }
            }

            await _next(context);
        }

        private bool VerifySignature(byte[] publicKey, byte[] data, byte[] signature)
        {
            var (r, s) = DecodeSignature(signature);
            return ECCService.Verify(publicKey, data, r, s);
        }

        private (BigInteger r, BigInteger s) DecodeSignature(byte[] signature)
        {
            var sequence = (DerSequence)Asn1Object.FromByteArray(signature);
            var r = ((DerInteger)sequence[0]).Value;
            var s = ((DerInteger)sequence[1]).Value;
            return (r, s);
        }

        private bool VerifyTimestamp(string timestamp)
        {
            if (!long.TryParse(timestamp, out long timestampValue))
            {
                return false;
            }

            var requestTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampValue);
            var currentTime = DateTimeOffset.UtcNow;
            var timeDifference = currentTime - requestTime;

            // Allow requests within the last 5 minutes
            return timeDifference >= TimeSpan.Zero && timeDifference <= TimeSpan.FromMinutes(5);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
