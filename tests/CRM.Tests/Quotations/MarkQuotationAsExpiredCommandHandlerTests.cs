using System;
using System.Threading.Tasks;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class MarkQuotationAsExpiredCommandHandlerTests
    {
        [Fact]
        public async Task Handle_MarksDraftQuotationAsExpired()
        {
            using var context = CreateContext();
            var quotation = await SeedQuotation(context, QuotationStatus.Sent);

            var handler = new MarkQuotationAsExpiredCommandHandler(context);

            await handler.Handle(new MarkQuotationAsExpiredCommand
            {
                QuotationId = quotation.QuotationId,
                Reason = "Auto-expire"
            });

            var updated = await context.Quotations.FirstAsync(q => q.QuotationId == quotation.QuotationId);
            Assert.Equal(QuotationStatus.Expired, updated.Status);
        }

        [Fact]
        public async Task Handle_InvalidId_Throws()
        {
            using var context = CreateContext();
            var handler = new MarkQuotationAsExpiredCommandHandler(context);

            await Assert.ThrowsAsync<QuotationNotFoundException>(() =>
                handler.Handle(new MarkQuotationAsExpiredCommand { QuotationId = Guid.NewGuid() }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<Quotation> SeedQuotation(AppDbContext context, QuotationStatus status)
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Client Co",
                Email = "client@example.com",
                StateCode = "27",
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = client.CreatedByUserId,
                QuotationNumber = "QT-001",
                Status = status,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today,
                SubTotal = 100,
                TotalAmount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Quotations.Add(quotation);

            await context.SaveChangesAsync();
            return quotation;
        }
    }
}


