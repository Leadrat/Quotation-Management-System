using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Mapping;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Queries;
using CRM.Application.Quotations.Queries.Handlers;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class GetQuotationByAccessTokenQueryHandlerTests
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
        public async Task Handle_ReturnsPublicQuotation_WhenLinkValid()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, token) = await SeedQuotationWithLink(context);

            var handler = new GetQuotationByAccessTokenQueryHandler(context, mapper);
            var result = await handler.Handle(new GetQuotationByAccessTokenQuery
            {
                QuotationId = quotationId,
                AccessToken = token
            });

            Assert.Equal(quotationId, result.QuotationId);
            Assert.Equal("Client Co", result.ClientName);
            Assert.Single(result.LineItems);
        }

        [Fact]
        public async Task Handle_Throws_WhenTokenInvalid()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var handler = new GetQuotationByAccessTokenQueryHandler(context, mapper);

            await Assert.ThrowsAsync<QuotationAccessLinkNotFoundException>(() => handler.Handle(new GetQuotationByAccessTokenQuery
            {
                QuotationId = Guid.NewGuid(),
                AccessToken = "missing"
            }));
        }

        [Fact]
        public async Task Handle_Throws_WhenLinkExpired()
        {
            using var context = CreateContext();
            var mapper = CreateMapper();
            var (quotationId, token) = await SeedQuotationWithLink(context, expired: true);

            var handler = new GetQuotationByAccessTokenQueryHandler(context, mapper);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new GetQuotationByAccessTokenQuery
            {
                QuotationId = quotationId,
                AccessToken = token
            }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Guid quotationId, string token)> SeedQuotationWithLink(AppDbContext context, bool expired = false)
        {
            var quotationId = Guid.NewGuid();
            var token = Guid.NewGuid().ToString("N");

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
                QuotationId = quotationId,
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = client.CreatedByUserId,
                QuotationNumber = "QT-777",
                Status = QuotationStatus.Sent,
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
                QuotationId = quotationId,
                ItemName = "Service",
                Quantity = 1,
                UnitRate = 100,
                Amount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            context.Quotations.Add(quotation);

            context.QuotationAccessLinks.Add(new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotationId,
                Quotation = quotation,
                ClientEmail = "client@example.com",
                AccessToken = token,
                IsActive = !expired,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ExpiresAt = expired ? DateTimeOffset.UtcNow.AddHours(-1) : DateTimeOffset.UtcNow.AddDays(10)
            });

            await context.SaveChangesAsync();
            return (quotationId, token);
        }
    }
}


