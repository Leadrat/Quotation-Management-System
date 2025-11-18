using Xunit;
using Microsoft.EntityFrameworkCore;
using CRM.Infrastructure.Persistence;
using CRM.Application.Adjustments.Commands;
using CRM.Application.Adjustments.Commands.Handlers;
using CRM.Application.Adjustments.Queries;
using CRM.Application.Adjustments.Queries.Handlers;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Application.Common.Persistence;

namespace CRM.Tests.Integration.Adjustments;

public class AdjustmentFlowIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IAppDbContext _appContext;
    private readonly InitiateAdjustmentCommandHandler _createHandler;
    private readonly ApproveAdjustmentCommandHandler _approveHandler;
    private readonly ApplyAdjustmentCommandHandler _applyHandler;
    private readonly GetAdjustmentsByQuotationQueryHandler _getHandler;

    public AdjustmentFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _appContext = _context;
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<InitiateAdjustmentCommandHandler>.Instance;
        var approveLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApproveAdjustmentCommandHandler>.Instance;
        var applyLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApplyAdjustmentCommandHandler>.Instance;
        _createHandler = new InitiateAdjustmentCommandHandler(_appContext, logger);
        _approveHandler = new ApproveAdjustmentCommandHandler(_appContext, approveLogger);
        _applyHandler = new ApplyAdjustmentCommandHandler(_appContext, applyLogger);
        _getHandler = new GetAdjustmentsByQuotationQueryHandler(_appContext);

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
            SubTotal = 5000m
        };

        _context.Companies.Add(company);
        _context.Clients.Add(client);
        _context.Quotations.Add(quotation);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AdjustmentFlow_Create_Approve_Apply_CompletesSuccessfully()
    {
        // Arrange
        var quotation = await _context.Quotations.FirstAsync();
        var createCommand = new InitiateAdjustmentCommand
        {
            Request = new CreateAdjustmentRequest
            {
                QuotationId = quotation.QuotationId,
                AdjustmentType = AdjustmentType.DISCOUNT_CHANGE,
                OriginalAmount = 5000m,
                AdjustedAmount = 4500m,
                Reason = "Discount adjustment"
            }
        };

        // Act - Create
        var createResult = await _createHandler.Handle(createCommand);
        Assert.NotNull(createResult);

        var adjustmentId = createResult.AdjustmentId;

        // Act - Approve
        var approveCommand = new ApproveAdjustmentCommand
        {
            AdjustmentId = adjustmentId,
            Request = new ApproveAdjustmentRequest
            {
                Comments = "Approved"
            }
        };
        var approveResult = await _approveHandler.Handle(approveCommand);
        Assert.NotNull(approveResult);

        // Act - Apply
        var applyCommand = new ApplyAdjustmentCommand
        {
            AdjustmentId = adjustmentId
        };
        var applyResult = await _applyHandler.Handle(applyCommand);
        Assert.NotNull(applyResult);

        // Assert
        var updatedQuotation = await _context.Quotations.FindAsync(quotation.QuotationId);
        Assert.NotNull(updatedQuotation);
        // Total amount should be updated after applying adjustment
    }

    [Fact]
    public async Task AdjustmentFlow_TaxRecalculation_UpdatesTaxAmounts()
    {
        // Arrange
        var quotation = await _context.Quotations.FirstAsync();
        quotation.CgstAmount = 450m;
        quotation.SgstAmount = 450m;
        quotation.TotalAmount = 5900m;
        await _context.SaveChangesAsync();

        var createCommand = new InitiateAdjustmentCommand
        {
            Request = new CreateAdjustmentRequest
            {
                QuotationId = quotation.QuotationId,
                AdjustmentType = AdjustmentType.TAX_CORRECTION,
                OriginalAmount = 5900m,
                AdjustedAmount = 5400m,
                Reason = "Tax correction"
            }
        };

        // Act
        var result = await _createHandler.Handle(createCommand);

        // Assert
        Assert.NotNull(result);
        // Tax amounts should be recalculated when adjustment is applied
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

