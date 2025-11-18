using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Quotations
{
    public class QuotationStatusHistoryEndpointTests
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
            var cfg = new MapperConfiguration(c => {
                c.AddProfile(new QuotationProfile());
                c.AddProfile(new QuotationManagementProfile());
                c.AddProfile(new ClientProfile());
                c.AddProfile(new UserProfile());
            });
            return cfg.CreateMapper();
        }

        private static ClaimsPrincipal SalesRep(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "SalesRep")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Admin")
            }, "TestAuth"));
        }

        private static (Guid quotationId, Guid userId) SeedQuotationWithHistory(AppDbContext db)
        {
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var quotationId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Roles.Add(new Role
            {
                RoleId = roleId,
                RoleName = "SalesRep",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            db.Users.Add(new User
            {
                UserId = userId,
                Email = "salesrep@example.com",
                PasswordHash = "hash",
                FirstName = "Sales",
                LastName = "Rep",
                IsActive = true,
                RoleId = roleId,
                CreatedAt = now,
                UpdatedAt = now
            });

            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "Test Company",
                Email = "client@example.com",
                Mobile = "1234567890",
                CreatedByUserId = userId,
                StateCode = "27",
                CreatedAt = now,
                UpdatedAt = now
            });

            var quotation = new Quotation
            {
                QuotationId = quotationId,
                ClientId = clientId,
                CreatedByUserId = userId,
                QuotationNumber = "QT-2025-TEST",
                Status = QuotationStatus.Viewed,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                TotalAmount = 1180,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Add status history entries
            db.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotationId,
                PreviousStatus = null,
                NewStatus = "Draft",
                ChangedByUserId = userId,
                ChangedAt = now.AddDays(-2),
                Reason = "Quotation created"
            });

            db.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotationId,
                PreviousStatus = "Draft",
                NewStatus = "Sent",
                ChangedByUserId = userId,
                ChangedAt = now.AddDays(-1),
                Reason = "Quotation sent to client"
            });

            db.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotationId,
                PreviousStatus = "Sent",
                NewStatus = "Viewed",
                ChangedAt = now,
                Reason = "Quotation viewed by client"
            });

            db.Quotations.Add(quotation);
            db.SaveChanges();

            return (quotationId, userId);
        }

        [Fact]
        public async Task GetQuotationStatusHistory_ValidQuotation_ReturnsHistory()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithHistory(db);

            var getStatusHistoryHandler = new Application.Quotations.Queries.Handlers.GetQuotationStatusHistoryQueryHandler(
                db,
                mapper);

            var getStatusHistoryValidator = new Application.Quotations.Validators.GetQuotationStatusHistoryQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null,
                getStatusHistoryHandler, null, null, null,
                null,
                null, null, null, null,
                getStatusHistoryValidator, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(userId) }
            };

            // Act
            var result = await controller.GetQuotationStatusHistory(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);

            var history = response.data as System.Collections.IEnumerable;
            Assert.NotNull(history);
            Assert.Equal(3, history.Cast<object>().Count());
        }

        [Fact]
        public async Task GetQuotationStatusHistory_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithHistory(db);

            var otherUserId = Guid.NewGuid();

            var getStatusHistoryHandler = new Application.Quotations.Queries.Handlers.GetQuotationStatusHistoryQueryHandler(
                db,
                mapper);

            var getStatusHistoryValidator = new Application.Quotations.Validators.GetQuotationStatusHistoryQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null,
                getStatusHistoryHandler, null, null, null,
                null,
                null, null, null, null,
                getStatusHistoryValidator, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(otherUserId) }
            };

            // Act
            var result = await controller.GetQuotationStatusHistory(quotationId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(403, objectResult?.StatusCode);
        }

        [Fact]
        public async Task GetQuotationStatusHistory_AdminUser_ReturnsHistory()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithHistory(db);

            var adminUserId = Guid.NewGuid();

            var getStatusHistoryHandler = new Application.Quotations.Queries.Handlers.GetQuotationStatusHistoryQueryHandler(
                db,
                mapper);

            var getStatusHistoryValidator = new Application.Quotations.Validators.GetQuotationStatusHistoryQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null,
                getStatusHistoryHandler, null, null, null,
                null,
                null, null, null, null,
                getStatusHistoryValidator, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Admin(adminUserId) }
            };

            // Act
            var result = await controller.GetQuotationStatusHistory(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);
        }
    }
}

