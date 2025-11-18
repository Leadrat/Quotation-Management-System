using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Application.Auth.Services;
using CRM.Domain.Entities;
using CRM.Infrastructure.Auth;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private static AppDbContext MakeDb()
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkInMemoryDatabase();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<AppDbContext>();
    }

    private static IJwtTokenGenerator MakeJwt()
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Jwt:Secret"] = new string('x', 32),
                ["Jwt:Issuer"] = "crm.system",
                ["Jwt:Audience"] = "crm.api",
                ["Jwt:AccessTokenExpiration"] = "3600",
                ["Jwt:RefreshTokenExpiration"] = "2592000"
            })
            .Build();
        return new JwtTokenGenerator(cfg, NullLogger<JwtTokenGenerator>.Instance);
    }

    [Fact]
    public async Task Refresh_Succeeds_And_Rotates_Token()
    {
        using var db = MakeDb();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@crm.com",
            PasswordHash = "x",
            FirstName = "U",
            LastName = "Ser",
            IsActive = true,
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var jwt = MakeJwt();
        var now = DateTime.UtcNow;
        var (token, jti, exp) = jwt.GenerateRefreshToken(user.UserId, now);
        db.RefreshTokens.Add(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenJti = jti,
            IsRevoked = false,
            ExpiresAt = exp,
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(db, jwt);
        var result = await handler.Handle(new RefreshTokenCommand { RefreshToken = token }, null);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        // old token should be revoked
        var old = await db.RefreshTokens.FirstAsync(rt => rt.TokenJti == jti);
        Assert.True(old.IsRevoked);
    }

    [Fact]
    public async Task Refresh_Fails_For_Revoked_Token()
    {
        using var db = MakeDb();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@crm.com",
            PasswordHash = "x",
            FirstName = "U",
            LastName = "Ser",
            IsActive = true,
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var jwt = MakeJwt();
        var now = DateTime.UtcNow;
        var (token, jti, exp) = jwt.GenerateRefreshToken(user.UserId, now);
        db.RefreshTokens.Add(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenJti = jti,
            IsRevoked = true,
            RevokedAt = now,
            ExpiresAt = exp,
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(db, jwt);
        await Assert.ThrowsAsync<TokenRevokedException>(() => handler.Handle(new RefreshTokenCommand { RefreshToken = token }, null));
    }
}
