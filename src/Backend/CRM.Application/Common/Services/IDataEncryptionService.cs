namespace CRM.Application.Common.Services;

public interface IDataEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}

