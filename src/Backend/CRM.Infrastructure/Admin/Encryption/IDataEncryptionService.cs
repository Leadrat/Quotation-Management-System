namespace CRM.Infrastructure.Admin.Encryption;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES-256-GCM
/// </summary>
public interface IDataEncryptionService
{
    /// <summary>
    /// Encrypts plain text data
    /// </summary>
    /// <param name="plainText">Plain text to encrypt</param>
    /// <returns>Base64-encoded encrypted data (includes IV and authentication tag)</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts encrypted data
    /// </summary>
    /// <param name="encryptedData">Base64-encoded encrypted data</param>
    /// <returns>Decrypted plain text</returns>
    string Decrypt(string encryptedData);
}

