using System;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Auth;

public interface ITokenBlacklistService
{
    Task AddAsync(string jti, DateTime expiresAtUtc);
    Task<bool> IsBlacklistedAsync(string jti);
}
