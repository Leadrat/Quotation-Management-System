using Xunit;
using Moq;
using CRM.Application.Refunds.Commands;
using CRM.Application.Refunds.Commands.Handlers;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Tests.Unit.Refunds;

public class InitiateRefundCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<ILogger<InitiateRefundCommandHandler>> _mockLogger;
    private readonly InitiateRefundCommandHandler _handler;

    public InitiateRefundCommandHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _mockLogger = new Mock<ILogger<InitiateRefundCommandHandler>>();
        _handler = new InitiateRefundCommandHandler(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesRefund()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var quotationId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            Request = new CreateRefundRequest
            {
                PaymentId = paymentId,
                QuotationId = quotationId,
                RefundAmount = 1000m,
                RefundReason = "Client request",
                RefundReasonCode = RefundReasonCode.CLIENT_REQUEST
            }
        };

        var payment = new Payment
        {
            PaymentId = paymentId,
            QuotationId = quotationId,
            PaymentAmount = 5000m,
            PaymentStatus = PaymentStatus.Success,
            RefundedAmount = 0m
        };

        var payments = new List<Payment> { payment }.AsQueryable().BuildMockDbSet();
        var refunds = new List<Refund>().AsQueryable().BuildMockDbSet();

        _mockContext.Setup(c => c.Payments).Returns(payments);
        _mockContext.Setup(c => c.Refunds).Returns(refunds);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.RefundAmount);
        _mockContext.Verify(c => c.Refunds.Add(It.IsAny<Refund>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// Extension method for building mock DbSet
public static class QueryableExtensions
{
    public static DbSet<T> BuildMockDbSet<T>(this IQueryable<T> queryable) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(queryable.ToList().Add);
        return mockSet.Object;
    }
}

