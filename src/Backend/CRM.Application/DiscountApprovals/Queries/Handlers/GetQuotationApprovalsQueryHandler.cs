using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Queries.Handlers
{
    public class GetQuotationApprovalsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetQuotationApprovalsQueryHandler> _logger;

        public GetQuotationApprovalsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetQuotationApprovalsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DiscountApprovalDto>> Handle(GetQuotationApprovalsQuery query)
        {
            try
            {
                _logger.LogInformation("Getting all approvals for quotation {QuotationId}", query.QuotationId);

                // Use includes with null-safe handling
                var approvals = await _db.DiscountApprovals
                    .AsNoTracking()
                    .Where(a => a.QuotationId == query.QuotationId)
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .Include(a => a.ApproverUser)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} approvals for quotation {QuotationId}", approvals.Count, query.QuotationId);

                return approvals.Select(a => _mapper.Map<DiscountApprovalDto>(a)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approvals for quotation {QuotationId}: {Message}", query.QuotationId, ex.Message);
                throw;
            }
        }
    }
}

