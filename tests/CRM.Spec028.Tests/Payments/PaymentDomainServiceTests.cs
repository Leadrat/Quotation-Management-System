using System;
using System.Threading.Tasks;
using CRM.Application.Payments.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Spec028.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Spec028.Tests.Payments
{
    public class PaymentDomainServiceTests
    {
        private TestAppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<TestAppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new TestAppDbContext(options);
        }

        [Fact]
        public async Task ValidateManualPayment_ShouldBlock_WhenAmountExceedsOutstanding()
        {
            // Arrange
            var db = CreateDb(nameof(ValidateManualPayment_ShouldBlock_WhenAmountExceedsOutstanding));
            var quotationId = Guid.NewGuid();
            db.Quotations.Add(new Quotation { QuotationId = quotationId, TotalAmount = 1000m, SubTotal = 1000m, DiscountAmount = 0m });
            db.Payments.Add(new Payment
            {
                PaymentId = Guid.NewGuid(),
                QuotationId = quotationId,
                PaymentGateway = "Manual",
                PaymentReference = Guid.NewGuid().ToString(),
                AmountPaid = 900m,
                Currency = "INR",
                PaymentStatus = PaymentStatus.Success,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            var agg = new PaymentAggregationService(db);
            var domain = new PaymentDomainService(db, agg);

            // Act
            var (ok, error) = await domain.ValidateManualPaymentAsync(quotationId, 200m);

            // Assert
            Assert.False(ok);
            Assert.Equal("Amount exceeds outstanding balance", error);
        }

        [Fact]
        public async Task ValidateManualPaymentUpdate_Allows_IncreaseWithinOutstanding()
        {
            var db = CreateDb(nameof(ValidateManualPaymentUpdate_Allows_IncreaseWithinOutstanding));
            var quotationId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            db.Quotations.Add(new Quotation { QuotationId = quotationId, TotalAmount = 1000m, SubTotal = 1000m, DiscountAmount = 0m });
            db.Payments.Add(new Payment
            {
                PaymentId = paymentId,
                QuotationId = quotationId,
                PaymentGateway = "Manual",
                PaymentReference = Guid.NewGuid().ToString(),
                AmountPaid = 200m,
                Currency = "INR",
                PaymentStatus = PaymentStatus.Success,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            var agg = new PaymentAggregationService(db);
            var domain = new PaymentDomainService(db, agg);

            var (ok, error) = await domain.ValidateManualPaymentUpdateAsync(paymentId, 400m);

            Assert.True(ok);
            Assert.Null(error);
        }
    }
}
