using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Integration.Quotations
{
    public class QuotationEndpointTests
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
                c.AddProfile(new CRM.Application.Mapping.QuotationProfile());
                c.AddProfile(new CRM.Application.Mapping.ClientProfile());
                c.AddProfile(new CRM.Application.Mapping.UserProfile());
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

        private static QuotationsController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var quotationSettings = Options.Create(new QuotationSettings());
            var companySettings = Options.Create(new CompanySettings { StateCode = "27", StateName = "Maharashtra" });
            var numberGenerator = new CRM.Application.Quotations.Services.QuotationNumberGenerator(db, quotationSettings);
            var totalsCalculator = new CRM.Application.Quotations.Services.QuotationTotalsCalculator();
            var taxCalculator = new CRM.Application.Quotations.Services.TaxCalculationService(companySettings);
            var createHandler = new CRM.Application.Quotations.Commands.Handlers.CreateQuotationCommandHandler(
                db, mapper, numberGenerator, totalsCalculator, taxCalculator, quotationSettings);
            var updateHandler = new CRM.Application.Quotations.Commands.Handlers.UpdateQuotationCommandHandler(
                db, mapper, totalsCalculator, taxCalculator, quotationSettings);
            var deleteHandler = new CRM.Application.Quotations.Commands.Handlers.DeleteQuotationCommandHandler(db);
            var getByIdHandler = new CRM.Application.Quotations.Queries.Handlers.GetQuotationByIdQueryHandler(db, mapper);
            var getAllHandler = new CRM.Application.Quotations.Queries.Handlers.GetAllQuotationsQueryHandler(db, mapper);
            var getByClientHandler = new CRM.Application.Quotations.Queries.Handlers.GetQuotationsByClientQueryHandler(db, mapper);
            var createValidator = new CRM.Application.Quotations.Validators.CreateQuotationRequestValidator();
            var updateValidator = new CRM.Application.Quotations.Validators.UpdateQuotationRequestValidator();

            var ctrl = new QuotationsController(
                createHandler, updateHandler, deleteHandler,
                getByIdHandler, getAllHandler, getByClientHandler,
                createValidator, updateValidator);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
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
                Email = "test@example.com",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
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

        [Fact]
        public async Task CreateQuotation_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var controller = NewController(db, mapper, SalesRep(userId));

            var request = new CRM.Application.Quotations.Dtos.CreateQuotationRequest
            {
                ClientId = clientId,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                DiscountPercentage = 10,
                LineItems = new List<CRM.Application.Quotations.Dtos.CreateLineItemRequest>
                {
                    new CRM.Application.Quotations.Dtos.CreateLineItemRequest
                    {
                        ItemName = "Test Item",
                        Quantity = 10,
                        UnitRate = 100
                    }
                }
            };

            // Act
            var result = await controller.CreateQuotation(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var response = Assert.IsType<Dictionary<string, object>>(createdResult.Value);
            Assert.True((bool)response["success"]);
        }

        [Fact]
        public async Task GetQuotationById_ExistingQuotation_ReturnsQuotation()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                ClientId = clientId,
                CreatedByUserId = userId,
                QuotationNumber = "QT-2025-0001",
                Status = QuotationStatus.Draft,
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
            });
            db.SaveChanges();

            var controller = NewController(db, mapper, SalesRep(userId));

            // Act
            var result = await controller.GetQuotationById(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.True((bool)response["success"]);
        }

        [Fact]
        public async Task GetAllQuotations_ReturnsPaginatedResults()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var now = DateTimeOffset.UtcNow;

            for (int i = 0; i < 5; i++)
            {
                db.Quotations.Add(new Quotation
                {
                    QuotationId = Guid.NewGuid(),
                    ClientId = clientId,
                    CreatedByUserId = userId,
                    QuotationNumber = $"QT-2025-{i:D4}",
                    Status = QuotationStatus.Draft,
                    QuotationDate = DateTime.Today,
                    ValidUntil = DateTime.Today.AddDays(30),
                    SubTotal = 1000,
                    TotalAmount = 1180,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            db.SaveChanges();

            var controller = NewController(db, mapper, SalesRep(userId));

            // Act
            var result = await controller.GetAllQuotations(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.True((bool)response["success"]);
        }

        [Fact]
        public async Task UpdateQuotation_DraftStatus_UpdatesSuccessfully()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                ClientId = clientId,
                CreatedByUserId = userId,
                QuotationNumber = "QT-2025-0001",
                Status = QuotationStatus.Draft,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                TotalAmount = 1180,
                CreatedAt = now,
                UpdatedAt = now
            });
            db.SaveChanges();

            var controller = NewController(db, mapper, SalesRep(userId));
            var updateRequest = new CRM.Application.Quotations.Dtos.UpdateQuotationRequest
            {
                DiscountPercentage = 15
            };

            // Act
            var result = await controller.UpdateQuotation(quotationId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.True((bool)response["success"]);
        }

        [Fact]
        public async Task DeleteQuotation_DraftStatus_DeletesSuccessfully()
        {
            // Arrange
            var db = NewDb();
            var mapper = NewMapper();
            var (clientId, userId) = SeedData(db);
            var quotationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                ClientId = clientId,
                CreatedByUserId = userId,
                QuotationNumber = "QT-2025-0001",
                Status = QuotationStatus.Draft,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                TotalAmount = 1180,
                CreatedAt = now,
                UpdatedAt = now
            });
            db.SaveChanges();

            var controller = NewController(db, mapper, SalesRep(userId));

            // Act
            var result = await controller.DeleteQuotation(quotationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.True((bool)response["success"]);

            // Verify status changed to Cancelled
            var quotation = await db.Quotations.FindAsync(quotationId);
            Assert.Equal(QuotationStatus.Cancelled, quotation.Status);
        }
    }
}

