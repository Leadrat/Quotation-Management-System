using System;
using System.Collections.Generic;
using System.Security.Claims;
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Quotations
{
    public class SendQuotationEndpointTests
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

        private static (Guid clientId, Guid userId) SeedData(AppDbContext db)
        {
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
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

            db.SaveChanges();
            return (clientId, userId);
        }

        private static Guid SeedQuotation(AppDbContext db, Guid clientId, Guid userId, QuotationStatus status = QuotationStatus.Draft)
        {
            var quotationId = Guid.NewGuid();
            var lineItemId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var quotation = new Quotation
            {
                QuotationId = quotationId,
                ClientId = clientId,
                CreatedByUserId = userId,
                QuotationNumber = $"QT-2025-{Guid.NewGuid().ToString().Substring(0, 4)}",
                Status = status,
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

            db.Quotations.Add(quotation);
            db.QuotationLineItems.Add(lineItem);
            db.SaveChanges();

            return quotationId;
        }

        [Fact]
        public async Task SendQuotation_DraftStatus_SendsSuccessfully()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = SeedQuotation(db, clientId, userId, QuotationStatus.Draft);

            var mockPdfService = new Mock<IQuotationPdfGenerationService>();
            mockPdfService.Setup(x => x.GenerateQuotationPdf(It.IsAny<Quotation>()))
                .Returns(new byte[] { 1, 2, 3 });

            var mockEmailService = new Mock<IQuotationEmailService>();
            mockEmailService.Setup(x => x.SendQuotationEmailAsync(
                It.IsAny<Quotation>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var settings = Options.Create(new QuotationManagementSettings
            {
                BaseUrl = "https://test.com",
                AccessLinkExpirationDays = 90
            });

            var workflow = new QuotationSendWorkflow(
                db,
                mockPdfService.Object,
                mockEmailService.Object,
                settings,
                Mock.Of<ILogger<QuotationSendWorkflow>>());

            var sendHandler = new Application.Quotations.Commands.Handlers.SendQuotationCommandHandler(
                db,
                workflow,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SendQuotationCommandHandler>>());

            var sendValidator = new Application.Quotations.Validators.SendQuotationRequestValidator();

            var controller = new QuotationsController(
                null, null, null, sendHandler, null,
                null, null, null, null, null, null, null,
                mockPdfService.Object,
                null, null, sendValidator, null, null, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(userId) }
            };

            var request = new Application.Quotations.Dtos.SendQuotationRequest
            {
                RecipientEmail = "client@example.com",
                CustomMessage = "Please review this quotation."
            };

            // Act
            var result = await controller.SendQuotation(quotationId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.True(response.success);

            // Verify quotation status changed to Sent
            var quotation = await db.Quotations.FindAsync(quotationId);
            Assert.Equal(QuotationStatus.Sent, quotation.Status);

            // Verify access link created
            var accessLink = await db.QuotationAccessLinks.FirstOrDefaultAsync(x => x.QuotationId == quotationId);
            Assert.NotNull(accessLink);
            Assert.True(accessLink.IsActive);
            Assert.NotNull(accessLink.SentAt);

            // Verify email was sent
            mockEmailService.Verify(x => x.SendQuotationEmailAsync(
                It.IsAny<Quotation>(),
                "client@example.com",
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                null,
                null,
                "Please review this quotation."), Times.Once);
        }

        [Fact]
        public async Task SendQuotation_NonDraftStatus_ReturnsBadRequest()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = SeedQuotation(db, clientId, userId, QuotationStatus.Sent);

            var mockPdfService = new Mock<IQuotationPdfGenerationService>();
            var mockEmailService = new Mock<IQuotationEmailService>();
            var settings = Options.Create(new QuotationManagementSettings());

            var workflow = new QuotationSendWorkflow(
                db,
                mockPdfService.Object,
                mockEmailService.Object,
                settings,
                Mock.Of<ILogger<QuotationSendWorkflow>>());

            var sendHandler = new Application.Quotations.Commands.Handlers.SendQuotationCommandHandler(
                db,
                workflow,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SendQuotationCommandHandler>>());

            var sendValidator = new Application.Quotations.Validators.SendQuotationRequestValidator();

            var controller = new QuotationsController(
                null, null, null, sendHandler, null,
                null, null, null, null, null, null, null,
                mockPdfService.Object,
                null, null, sendValidator, null, null, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(userId) }
            };

            var request = new Application.Quotations.Dtos.SendQuotationRequest
            {
                RecipientEmail = "client@example.com"
            };

            // Act
            var result = await controller.SendQuotation(quotationId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SendQuotation_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = SeedQuotation(db, clientId, userId, QuotationStatus.Draft);

            var otherUserId = Guid.NewGuid();

            var mockPdfService = new Mock<IQuotationPdfGenerationService>();
            var mockEmailService = new Mock<IQuotationEmailService>();
            var settings = Options.Create(new QuotationManagementSettings());

            var workflow = new QuotationSendWorkflow(
                db,
                mockPdfService.Object,
                mockEmailService.Object,
                settings,
                Mock.Of<ILogger<QuotationSendWorkflow>>());

            var sendHandler = new Application.Quotations.Commands.Handlers.SendQuotationCommandHandler(
                db,
                workflow,
                Mock.Of<ILogger<Application.Quotations.Commands.Handlers.SendQuotationCommandHandler>>());

            var sendValidator = new Application.Quotations.Validators.SendQuotationRequestValidator();

            var controller = new QuotationsController(
                null, null, null, sendHandler, null,
                null, null, null, null, null, null, null,
                mockPdfService.Object,
                null, null, sendValidator, null, null, null, null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = SalesRep(otherUserId) }
            };

            var request = new Application.Quotations.Dtos.SendQuotationRequest
            {
                RecipientEmail = "client@example.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await controller.SendQuotation(quotationId, request));
        }
    }
}

