using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Commands.Handlers;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Application.Mapping;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.DiscountApprovals
{
    public class RequestDiscountApprovalCommandHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new DiscountApprovalProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_CreatesManagerApproval_ForDiscountBetween10And20()
        {
            using var context = CreateContext();
            var (quotation, userId) = await SeedQuotation(context, 15.0m);
            var mapper = NewMapper();

            var handler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var command = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = quotation.QuotationId,
                    RequestedDiscountPercentage = 15.0m,
                    Reason = "Competitive pricing required for this client",
                    Comments = "Client requested discount"
                }
            };

            var result = await handler.Handle(command);

            Assert.NotNull(result);
            Assert.Equal(ApprovalLevel.Manager, result.ApprovalLevel);
            Assert.Equal(ApprovalStatus.Pending, result.Status);
            Assert.Equal(15.0m, result.RequestedDiscountPercentage);

            var saved = await context.DiscountApprovals
                .FirstAsync(a => a.ApprovalId == result.ApprovalId);

            Assert.NotNull(saved);
            Assert.Equal(ApprovalLevel.Manager, saved.ApprovalLevel);
            Assert.True(quotation.IsPendingApproval);
        }

        [Fact]
        public async Task Handle_CreatesAdminApproval_ForDiscountAbove20()
        {
            using var context = CreateContext();
            var (quotation, userId) = await SeedQuotation(context, 25.0m);
            var mapper = NewMapper();

            var handler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var command = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = quotation.QuotationId,
                    RequestedDiscountPercentage = 25.0m,
                    Reason = "Large volume order requires significant discount",
                    Comments = "Strategic client"
                }
            };

            var result = await handler.Handle(command);

            Assert.NotNull(result);
            Assert.Equal(ApprovalLevel.Admin, result.ApprovalLevel);
            Assert.Equal(ApprovalStatus.Pending, result.Status);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenQuotationNotFound()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUser(context, userId);
            var mapper = NewMapper();

            var handler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var command = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = Guid.NewGuid(),
                    RequestedDiscountPercentage = 15.0m,
                    Reason = "Test reason"
                }
            };

            await Assert.ThrowsAsync<QuotationNotFoundException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenQuotationAlreadyPending()
        {
            using var context = CreateContext();
            var (quotation, userId) = await SeedQuotation(context, 15.0m);
            var mapper = NewMapper();

            // Create first approval
            var firstHandler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var firstCommand = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = quotation.QuotationId,
                    RequestedDiscountPercentage = 15.0m,
                    Reason = "First request"
                }
            };

            await firstHandler.Handle(firstCommand);

            // Try to create second approval
            var secondHandler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var secondCommand = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = quotation.QuotationId,
                    RequestedDiscountPercentage = 15.0m,
                    Reason = "Second request"
                }
            };

            await Assert.ThrowsAsync<QuotationLockedException>(() => secondHandler.Handle(secondCommand));
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenDiscountBelowThreshold()
        {
            using var context = CreateContext();
            var (quotation, userId) = await SeedQuotation(context, 5.0m);
            var mapper = NewMapper();

            var handler = new RequestDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RequestDiscountApprovalCommandHandler>.Instance);

            var command = new RequestDiscountApprovalCommand
            {
                RequestedByUserId = userId,
                Request = new CreateDiscountApprovalRequest
                {
                    QuotationId = quotation.QuotationId,
                    RequestedDiscountPercentage = 5.0m,
                    Reason = "Test reason"
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static void SeedUser(AppDbContext context, Guid userId)
        {
            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "SalesRep"
            };
            context.Roles.Add(role);

            context.Users.Add(new User
            {
                UserId = userId,
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        private static async Task<(Quotation, Guid)> SeedQuotation(AppDbContext context, decimal discountPercentage)
        {
            var userId = Guid.NewGuid();
            SeedUser(context, userId);

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Test Client",
                Email = "client@example.com",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                QuotationNumber = "QT-001",
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = userId,
                CreatedByUser = context.Users.First(u => u.UserId == userId),
                Status = QuotationStatus.Draft,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                DiscountPercentage = discountPercentage,
                SubTotal = 1000.0m,
                TaxPercentage = 10.0m,
                TaxAmount = 100.0m,
                TotalAmount = 1100.0m,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Quotations.Add(quotation);

            await context.SaveChangesAsync();
            return (quotation, userId);
        }
    }
}

