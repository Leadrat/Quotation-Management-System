using CRM.Application.Common.Services;
using CRM.Infrastructure.Admin.Encryption;

namespace CRM.Api.Adapters;

public class DataEncryptionServiceAdapter : CRM.Application.Common.Services.IDataEncryptionService
{
    private readonly Infrastructure.Admin.Encryption.IDataEncryptionService _infrastructureService;

    public DataEncryptionServiceAdapter(Infrastructure.Admin.Encryption.IDataEncryptionService infrastructureService)
    {
        _infrastructureService = infrastructureService;
    }

    public string Encrypt(string plainText) => _infrastructureService.Encrypt(plainText);
    public string Decrypt(string encryptedData) => _infrastructureService.Decrypt(encryptedData);
}

