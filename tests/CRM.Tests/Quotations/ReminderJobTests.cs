using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class ReminderJobTests
    {
        [Fact]
        public async Task UnviewedReminder_Sends_ForOldSentQuotation()
        {
            using var context = CreateContext();
            var emailService = new TestEmailService();
            var service = new QuotationReminderService(context, emailService, NullLogger<QuotationReminderService>.Instance);
            await SeedQuotationAsync(context, sentDaysAgo: 4, firstViewedAt: null);

            var count = await service.SendUnviewedRemindersAsync(DateTimeOffset.UtcNow);

            Assert.Equal(1, count);
            Assert.Single(emailService.UnviewedReminders);
        }

        [Fact]
        public async Task UnviewedReminder_IgnoresViewedQuotations()
        {
            using var context = CreateContext();
            var emailService = new TestEmailService();
            var service = new QuotationReminderService(context, emailService, NullLogger<QuotationReminderService>.Instance);
            await SeedQuotationAsync(context, sentDaysAgo: 5, firstViewedAt: DateTimeOffset.UtcNow.AddDays(-1));

            var count = await service.SendUnviewedRemindersAsync(DateTimeOffset.UtcNow);

            Assert.Equal(0, count);
            Assert.Empty(emailService.UnviewedReminders);
        }

        [Fact]
        public async Task PendingResponseReminder_Sends_WhenViewedSevenDaysAgo()
        {
            using var context = CreateContext();
            var emailService = new TestEmailService();
            var service = new QuotationReminderService(context, emailService, NullLogger<QuotationReminderService>.Instance);
            await SeedQuotationAsync(context, sentDaysAgo: 10, firstViewedAt: DateTimeOffset.UtcNow.AddDays(-8));

            var count = await service.SendPendingResponseFollowUpsAsync(DateTimeOffset.UtcNow);

            Assert.Equal(1, count);
            Assert.Single(emailService.PendingReminders);
        }

        [Fact]
        public async Task PendingResponseReminder_Skips_WhenResponseExists()
        {
            using var context = CreateContext();
            var emailService = new TestEmailService();
            var service = new QuotationReminderService(context, emailService, NullLogger<QuotationReminderService>.Instance);
            var quotation = await SeedQuotationAsync(context, sentDaysAgo: 10, firstViewedAt: DateTimeOffset.UtcNow.AddDays(-8));

            context.QuotationResponses.Add(new QuotationResponse
            {
                ResponseId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ResponseType = "ACCEPTED",
                ClientEmail = "client@example.com",
                ResponseDate = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();

            var count = await service.SendPendingResponseFollowUpsAsync(DateTimeOffset.UtcNow);

            Assert.Equal(0, count);
            Assert.Empty(emailService.PendingReminders);
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<Quotation> SeedQuotationAsync(
            AppDbContext context,
            int sentDaysAgo,
            DateTimeOffset? firstViewedAt)
        {
            var ownerId = Guid.NewGuid();
            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                CreatedByUserId = ownerId,
                QuotationNumber = "QT-REM",
                Status = firstViewedAt.HasValue ? QuotationStatus.Viewed : QuotationStatus.Sent,
                QuotationDate = DateTime.Today.AddDays(-sentDaysAgo),
                ValidUntil = DateTime.Today.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-sentDaysAgo),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-sentDaysAgo)
            };

            context.Quotations.Add(quotation);

            context.Users.Add(new User
            {
                UserId = ownerId,
                Email = "owner@example.com",
                FirstName = "Owner",
                LastName = "User",
                RoleId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });

            context.QuotationAccessLinks.Add(new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                Quotation = quotation,
                ClientEmail = "client@example.com",
                AccessToken = Guid.NewGuid().ToString("N"),
                SentAt = DateTimeOffset.UtcNow.AddDays(-sentDaysAgo),
                FirstViewedAt = firstViewedAt,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-sentDaysAgo),
                IsActive = true
            });

            await context.SaveChangesAsync();
            return quotation;
        }

        private class TestEmailService : IQuotationEmailService
        {
            public List<Guid> UnviewedReminders { get; } = new();
            public List<Guid> PendingReminders { get; } = new();

            public Task SendQuotationEmailAsync(Quotation quotation, string recipientEmail, byte[] pdfAttachment, string accessLink, List<string>? ccEmails = null, List<string>? bccEmails = null, string? customMessage = null)
                => Task.CompletedTask;

            public Task SendQuotationAcceptedNotificationAsync(Quotation quotation, QuotationResponse response, string salesRepEmail)
                => Task.CompletedTask;

            public Task SendQuotationRejectedNotificationAsync(Quotation quotation, QuotationResponse response, string salesRepEmail)
                => Task.CompletedTask;

            public Task SendUnviewedQuotationReminderAsync(Quotation quotation, string salesRepEmail, DateTimeOffset sentAt)
            {
                UnviewedReminders.Add(quotation.QuotationId);
                return Task.CompletedTask;
            }

            public Task SendPendingResponseFollowUpAsync(Quotation quotation, string salesRepEmail, DateTimeOffset firstViewedAt)
            {
                PendingReminders.Add(quotation.QuotationId);
                return Task.CompletedTask;
            }
        }
    }
}


