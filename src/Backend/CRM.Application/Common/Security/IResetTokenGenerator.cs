using System;

namespace CRM.Application.Common.Security
{
    public interface IResetTokenGenerator
    {
        // Returns (plainToken, tokenHashBytes)
        (string token, byte[] hash) Generate();
    }
}
