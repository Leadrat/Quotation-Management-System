using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Queries;
using CRM.Application.Quotations.Queries.Handlers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class GetQuotationStatusHistoryQueryHandlerTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<QuotationManagementProfile>();
                cfg.AddProfile<QuotationProfile>();
            });
            return config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ReturnsHistory_ForOwner()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, ownerId) = await SeedQuotationWithHistory(context);

            var handler = new GetQuotationStatusHistoryQueryHandler(context, mapper);
            var result = await handler.Handle(new GetQuotationStatusHistoryQuery
            {
                QuotationId = quotationId,
                RequestorUserId = ownerId,
                RequestorRole = "SalesRep"
            });

            Assert.Equal(2, result.Count);
            Assert.True(result[0].ChangedAt >= result[1].ChangedAt);
        }

        [Fact]
        public async Task Handle_ThrowsUnauthorized_ForDifferentSalesRep()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, _) = await SeedQuotationWithHistory(context);

            var handler = new GetQuotationStatusHistoryQueryHandler(context, mapper);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new GetQuotationStatusHistoryQuery
            {
                QuotationId = quotationId,
                RequestorUserId = Guid.NewGuid(),
                RequestorRole = "SalesRep"
            }));
        }

        [Fact]
        public async Task Handle_ThrowsNotFound_ForMissingQuotation()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var handler = new GetQuotationStatusHistoryQueryHandler(context, mapper);

            await Assert.ThrowsAsync<QuotationNotFoundException>(() => handler.Handle(new GetQuotationStatusHistoryQuery
            {
                QuotationId = Guid.NewGuid(),
                RequestorUserId = Guid.NewGuid(),
                RequestorRole = "Admin"
            }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Guid quotationId, Guid ownerId)> SeedQuotationWithHistory(AppDbContext context)
        {
            var ownerId = Guid.NewGuid();
            var quotationId = Guid.NewGuid();

            context.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                ClientId = Guid.NewGuid(),
                CreatedByUserId = ownerId,
                QuotationNumber = "QT-1001",
                Status = QuotationStatus.Sent,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            context.Users.Add(new User
            {
                UserId = ownerId,
                Email = "owner@example.com",
                FirstName = "Owner",
                LastName = "User",
                RoleId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });

            context.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotationId,
                PreviousStatus = QuotationStatus.Draft.ToString(),
                NewStatus = QuotationStatus.Sent.ToString(),
                ChangedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                ChangedByUserId = ownerId
            });

            context.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotationId,
                PreviousStatus = QuotationStatus.Sent.ToString(),
                NewStatus = QuotationStatus.Viewed.ToString(),
                ChangedAt = DateTimeOffset.UtcNow,
                ChangedByUserId = ownerId
            });

            await context.SaveChangesAsync();
            return (quotationId, ownerId);
        }
    }
}


