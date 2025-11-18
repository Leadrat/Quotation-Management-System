using System;
using System.Security.Claims;
using CRM.Domain.Entities;

namespace CRM.Application.Auth.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, DateTime nowUtc, string? refreshJti = null);
    (string token, string jti, DateTime expiresAtUtc) GenerateRefreshToken(Guid userId, DateTime nowUtc);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    Guid GetUserIdFromToken(ClaimsPrincipal principal);
    string? GetJtiFromToken(ClaimsPrincipal principal);
}
