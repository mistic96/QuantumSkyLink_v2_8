using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using BigInteger = Org.BouncyCastle.Math.BigInteger;

namespace LiquidStorageCloud.Core.Cryptography
{
    public sealed class ECCService
    {
        private const string CURVE = "secp256k1";
        private static readonly X9ECParameters curve = ECNamedCurveTable.GetByName(CURVE);
        private static readonly ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

        public static (byte[] privateKey, byte[] publicKey) GenerateKeyPair()
        {
            var keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(new ECKeyGenerationParameters(domain, new SecureRandom()));
            var keyPair = keyPairGenerator.GenerateKeyPair();

            var privateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
            var publicKey = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(true);

            return (privateKey, publicKey);
        }

        public static bool Verify(byte[] publicKey, byte[] data, BigInteger r, BigInteger s)
        {
            var signer = new ECDsaSigner();
            var pubKey = curve.Curve.DecodePoint(publicKey);
            var pubKeyParams = new ECPublicKeyParameters(pubKey, domain);
            signer.Init(false, pubKeyParams);

            var hash = SHA256(data);
            return signer.VerifySignature(hash, r, s);
        }

        public static byte[] SHA256(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(data);
        }

        public static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public static byte[] HexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static ECDomainParameters GetDomain()
        {
            return domain;
        }
    }
}
