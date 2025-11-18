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

public class ApproveRefundCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<ILogger<ApproveRefundCommandHandler>> _mockLogger;
    private readonly ApproveRefundCommandHandler _handler;

    public ApproveRefundCommandHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _mockLogger = new Mock<ILogger<ApproveRefundCommandHandler>>();
        _handler = new ApproveRefundCommandHandler(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidApproval_UpdatesRefundStatus()
    {
        // Arrange
        var refundId = Guid.NewGuid();
        var command = new ApproveRefundCommand
        {
            RefundId = refundId,
            Request = new ApproveRefundRequest
            {
                Comments = "Approved"
            }
        };

        var refund = new Refund
        {
            RefundId = refundId,
            RefundStatus = RefundStatus.Pending,
            RefundAmount = 1000m
        };

        var refunds = new List<Refund> { refund }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Refunds).Returns(refunds);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RefundStatus.Approved, refund.RefundStatus);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

