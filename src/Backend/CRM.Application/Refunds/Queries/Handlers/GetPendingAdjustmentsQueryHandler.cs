using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetPendingAdjustmentsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPendingAdjustmentsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdjustmentDto>> Handle(GetPendingAdjustmentsQuery query)
        {
            var adjustmentsQuery = _db.Adjustments
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApprovedByUser)
                .Where(a => a.Status == AdjustmentStatus.PENDING);

            if (!string.IsNullOrEmpty(query.ApprovalLevel))
            {
                adjustmentsQuery = adjustmentsQuery.Where(a => a.ApprovalLevel == query.ApprovalLevel);
            }

            var adjustments = await adjustmentsQuery
                .OrderByDescending(a => a.RequestDate)
                .ToListAsync();

            return adjustments.Select(a => new AdjustmentDto
            {
                AdjustmentId = a.AdjustmentId,
                QuotationId = a.QuotationId,
                AdjustmentType = a.AdjustmentType,
                OriginalAmount = a.OriginalAmount,
                AdjustedAmount = a.AdjustedAmount,
                Reason = a.Reason,
                RequestedByUserName = a.RequestedByUser != null 
                    ? $"{a.RequestedByUser.FirstName} {a.RequestedByUser.LastName}" 
                    : string.Empty,
                ApprovedByUserName = a.ApprovedByUser != null 
                    ? $"{a.ApprovedByUser.FirstName} {a.ApprovedByUser.LastName}" 
                    : null,
                Status = a.Status,
                ApprovalLevel = a.ApprovalLevel,
                RequestDate = a.RequestDate,
                ApprovalDate = a.ApprovalDate,
                AppliedDate = a.AppliedDate,
                AdjustmentDifference = a.GetAdjustmentDifference()
            }).ToList();
        }
    }
}

