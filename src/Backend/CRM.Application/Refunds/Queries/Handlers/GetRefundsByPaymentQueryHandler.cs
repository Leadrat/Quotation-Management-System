using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetRefundsByPaymentQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetRefundsByPaymentQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<RefundDto>> Handle(GetRefundsByPaymentQuery query)
        {
            var refunds = await _db.Refunds
                .Include(r => r.RequestedByUser)
                .Include(r => r.ApprovedByUser)
                .Where(r => r.PaymentId == query.PaymentId)
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

