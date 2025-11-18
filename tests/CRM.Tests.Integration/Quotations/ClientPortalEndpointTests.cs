using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Mapping;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Quotations
{
    public class ClientPortalEndpointTests
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

        private static (Guid quotationId, string token) SeedQuotationWithAccessLink(AppDbContext db)
        {
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var quotationId = Guid.NewGuid();
            var lineItemId = Guid.NewGuid();
            var accessLinkId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var token = AccessTokenGenerator.Generate();

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
                Status = QuotationStatus.Sent,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                DiscountAmount = 0,
                DiscountPercentage = 0,
                TaxAmount = 180,
                CgstAmount = 90,
                SgstAmount = 90,
                IgstAmount = 0,
                TotalAmount = 1180,
                CreatedAt = now,
                UpdatedAt = now
            };

            var lineItem = new QuotationLineItem
            {
                LineItemId = lineItemId,
                QuotationId = quotationId,
                SequenceNumber = 1,
                ItemName = "Test Item",
                Description = "Test Description",
                Quantity = 10,
                UnitRate = 100,
                Amount = 1000,
                CreatedAt = now,
                UpdatedAt = now
            };

            var accessLink = new QuotationAccessLink
            {
                AccessLinkId = accessLinkId,
                QuotationId = quotationId,
                ClientEmail = "client@example.com",
                AccessToken = token,
                IsActive = true,
                CreatedAt = now,
                ExpiresAt = now.AddDays(90),
                SentAt = now,
                ViewCount = 0
            };

            db.Quotations.Add(quotation);
            db.QuotationLineItems.Add(lineItem);
            db.QuotationAccessLinks.Add(accessLink);
            db.SaveChanges();

            return (quotationId, token);
        }

        [Fact]
        public async Task GetQuotationByAccessToken_ValidToken_ReturnsQuotation()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, token) = SeedQuotationWithAccessLink(db);

            var settings = Options.Create(new QuotationManagementSettings
            {
                BaseUrl = "https://test.com"
            });

            var getByAccessTokenHandler = new Application.Quotations.Queries.Handlers.GetQuotationByAccessTokenQueryHandler(
                db,
                mapper,
                settings);

            var markAsViewedHandler = new Application.Quotations.Commands.Handlers.MarkQuotationAsViewedCommandHandler(
                db,
                settings,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.MarkQuotationAsViewedCommandHandler>>());

            var getByAccessTokenValidator = new Application.Quotations.Validators.GetQuotationByAccessTokenQueryValidator();

            var controller = new ClientPortalController(
                getByAccessTokenHandler,
                markAsViewedHandler,
                null,
                getByAccessTokenValidator,
                null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetQuotationByAccessToken(quotationId, token);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);

            // Verify view tracking
            var accessLink = await db.QuotationAccessLinks.FirstOrDefaultAsync(x => x.AccessToken == token);
            Assert.NotNull(accessLink);
            Assert.Equal(1, accessLink.ViewCount);
            Assert.NotNull(accessLink.FirstViewedAt);
        }

        [Fact]
        public async Task GetQuotationByAccessToken_InvalidToken_ReturnsNotFound()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, _) = SeedQuotationWithAccessLink(db);

            var settings = Options.Create(new QuotationManagementSettings());

            var getByAccessTokenHandler = new Application.Quotations.Queries.Handlers.GetQuotationByAccessTokenQueryHandler(
                db,
                mapper,
                settings);

            var markAsViewedHandler = new Application.Quotations.Commands.Handlers.MarkQuotationAsViewedCommandHandler(
                db,
                settings,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.MarkQuotationAsViewedCommandHandler>>());

            var getByAccessTokenValidator = new Application.Quotations.Validators.GetQuotationByAccessTokenQueryValidator();

            var controller = new ClientPortalController(
                getByAccessTokenHandler,
                markAsViewedHandler,
                null,
                getByAccessTokenValidator,
                null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetQuotationByAccessToken(quotationId, "invalid-token");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SubmitQuotationResponse_ValidRequest_SubmitsSuccessfully()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, token) = SeedQuotationWithAccessLink(db);

            var mockEmailService = new Mock<IQuotationEmailService>();
            mockEmailService.Setup(x => x.SendQuotationAcceptedNotificationAsync(
                It.IsAny<Quotation>(),
                It.IsAny<QuotationResponse>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var submitResponseHandler = new Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler(
                db,
                mockEmailService.Object,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler>>());

            var submitResponseValidator = new Application.Quotations.Validators.SubmitQuotationResponseCommandValidator();

            var controller = new ClientPortalController(
                null,
                null,
                submitResponseHandler,
                null,
                submitResponseValidator);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var request = new Application.Quotations.Dtos.SubmitQuotationResponseRequest
            {
                ResponseType = "ACCEPTED",
                ClientName = "Test Client",
                ClientEmail = "client@example.com",
                ResponseMessage = "Looks good!"
            };

            // Act
            var result = await controller.SubmitQuotationResponse(quotationId, token, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);

            // Verify response recorded
            var quotationResponse = await db.QuotationResponses.FirstOrDefaultAsync(x => x.QuotationId == quotationId);
            Assert.NotNull(quotationResponse);
            Assert.Equal("ACCEPTED", quotationResponse.ResponseType);

            // Verify quotation status updated
            var quotation = await db.Quotations.FindAsync(quotationId);
            Assert.Equal(QuotationStatus.Accepted, quotation.Status);

            // Verify email sent
            mockEmailService.Verify(x => x.SendQuotationAcceptedNotificationAsync(
                It.IsAny<Quotation>(),
                It.IsAny<QuotationResponse>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SubmitQuotationResponse_AlreadyResponded_ReturnsBadRequest()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, token) = SeedQuotationWithAccessLink(db);

            // Add existing response
            db.QuotationResponses.Add(new QuotationResponse
            {
                ResponseId = Guid.NewGuid(),
                QuotationId = quotationId,
                ResponseType = "ACCEPTED",
                ClientEmail = "client@example.com",
                ResponseDate = DateTimeOffset.UtcNow
            });
            db.SaveChanges();

            var mockEmailService = new Mock<IQuotationEmailService>();

            var submitResponseHandler = new Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler(
                db,
                mockEmailService.Object,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler>>());

            var submitResponseValidator = new Application.Quotations.Validators.SubmitQuotationResponseCommandValidator();

            var controller = new ClientPortalController(
                null,
                null,
                submitResponseHandler,
                null,
                submitResponseValidator);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var request = new Application.Quotations.Dtos.SubmitQuotationResponseRequest
            {
                ResponseType = "REJECTED",
                ClientEmail = "client@example.com"
            };

            // Act
            var result = await controller.SubmitQuotationResponse(quotationId, token, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SubmitQuotationResponse_ExpiredLink_ReturnsBadRequest()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, token) = SeedQuotationWithAccessLink(db);

            // Expire the access link
            var accessLink = await db.QuotationAccessLinks.FirstOrDefaultAsync(x => x.AccessToken == token);
            accessLink!.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();

            var mockEmailService = new Mock<IQuotationEmailService>();

            var submitResponseHandler = new Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler(
                db,
                mockEmailService.Object,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SubmitQuotationResponseCommandHandler>>());

            var submitResponseValidator = new Application.Quotations.Validators.SubmitQuotationResponseCommandValidator();

            var controller = new ClientPortalController(
                null,
                null,
                submitResponseHandler,
                null,
                submitResponseValidator);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var request = new Application.Quotations.Dtos.SubmitQuotationResponseRequest
            {
                ResponseType = "ACCEPTED",
                ClientEmail = "client@example.com"
            };

            // Act
            var result = await controller.SubmitQuotationResponse(quotationId, token, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}

