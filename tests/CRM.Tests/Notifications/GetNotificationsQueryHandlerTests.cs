using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Mapping;
using CRM.Application.Notifications.Queries;
using CRM.Application.Notifications.Queries.Handlers;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Notifications
{
    public class GetNotificationsQueryHandlerTests
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
        public async Task Handle_ReturnsOnlyUnreadNotifications_WhenUnreadFilterIsTrue()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var handler = new GetNotificationsQueryHandler(db, mapper, NullLogger<GetNotificationsQueryHandler>.Instance);

            var userId = Guid.NewGuid();
            var readNotification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationSent",
                Message = "Read",
                IsRead = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var unreadNotification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationViewed",
                Message = "Unread",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Notifications.AddRange(readNotification, unreadNotification);
            await db.SaveChangesAsync();

            var query = new GetNotificationsQuery
            {
                Unread = true,
                PageNumber = 1,
                PageSize = 20,
                RequestorUserId = userId
            };

            // Act
            var result = await handler.Handle(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Equal(unreadNotification.NotificationId, result.Data[0].NotificationId);
        }

        [Fact]
        public async Task Handle_ReturnsPagedResults()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var handler = new GetNotificationsQueryHandler(db, mapper, NullLogger<GetNotificationsQueryHandler>.Instance);

            var userId = Guid.NewGuid();
            for (int i = 0; i < 5; i++)
            {
                db.Notifications.Add(new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientUserId = userId,
                    EventType = "QuotationSent",
                    Message = $"Test {i}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
                });
            }
            await db.SaveChangesAsync();

            var query = new GetNotificationsQuery
            {
                PageNumber = 1,
                PageSize = 2,
                RequestorUserId = userId
            };

            // Act
            var result = await handler.Handle(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Length);
            Assert.Equal(5, result.TotalCount);
        }
    }
}

