using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Utils;

namespace QuantumLedger.Cryptography.Extensions
{
    public static class KeyGenerationHelperExtensions
    {
        /// <summary>
        /// Generates a new EC-256 key pair and returns them as Base64 encoded strings
        /// </summary>
        /// <returns>A tuple containing the Base64 encoded private and public keys</returns>
        /// <exception cref="KeyOperationException">Thrown when key generation fails</exception>
        public static (string privateKey, string publicKey) GenerateEC256KeyPairAsBase64()
        {
            var (privateKey, publicKey) = KeyGenerationHelper.GenerateEC256KeyPair();
            return (Convert.ToBase64String(privateKey), Convert.ToBase64String(publicKey));
        }

        /// <summary>
        /// Validates that a Base64 encoded private key and public key form a valid key pair
        /// </summary>
        /// <param name="privateKeyBase64">The Base64 encoded private key</param>
        /// <param name="publicKeyBase64">The Base64 encoded public key</param>
        /// <param name="algorithm">The algorithm the keys are for</param>
        /// <returns>True if the keys form a valid pair, false otherwise</returns>
        /// <exception cref="KeyOperationException">Thrown when validation fails due to an error</exception>
        public static bool ValidateKeyPairBase64(string privateKeyBase64, string publicKeyBase64, string algorithm)
        {
            var privateKey = Convert.FromBase64String(privateKeyBase64);
            var publicKey = Convert.FromBase64String(publicKeyBase64);
            return KeyGenerationHelper.ValidateKeyPair(privateKey, publicKey, algorithm);
        }
    }
}
