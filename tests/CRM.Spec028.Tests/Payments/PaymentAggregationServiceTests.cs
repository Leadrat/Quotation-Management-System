using System;
using System.Threading.Tasks;
using CRM.Application.Payments.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Spec028.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Spec028.Tests.Payments
{
    public class PaymentAggregationServiceTests
    {
        private TestAppDbContext CreateDb(string name)
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            var provider = services.BuildServiceProvider();
            var options = new DbContextOptionsBuilder<TestAppDbContext>()
                .UseInMemoryDatabase(name)
                .UseInternalServiceProvider(provider)
                .Options;
            return new TestAppDbContext(options);
        }

        [Fact]
        public async Task GetPaidTotals_ShouldComputeGrossAndNet_WithRefunds()
        {
            // Arrange
            var db = CreateDb(nameof(GetPaidTotals_ShouldComputeGrossAndNet_WithRefunds));
            var quotationId = Guid.NewGuid();

            db.Quotations.Add(new Quotation
            {
                QuotationId = quotationId,
                TotalAmount = 10_000m,
                SubTotal = 10_000m,
                DiscountAmount = 0m
            });

            db.Payments.AddRange(
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    QuotationId = quotationId,
                    PaymentGateway = "Manual",
                    PaymentReference = Guid.NewGuid().ToString(),
                    AmountPaid = 1000m,
                    Currency = "INR",
                    PaymentStatus = PaymentStatus.Success,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    QuotationId = quotationId,
                    PaymentGateway = "Manual",
                    PaymentReference = Guid.NewGuid().ToString(),
                    AmountPaid = 500m,
                    RefundAmount = 500m,
                    Currency = "INR",
                    PaymentStatus = PaymentStatus.Refunded,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    QuotationId = quotationId,
                    PaymentGateway = "Manual",
                    PaymentReference = Guid.NewGuid().ToString(),
                    AmountPaid = 300m,
                    RefundAmount = 100m,
                    Currency = "INR",
                    PaymentStatus = PaymentStatus.PartiallyRefunded,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                }
            );
            await db.SaveChangesAsync();

            var agg = new PaymentAggregationService(db);

            // Act
            var gross = await agg.GetPaidTotalAsync(quotationId);
            var net = await agg.GetPaidNetTotalAsync(quotationId);

            // Assert
            Assert.Equal(1800m, gross);
            Assert.Equal(700m, net); // 1000 + (500-500) + (300-100)
        }
    }
}
