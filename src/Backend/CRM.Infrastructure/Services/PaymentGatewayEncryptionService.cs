using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Services
{
    /// <summary>
    /// Simple encryption service for payment gateway credentials
    /// TODO: Replace with proper key management (Azure Key Vault, AWS KMS, etc.) in production
    /// </summary>
    public class PaymentGatewayEncryptionService : IPaymentGatewayEncryptionService
    {
        private readonly string _encryptionKey;

        public PaymentGatewayEncryptionService(IConfiguration configuration)
        {
            _encryptionKey = configuration["PaymentGateway:EncryptionKey"] 
                ?? throw new InvalidOperationException("PaymentGateway:EncryptionKey not configured");
            
            if (_encryptionKey.Length < 32)
                throw new InvalidOperationException("Encryption key must be at least 32 characters");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
            aes.IV = new byte[16]; // In production, generate a random IV and store it

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.Substring(0, 32));
            aes.IV = new byte[16]; // In production, retrieve the stored IV

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}

