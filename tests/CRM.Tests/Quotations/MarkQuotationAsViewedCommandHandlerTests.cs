using System;
using System.Threading.Tasks;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class MarkQuotationAsViewedCommandHandlerTests
    {
        [Fact]
        public async Task Handle_IncrementsViewCount_And_UpdatesStatus()
        {
            using var context = CreateContext();
            var quotation = await SeedQuotationWithAccessLink(context, QuotationStatus.Sent);

            var handler = new MarkQuotationAsViewedCommandHandler(
                context,
                Options.Create(new CRM.Shared.Config.QuotationManagementSettings { BaseUrl = "https://app.test" }),
                NullLogger<MarkQuotationAsViewedCommandHandler>.Instance);

            var command = new MarkQuotationAsViewedCommand
            {
                AccessToken = quotation.Item2.AccessToken,
                IpAddress = "127.0.0.1"
            };

            var result = await handler.Handle(command);

            Assert.Equal(quotation.Item2.AccessLinkId, result.AccessLinkId);

            var updatedLink = await context.QuotationAccessLinks.FirstAsync(x => x.AccessLinkId == quotation.Item2.AccessLinkId);
            Assert.Equal(1, updatedLink.ViewCount);
            Assert.NotNull(updatedLink.FirstViewedAt);
            Assert.Equal(QuotationStatus.Viewed, quotation.Item1.Status);
        }

        [Fact]
        public async Task Handle_InvalidToken_Throws()
        {
            using var context = CreateContext();
            var handler = new MarkQuotationAsViewedCommandHandler(
                context,
                Options.Create(new CRM.Shared.Config.QuotationManagementSettings()),
                NullLogger<MarkQuotationAsViewedCommandHandler>.Instance);

            await Assert.ThrowsAsync<QuotationAccessLinkNotFoundException>(() =>
                handler.Handle(new MarkQuotationAsViewedCommand { AccessToken = "invalid" }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Quotation, QuotationAccessLink)> SeedQuotationWithAccessLink(AppDbContext context, QuotationStatus status)
        {
            var userId = Guid.NewGuid();

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "SalesRep"
            };
            context.Roles.Add(role);

            context.Users.Add(new User
            {
                UserId = userId,
                Email = "sales@example.com",
                FirstName = "Sales",
                LastName = "Rep",
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Client Co",
                Email = "client@example.com",
                StateCode = "27",
                CreatedByUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = userId,
                QuotationNumber = "QT-001",
                Status = status,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 100,
                TotalAmount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            quotation.LineItems.Add(new QuotationLineItem
            {
                LineItemId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ItemName = "Item",
                Quantity = 1,
                UnitRate = 100,
                Amount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            context.Quotations.Add(quotation);

            var link = new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                Quotation = quotation,
                ClientEmail = "client@example.com",
                AccessToken = "token",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            };
            context.QuotationAccessLinks.Add(link);

            await context.SaveChangesAsync();

            return (quotation, link);
        }
    }
}


