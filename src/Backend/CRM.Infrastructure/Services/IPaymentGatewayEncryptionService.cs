namespace CRM.Infrastructure.Services
{
    /// <summary>
    /// Service for encrypting/decrypting payment gateway API keys and secrets
    /// </summary>
    public interface IPaymentGatewayEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}

