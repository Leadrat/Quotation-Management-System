using System;
using System.Threading.Tasks;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Queries;
using CRM.Application.Quotations.Queries.Handlers;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class GetQuotationAccessLinkQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsLatestLink_WithViewUrl()
        {
            using var context = CreateContext();
            var settings = Options.Create(new QuotationManagementSettings { BaseUrl = "https://app.test" });
            var (quotationId, ownerId) = await SeedQuotationWithLinks(context);

            var handler = new GetQuotationAccessLinkQueryHandler(context, settings);
            var result = await handler.Handle(new GetQuotationAccessLinkQuery
            {
                QuotationId = quotationId,
                RequestorUserId = ownerId,
                RequestorRole = "SalesRep"
            });

            Assert.NotNull(result);
            Assert.Contains("https://app.test", result!.ViewUrl);
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNoLink()
        {
            using var context = CreateContext();
            var settings = Options.Create(new QuotationManagementSettings());
            var (quotationId, ownerId) = await SeedQuotation(context);

            var handler = new GetQuotationAccessLinkQueryHandler(context, settings);
            var result = await handler.Handle(new GetQuotationAccessLinkQuery
            {
                QuotationId = quotationId,
                RequestorUserId = ownerId,
                RequestorRole = "SalesRep"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ThrowsUnauthorized_ForDifferentSalesRep()
        {
            using var context = CreateContext();
            var settings = Options.Create(new QuotationManagementSettings());
            var (quotationId, _) = await SeedQuotation(context);

            var handler = new GetQuotationAccessLinkQueryHandler(context, settings);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new GetQuotationAccessLinkQuery
            {
                QuotationId = quotationId,
                RequestorUserId = Guid.NewGuid(),
                RequestorRole = "SalesRep"
            }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Guid quotationId, Guid ownerId)> SeedQuotation(AppDbContext context)
        {
            var ownerId = Guid.NewGuid();
            var quotationId = Guid.NewGuid();

            context.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                ClientId = Guid.NewGuid(),
                CreatedByUserId = ownerId,
                QuotationNumber = "QT-2001",
                Status = QuotationStatus.Sent,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            await context.SaveChangesAsync();
            return (quotationId, ownerId);
        }

        private static async Task<(Guid quotationId, Guid ownerId)> SeedQuotationWithLinks(AppDbContext context)
        {
            var (quotationId, ownerId) = await SeedQuotation(context);

            context.QuotationAccessLinks.Add(new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotationId,
                ClientEmail = "client@example.com",
                AccessToken = "token-old",
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });

            context.QuotationAccessLinks.Add(new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotationId,
                ClientEmail = "client@example.com",
                AccessToken = "token-new",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await context.SaveChangesAsync();
            return (quotationId, ownerId);
        }
    }
}


