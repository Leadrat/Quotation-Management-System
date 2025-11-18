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
    public class GetQuotationResponseQueryHandlerTests
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
        public async Task Handle_ReturnsResponse_WhenExists()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, ownerId) = await SeedQuotationWithResponse(context);

            var handler = new GetQuotationResponseQueryHandler(context, mapper);
            var result = await handler.Handle(new GetQuotationResponseQuery
            {
                QuotationId = quotationId,
                RequestorUserId = ownerId,
                RequestorRole = "SalesRep"
            });

            Assert.NotNull(result);
            Assert.Equal("ACCEPTED", result!.ResponseType);
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNoResponse()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, ownerId) = await SeedQuotation(context);

            var handler = new GetQuotationResponseQueryHandler(context, mapper);
            var result = await handler.Handle(new GetQuotationResponseQuery
            {
                QuotationId = quotationId,
                RequestorUserId = ownerId,
                RequestorRole = "SalesRep"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ThrowsUnauthorized_ForDifferentUser()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, _) = await SeedQuotation(context);

            var handler = new GetQuotationResponseQueryHandler(context, mapper);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new GetQuotationResponseQuery
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

        private static async Task<(Guid quotationId, Guid ownerId)> SeedQuotationWithResponse(AppDbContext context)
        {
            var (quotationId, ownerId) = await SeedQuotation(context);

            context.QuotationResponses.Add(new QuotationResponse
            {
                ResponseId = Guid.NewGuid(),
                QuotationId = quotationId,
                ResponseType = "ACCEPTED",
                ClientEmail = "client@example.com",
                ResponseDate = DateTimeOffset.UtcNow
            });

            await context.SaveChangesAsync();
            return (quotationId, ownerId);
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
                QuotationNumber = "QT-1002",
                Status = QuotationStatus.Sent,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            await context.SaveChangesAsync();
            return (quotationId, ownerId);
        }
    }
}


