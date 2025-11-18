using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Mapping;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Commands.Handlers;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Notifications
{
    public class MarkNotificationsReadCommandHandlerTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static IMapper NewMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile(new NotificationProfile());
            });
            return cfg.CreateMapper();
        }

        [Fact]
        public async Task Handle_MarksSpecificNotificationsAsRead()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var handler = new MarkNotificationsReadCommandHandler(db, mapper, NullLogger<MarkNotificationsReadCommandHandler>.Instance);

            var userId = Guid.NewGuid();
            var notification1 = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationSent",
                Message = "Test 1",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var notification2 = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationViewed",
                Message = "Test 2",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Notifications.AddRange(notification1, notification2);
            await db.SaveChangesAsync();

            var command = new MarkNotificationsReadCommand
            {
                NotificationIds = new List<Guid> { notification1.NotificationId },
                RequestedByUserId = userId
            };

            // Act
            var result = await handler.Handle(command);

            // Assert
            Assert.True(result.Success);
            var updated1 = await db.Notifications.FindAsync(notification1.NotificationId);
            var updated2 = await db.Notifications.FindAsync(notification2.NotificationId);
            Assert.True(updated1?.IsRead);
            Assert.False(updated2?.IsRead);
        }

        [Fact]
        public async Task Handle_MarksAllNotificationsAsRead_WhenNotificationIdsIsNull()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var handler = new MarkNotificationsReadCommandHandler(db, mapper, NullLogger<MarkNotificationsReadCommandHandler>.Instance);

            var userId = Guid.NewGuid();
            var notification1 = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationSent",
                Message = "Test 1",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var notification2 = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationViewed",
                Message = "Test 2",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Notifications.AddRange(notification1, notification2);
            await db.SaveChangesAsync();

            var command = new MarkNotificationsReadCommand
            {
                NotificationIds = null,
                RequestedByUserId = userId
            };

            // Act
            var result = await handler.Handle(command);

            // Assert
            Assert.True(result.Success);
            var allRead = await db.Notifications
                .Where(n => n.RecipientUserId == userId)
                .AllAsync(n => n.IsRead);
            Assert.True(allRead);
        }
    }
}

