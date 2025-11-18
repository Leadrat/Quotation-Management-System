using System;
using System.Threading.Tasks;

namespace CRM.Application.Auth.Services
{
    public interface IRefreshTokenRevoker
    {
        Task RevokeAllForUserAsync(Guid userId);
    }
}
