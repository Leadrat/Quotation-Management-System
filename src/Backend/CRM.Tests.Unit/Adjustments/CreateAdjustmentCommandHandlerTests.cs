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

namespace CRM.Tests.Unit.Adjustments;

public class CreateAdjustmentCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<ILogger<InitiateAdjustmentCommandHandler>> _mockLogger;
    private readonly InitiateAdjustmentCommandHandler _handler;

    public CreateAdjustmentCommandHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _mockLogger = new Mock<ILogger<InitiateAdjustmentCommandHandler>>();
        _handler = new InitiateAdjustmentCommandHandler(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesAdjustment()
    {
        // Arrange
        var quotationId = Guid.NewGuid();
        var command = new InitiateAdjustmentCommand
        {
            Request = new CreateAdjustmentRequest
            {
                QuotationId = quotationId,
                AdjustmentType = AdjustmentType.AMOUNT_CORRECTION,
                OriginalAmount = 5000m,
                AdjustedAmount = 4500m,
                Reason = "Discount adjustment"
            }
        };

        var quotation = new Quotation
        {
            QuotationId = quotationId,
            TotalAmount = 5000m
        };

        var quotations = new List<Quotation> { quotation }.AsQueryable().BuildMockDbSet();
        var adjustments = new List<Adjustment>().AsQueryable().BuildMockDbSet();

        _mockContext.Setup(c => c.Quotations).Returns(quotations);
        _mockContext.Setup(c => c.Adjustments).Returns(adjustments);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AdjustmentType.AMOUNT_CORRECTION, result.AdjustmentType);
        _mockContext.Verify(c => c.Adjustments.Add(It.IsAny<Adjustment>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

