using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Application.Auth.Services;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Security;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Auth
{
    public class ChangePasswordTests
    {
        private static AppDbContext NewDb()
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<AppDbContext>();
        }

        private class FakeRevoker : IRefreshTokenRevoker
        {
            public int Calls;
            public Task RevokeAllForUserAsync(Guid userId)
            {
                Calls++;
                return Task.CompletedTask;
            }
        }

        private class FakeEmailQueue : IEmailQueue
        {
            public int Calls;
            public Task EnqueueAsync(EmailMessage message)
            {
                Calls++;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task ChangePassword_Succeeds_And_Revokes_Tokens()
        {
            using var db = NewDb();
            IPasswordHasher hasher = new BCryptPasswordHasher();
            var revoker = new FakeRevoker();
            var emails = new FakeEmailQueue();

            var userId = Guid.NewGuid();
            db.Users.Add(new CRM.Domain.Entities.User
            {
                UserId = userId,
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Smith",
                PasswordHash = hasher.Hash("OldPass!23"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var handler = new ChangePasswordCommandHandler(db, hasher, revoker, emails);
            var cmd = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = "OldPass!23",
                NewPassword = "NewPass!23",
                ConfirmPassword = "NewPass!23"
            };

            await handler.Handle(cmd);

            var user = await db.Users.FirstAsync();
            Assert.True(hasher.Verify(user.PasswordHash, "NewPass!23"));
            Assert.Equal(0, user.LoginAttempts);
            Assert.False(user.IsLockedOut);
            Assert.Equal(1, revoker.Calls);
            Assert.Equal(1, emails.Calls);
        }

        [Fact]
        public async Task ChangePassword_InvalidCurrent_Increments_Attempts_And_Locks()
        {
            using var db = NewDb();
            IPasswordHasher hasher = new BCryptPasswordHasher();
            var revoker = new FakeRevoker();
            var emails = new FakeEmailQueue();

            var userId = Guid.NewGuid();
            db.Users.Add(new CRM.Domain.Entities.User
            {
                UserId = userId,
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Smith",
                PasswordHash = hasher.Hash("OldPass!23"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var handler = new ChangePasswordCommandHandler(db, hasher, revoker, emails);

            // 5 invalid attempts should lock
            for (int i = 1; i <= 5; i++)
            {
                await Assert.ThrowsAsync<InvalidCurrentPasswordException>(async () =>
                {
                    await handler.Handle(new ChangePasswordCommand
                    {
                        UserId = userId,
                        CurrentPassword = "Wrong!23",
                        NewPassword = "NewPass!23",
                        ConfirmPassword = "NewPass!23"
                    });
                });
            }

            var user = await db.Users.FirstAsync();
            Assert.Equal(5, user.LoginAttempts);
            Assert.True(user.IsLockedOut);
        }
    }
}
