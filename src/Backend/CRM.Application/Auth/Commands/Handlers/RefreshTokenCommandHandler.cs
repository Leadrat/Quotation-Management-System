using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands.Results;
using CRM.Application.Auth.Services;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Auth.Commands.Handlers;

public class RefreshTokenCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public RefreshTokenCommandHandler(IAppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand cmd, string? cookieToken)
    {
        var token = !string.IsNullOrWhiteSpace(cookieToken) ? cookieToken : cmd.RefreshToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidTokenException();
        }
        var principal = _jwt.ValidateToken(token!, validateLifetime: true);
        if (principal == null)
        {
            throw new InvalidTokenException();
        }
        var userId = _jwt.GetUserIdFromToken(principal);
        var jti = _jwt.GetJtiFromToken(principal);
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(jti))
        {
            throw new InvalidTokenException();
        }

        var now = DateTime.UtcNow;
        var dbToken = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenJti == jti && r.UserId == userId);
        if (dbToken == null || dbToken.IsRevoked || dbToken.ExpiresAt <= now)
        {
            throw new TokenRevokedException();
        }

        // rotate
        dbToken.IsRevoked = true;
        dbToken.RevokedAt = now;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.DeletedAt == null);
        if (user == null || !user.IsActive)
        {
            throw new UserNotActiveException();
        }

        var (newRefreshToken, newJti, newExp) = _jwt.GenerateRefreshToken(userId, now);
        var accessToken = _jwt.GenerateAccessToken(user, now, newJti);

        var newRt = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = userId,
            TokenJti = newJti,
            IsRevoked = false,
            ExpiresAt = newExp,
            CreatedAt = now
        };
        _db.RefreshTokens.Add(newRt);
        await _db.SaveChangesAsync();

        return new RefreshTokenResult
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600
        };
    }
}
