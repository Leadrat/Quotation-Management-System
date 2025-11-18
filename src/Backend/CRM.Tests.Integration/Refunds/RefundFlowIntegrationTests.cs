using Xunit;
using Microsoft.EntityFrameworkCore;
using CRM.Infrastructure.Persistence;
using CRM.Application.Refunds.Commands;
using CRM.Application.Refunds.Commands.Handlers;
using CRM.Application.Refunds.Queries;
using CRM.Application.Refunds.Queries.Handlers;
using CRM.Application.Payments.Commands;
using CRM.Application.Payments.Commands.Handlers;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Application.Common.Persistence;

namespace CRM.Tests.Integration.Refunds;

public class RefundFlowIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IAppDbContext _appContext;
    private readonly InitiateRefundCommandHandler _initiateHandler;
    private readonly ApproveRefundCommandHandler _approveHandler;
    private readonly GetRefundByIdQueryHandler _getHandler;

    public RefundFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _appContext = _context;
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<InitiateRefundCommandHandler>.Instance;
        var approveLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApproveRefundCommandHandler>.Instance;
        _initiateHandler = new InitiateRefundCommandHandler(_appContext, logger);
        _approveHandler = new ApproveRefundCommandHandler(_appContext, approveLogger);
        _getHandler = new GetRefundByIdQueryHandler(_appContext);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var company = new Company { CompanyId = Guid.NewGuid(), CompanyName = "Test Company" };
        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            CompanyId = company.CompanyId,
            ClientName = "Test Client",
            ClientEmail = "test@example.com"
        };
        var quotation = new Quotation
        {
            QuotationId = Guid.NewGuid(),
            CompanyId = company.CompanyId,
            ClientId = client.ClientId,
            QuotationNumber = "Q-001",
            TotalAmount = 5000m,
            Status = QuotationStatus.ACCEPTED
        };
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            QuotationId = quotation.QuotationId,
            PaymentAmount = 5000m,
            PaymentStatus = PaymentStatus.Success,
            RefundedAmount = 0m
        };

        _context.Companies.Add(company);
        _context.Clients.Add(client);
        _context.Quotations.Add(quotation);
        _context.Payments.Add(payment);
        _context.SaveChanges();
    }

    [Fact]
    public async Task RefundFlow_Initiate_Approve_CompletesSuccessfully()
    {
        // Arrange
        var payment = await _context.Payments.FirstAsync();
        var userId = Guid.NewGuid();
        var initiateCommand = new InitiateRefundCommand
        {
            Request = new CreateRefundRequest
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                RefundAmount = 1000m,
                RefundReason = "Test refund",
                RefundReasonCode = RefundReasonCode.CLIENT_REQUEST
            },
            RequestedByUserId = userId
        };

        // Act - Initiate
        var initiateResult = await _initiateHandler.Handle(initiateCommand);
        Assert.NotNull(initiateResult);

        var refundId = initiateResult.RefundId;

        // Act - Approve
        var approveCommand = new ApproveRefundCommand
        {
            RefundId = refundId,
            Request = new ApproveRefundRequest
            {
                Comments = "Approved for testing"
            },
            ApprovedByUserId = userId
        };
        var approveResult = await _approveHandler.Handle(approveCommand);
        Assert.NotNull(approveResult);

        // Act - Get refund
        var getQuery = new GetRefundByIdQuery { RefundId = refundId };
        var getResult = await _getHandler.Handle(getQuery);

        // Assert
        Assert.NotNull(getResult);
        Assert.Equal(RefundStatus.Approved, getResult.RefundStatus);
        Assert.Equal(1000m, getResult.RefundAmount);
    }

    [Fact]
    public async Task RefundFlow_PartialRefund_CreatesRefund()
    {
        // Arrange
        var payment = await _context.Payments.FirstAsync();
        var refundAmount = 2000m;

        var initiateCommand = new InitiateRefundCommand
        {
            Request = new CreateRefundRequest
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                RefundAmount = refundAmount,
                RefundReason = "Partial refund",
                RefundReasonCode = RefundReasonCode.CLIENT_REQUEST
            }
        };

        // Act
        var result = await _initiateHandler.Handle(initiateCommand);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(refundAmount, result.RefundAmount);
        var updatedPayment = await _context.Payments.FindAsync(payment.PaymentId);
        Assert.NotNull(updatedPayment);
        // Note: RefundedAmount update happens in ProcessRefund, not Initiate
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

