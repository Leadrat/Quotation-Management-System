using System;
using System.Security.Cryptography;

namespace CRM.Application.Quotations.Services
{
    public static class AccessTokenGenerator
    {
        /// <summary>
        /// Generates a cryptographically secure random token for quotation access links.
        /// Token is 32 bytes (256 bits) encoded as Base64URL, resulting in 44 characters.
        /// </summary>
        public static string Generate()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            
            // Convert to Base64URL (URL-safe Base64)
            var base64 = Convert.ToBase64String(bytes);
            return base64
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        /// <summary>
        /// Generates a token with custom length (in bytes).
        /// </summary>
        public static string Generate(int byteLength)
        {
            if (byteLength < 16)
                throw new ArgumentException("Token length must be at least 16 bytes for security", nameof(byteLength));

            var bytes = new byte[byteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            
            var base64 = Convert.ToBase64String(bytes);
            return base64
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
