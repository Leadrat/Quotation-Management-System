using System;
using System.Security.Cryptography;
using System.Text;
using CRM.Application.Common.Security;

namespace CRM.Infrastructure.Security
{
    public class ResetTokenGenerator : IResetTokenGenerator
    {
        private static readonly byte[] HmacKey = RandomNumberGenerator.GetBytes(32);

        public (string token, byte[] hash) Generate()
        {
            // 32-byte random token, base64url string for transport
            var raw = RandomNumberGenerator.GetBytes(32);
            var token = Base64UrlEncode(raw);
            using var hmac = new HMACSHA256(HmacKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return (token, hash);
        }

        private static string Base64UrlEncode(byte[] buffer)
        {
            return Convert.ToBase64String(buffer)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
