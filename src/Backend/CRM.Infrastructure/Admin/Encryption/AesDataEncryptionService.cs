using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Admin.Encryption;

/// <summary>
/// Implementation of IDataEncryptionService using AES-256-GCM encryption
/// </summary>
public class AesDataEncryptionService : IDataEncryptionService
{
    private readonly byte[] _encryptionKey;

    public AesDataEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption:Key configuration is missing");
        
        // Key should be 32 bytes (256 bits) for AES-256
        _encryptionKey = Convert.FromBase64String(keyString);
        
        if (_encryptionKey.Length != 32)
        {
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits)");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        // Generate random 12-byte nonce (IV) for GCM
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        // Encrypt using AES-256-GCM
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var ciphertext = new byte[plainBytes.Length];
        var tag = new byte[16]; // 128-bit authentication tag for GCM

        using (var aesGcm = new AesGcm(_encryptionKey))
        {
            aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);
        }

        // Combine nonce + tag + ciphertext and encode as base64
        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            
            // Extract nonce (12 bytes), tag (16 bytes), and ciphertext
            if (encryptedBytes.Length < 28) // 12 + 16 minimum
                throw new CryptographicException("Invalid encrypted data format");

            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertext = new byte[encryptedBytes.Length - 28];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, 12);
            Buffer.BlockCopy(encryptedBytes, 12, tag, 0, 16);
            Buffer.BlockCopy(encryptedBytes, 28, ciphertext, 0, ciphertext.Length);

            // Decrypt using AES-256-GCM
            var plainBytes = new byte[ciphertext.Length];

            using (var aesGcm = new AesGcm(_encryptionKey))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }
}

