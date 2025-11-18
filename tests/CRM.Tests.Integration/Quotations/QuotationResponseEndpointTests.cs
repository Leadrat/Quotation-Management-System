using System;
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
using Xunit;

namespace CRM.Tests.Integration.Quotations
{
    public class QuotationResponseEndpointTests
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

        private static (Guid quotationId, Guid userId) SeedQuotationWithResponse(AppDbContext db, bool includeResponse = true)
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
                Status = includeResponse ? QuotationStatus.Accepted : QuotationStatus.Viewed,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                TotalAmount = 1180,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Quotations.Add(quotation);

            if (includeResponse)
            {
                db.QuotationResponses.Add(new QuotationResponse
                {
                    ResponseId = Guid.NewGuid(),
                    QuotationId = quotationId,
                    ResponseType = "ACCEPTED",
                    ClientEmail = "client@example.com",
                    ClientName = "Test Client",
                    ResponseMessage = "Looks good!",
                    ResponseDate = now,
                    IpAddress = "127.0.0.1"
                });
            }

            db.SaveChanges();

            return (quotationId, userId);
        }

        [Fact]
        public async Task GetQuotationResponse_WithResponse_ReturnsResponse()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithResponse(db, includeResponse: true);

            var getResponseHandler = new Application.Quotations.Queries.Handlers.GetQuotationResponseQueryHandler(
                db,
                mapper);

            var getResponseValidator = new Application.Quotations.Validators.GetQuotationResponseQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null, null,
                getResponseHandler, null, null,
                null,
                null, null, null, null, null,
                getResponseValidator, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(userId) }
            };

            // Act
            var result = await controller.GetQuotationResponse(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);
            Assert.NotNull(response.data);
        }

        [Fact]
        public async Task GetQuotationResponse_NoResponse_ReturnsNoContent()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithResponse(db, includeResponse: false);

            var getResponseHandler = new Application.Quotations.Queries.Handlers.GetQuotationResponseQueryHandler(
                db,
                mapper);

            var getResponseValidator = new Application.Quotations.Validators.GetQuotationResponseQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null, null,
                getResponseHandler, null, null,
                null,
                null, null, null, null, null,
                getResponseValidator, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(userId) }
            };

            // Act
            var result = await controller.GetQuotationResponse(quotationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetQuotationResponse_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithResponse(db, includeResponse: true);

            var otherUserId = Guid.NewGuid();

            var getResponseHandler = new Application.Quotations.Queries.Handlers.GetQuotationResponseQueryHandler(
                db,
                mapper);

            var getResponseValidator = new Application.Quotations.Validators.GetQuotationResponseQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null, null,
                getResponseHandler, null, null,
                null,
                null, null, null, null, null,
                getResponseValidator, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(otherUserId) }
            };

            // Act
            var result = await controller.GetQuotationResponse(quotationId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(403, objectResult?.StatusCode);
        }

        [Fact]
        public async Task GetQuotationResponse_AdminUser_ReturnsResponse()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, userId) = SeedQuotationWithResponse(db, includeResponse: true);

            var adminUserId = Guid.NewGuid();

            var getResponseHandler = new Application.Quotations.Queries.Handlers.GetQuotationResponseQueryHandler(
                db,
                mapper);

            var getResponseValidator = new Application.Quotations.Validators.GetQuotationResponseQueryValidator();

            var controller = new QuotationsController(
                null, null, null, null, null,
                null, null, null, null,
                getResponseHandler, null, null,
                null,
                null, null, null, null, null,
                getResponseValidator, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Admin(adminUserId) }
            };

            // Act
            var result = await controller.GetQuotationResponse(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);
        }
    }
}

