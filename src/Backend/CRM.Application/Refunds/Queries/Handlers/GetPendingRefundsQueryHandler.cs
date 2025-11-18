using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetPendingRefundsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPendingRefundsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<RefundDto>> Handle(GetPendingRefundsQuery query)
        {
            var refundsQuery = _db.Refunds
                .Include(r => r.RequestedByUser)
                .Include(r => r.ApprovedByUser)
                .Where(r => r.RefundStatus == RefundStatus.Pending);

            if (!string.IsNullOrEmpty(query.ApprovalLevel))
            {
                refundsQuery = refundsQuery.Where(r => r.ApprovalLevel == query.ApprovalLevel);
            }

            var refunds = await refundsQuery
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return refunds.Select(r => new RefundDto
            {
                RefundId = r.RefundId,
                PaymentId = r.PaymentId,
                QuotationId = r.QuotationId,
                RefundAmount = r.RefundAmount,
                RefundReason = r.RefundReason,
                RefundReasonCode = r.RefundReasonCode,
                RequestedByUserName = r.RequestedByUser != null 
                    ? $"{r.RequestedByUser.FirstName} {r.RequestedByUser.LastName}" 
                    : string.Empty,
                ApprovedByUserName = r.ApprovedByUser != null 
                    ? $"{r.ApprovedByUser.FirstName} {r.ApprovedByUser.LastName}" 
                    : null,
                RefundStatus = r.RefundStatus,
                ApprovalLevel = r.ApprovalLevel,
                Comments = r.Comments,
                FailureReason = r.FailureReason,
                RequestDate = r.RequestDate,
                ApprovalDate = r.ApprovalDate,
                CompletedDate = r.CompletedDate,
                PaymentGatewayReference = r.PaymentGatewayReference,
                ReversedReason = r.ReversedReason,
                ReversedDate = r.ReversedDate
            }).ToList();
        }
    }
}

