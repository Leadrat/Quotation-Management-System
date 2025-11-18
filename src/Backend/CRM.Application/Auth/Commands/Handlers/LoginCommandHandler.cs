using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands.Results;
using CRM.Application.Auth.Services;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using CRM.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Auth.Commands.Handlers;

public class LoginCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public LoginCommandHandler(IAppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResult> Handle(LoginCommand cmd)
    {
        var email = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(cmd.Password))
        {
            throw new InvalidCredentialsException();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }
        if (!user.IsActive)
        {
            throw new UserNotActiveException();
        }

        var ok = PasswordHelper.VerifyPassword(cmd.Password, user.PasswordHash);
        if (!ok)
        {
            user.LoginAttempts += 1; // monitor only
            await _db.SaveChangesAsync();
            throw new InvalidCredentialsException();
        }

        user.LoginAttempts = 0;
        user.IsLockedOut = false; // unused per Spec-003
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var (refreshToken, jti, refreshExp) = _jwt.GenerateRefreshToken(user.UserId, now);
        var accessToken = _jwt.GenerateAccessToken(user, now, jti);

        var rt = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenJti = jti,
            IsRevoked = false,
            ExpiresAt = refreshExp,
            CreatedAt = now
        };
        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return new LoginResult
        {
            Success = true,
            Message = "Login successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            User = new { user.UserId, user.Email, user.FirstName, user.LastName },
            Timestamp = now
        };
    }
}
