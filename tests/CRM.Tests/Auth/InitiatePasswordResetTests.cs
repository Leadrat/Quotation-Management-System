using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Auth
{
    public class InitiatePasswordResetTests
    {
        private static AppDbContext NewDb()
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            var provider = services.BuildServiceProvider();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(provider)
                .Options;
            return new AppDbContext(options);
        }

        private class FakeTokenGen : IResetTokenGenerator
        {
            public (string token, byte[] hash) Generate()
            {
                var token = "token123";
                var hash = System.Text.Encoding.UTF8.GetBytes("hash");
                return (token, hash);
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
        public async Task Creates_Reset_Token_And_Enqueues_Email()
        {
            using var db = NewDb();
            var user = new User { UserId = Guid.NewGuid(), Email = "user@example.com", FirstName = "U", LastName = "S" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var q = new FakeEmailQueue();
            var h = new InitiatePasswordResetCommandHandler(db, new FakeTokenGen(), q);
            await h.Handle(new InitiatePasswordResetCommand { TargetUserId = user.UserId, AdminUserId = Guid.NewGuid() });

            var token = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
            Assert.NotNull(token);
            Assert.True(token!.ExpiresAt > DateTimeOffset.UtcNow);
            Assert.Equal(1, q.Calls);
        }
    }
}
