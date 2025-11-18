using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Application.Auth.Services;
using CRM.Domain.Entities;
using CRM.Infrastructure.Auth;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using CRM.Shared.Exceptions;
using CRM.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CRM.Tests.Auth;

public class LoginCommandHandlerTests
{
    private static AppDbContext MakeDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
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
    public async Task Login_Succeeds_For_Valid_Credentials()
    {
        using var db = MakeDb();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "sales@crm.com",
            PasswordHash = PasswordHelper.HashPassword("Sales@123"),
            FirstName = "Priya",
            LastName = "Singh",
            IsActive = true,
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, MakeJwt());
        var res = await handler.Handle(new LoginCommand { Email = "sales@crm.com", Password = "Sales@123" });

        Assert.True(res.Success);
        Assert.False(string.IsNullOrWhiteSpace(res.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(res.RefreshToken));
        Assert.Equal(3600, res.ExpiresIn);
    }

    [Fact]
    public async Task Login_Fails_For_Wrong_Password()
    {
        using var db = MakeDb();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "sales@crm.com",
            PasswordHash = PasswordHelper.HashPassword("Sales@123"),
            FirstName = "Priya",
            LastName = "Singh",
            IsActive = true,
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, MakeJwt());
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => handler.Handle(new LoginCommand { Email = "sales@crm.com", Password = "Wrong@123" }));

        var reloaded = await db.Users.FirstAsync();
        Assert.Equal(1, reloaded.LoginAttempts);
    }

    [Fact]
    public async Task Login_Fails_For_Inactive_User()
    {
        using var db = MakeDb();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "inactive@crm.com",
            PasswordHash = PasswordHelper.HashPassword("User@123"),
            FirstName = "Ina",
            LastName = "Ctive",
            IsActive = false,
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, MakeJwt());
        await Assert.ThrowsAsync<UserNotActiveException>(() => handler.Handle(new LoginCommand { Email = "inactive@crm.com", Password = "User@123" }));
    }
}
