using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Common.Results;
using CRM.Application.Mapping;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Commands.Handlers;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using CRM.Application.Notifications.Queries.Handlers;
using CRM.Application.Notifications.Validators;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Integration.Notifications
{
    public class NotificationsControllerTests
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

        private static ClaimsPrincipal User(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "SalesRep")
            }, "TestAuth"));
        }

        private static NotificationsController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var markReadHandler = new MarkNotificationsReadCommandHandler(db, mapper, NullLogger<MarkNotificationsReadCommandHandler>.Instance);
            var archiveHandler = new ArchiveNotificationsCommandHandler(db, mapper, NullLogger<ArchiveNotificationsCommandHandler>.Instance);
            var unarchiveHandler = new UnarchiveNotificationsCommandHandler(db, mapper, NullLogger<UnarchiveNotificationsCommandHandler>.Instance);
            var updatePrefsHandler = new UpdateNotificationPreferencesCommandHandler(db, mapper, NullLogger<UpdateNotificationPreferencesCommandHandler>.Instance);
            var getNotificationsHandler = new GetNotificationsQueryHandler(db, mapper, NullLogger<GetNotificationsQueryHandler>.Instance);
            var getUnreadCountHandler = new GetUnreadCountQueryHandler(db, NullLogger<GetUnreadCountQueryHandler>.Instance);
            var getPreferencesHandler = new GetNotificationPreferencesQueryHandler(db, mapper, NullLogger<GetNotificationPreferencesQueryHandler>.Instance);
            var getEntityNotificationsHandler = new GetEntityNotificationsQueryHandler(db, mapper, NullLogger<GetEntityNotificationsQueryHandler>.Instance);
            var getEmailLogsHandler = new GetEmailNotificationLogsQueryHandler(db, mapper, NullLogger<GetEmailNotificationLogsQueryHandler>.Instance);

            var markReadValidator = new MarkNotificationsReadCommandValidator();
            var archiveValidator = new ArchiveNotificationsCommandValidator();
            var unarchiveValidator = new UnarchiveNotificationsCommandValidator();
            var updatePrefsValidator = new UpdateNotificationPreferencesCommandValidator();
            var getNotificationsValidator = new GetNotificationsQueryValidator();
            var getUnreadCountValidator = new GetUnreadCountQueryValidator();
            var getPreferencesValidator = new GetNotificationPreferencesQueryValidator();
            var getEntityNotificationsValidator = new GetEntityNotificationsQueryValidator();
            var getEmailLogsValidator = new GetEmailNotificationLogsQueryValidator();

            var ctrl = new NotificationsController(
                markReadHandler, archiveHandler, unarchiveHandler, updatePrefsHandler,
                getNotificationsHandler, getUnreadCountHandler, getPreferencesHandler,
                getEntityNotificationsHandler, getEmailLogsHandler,
                markReadValidator, archiveValidator, unarchiveValidator, updatePrefsValidator,
                getNotificationsValidator, getUnreadCountValidator, getPreferencesValidator,
                getEntityNotificationsValidator, getEmailLogsValidator);

            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
        }

        [Fact]
        public async Task GetNotifications_ReturnsNotificationsForUser()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var user = User(userId);
            var controller = NewController(db, mapper, user);

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationSent",
                Message = "Test notification",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            // Act
            var result = await controller.GetNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<PagedResult<NotificationDto>>(okResult.Value);
            Assert.True(pagedResult.Success);
            Assert.Single(pagedResult.Data);
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsCorrectCount()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var user = User(userId);
            var controller = NewController(db, mapper, user);

            db.Notifications.AddRange(
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientUserId = userId,
                    EventType = "QuotationSent",
                    Message = "Unread 1",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientUserId = userId,
                    EventType = "QuotationViewed",
                    Message = "Unread 2",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    RecipientUserId = userId,
                    EventType = "QuotationAccepted",
                    Message = "Read",
                    IsRead = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            );
            await db.SaveChangesAsync();

            // Act
            var result = await controller.GetUnreadCount();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.Equal(2, response.data.count);
        }

        [Fact]
        public async Task MarkRead_MarksNotificationsAsRead()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var user = User(userId);
            var controller = NewController(db, mapper, user);

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                EventType = "QuotationSent",
                Message = "Test",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            var request = new MarkNotificationsReadRequest
            {
                NotificationIds = new[] { notification.NotificationId }
            };

            // Act
            var result = await controller.MarkRead(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updated = await db.Notifications.FindAsync(notification.NotificationId);
            Assert.True(updated?.IsRead);
        }
    }
}

