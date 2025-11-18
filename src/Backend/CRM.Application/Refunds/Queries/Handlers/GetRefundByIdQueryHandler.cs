using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetRefundByIdQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetRefundByIdQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<RefundDto?> Handle(GetRefundByIdQuery query)
        {
            var refund = await _db.Refunds
                .Include(r => r.RequestedByUser)
                .Include(r => r.ApprovedByUser)
                .FirstOrDefaultAsync(r => r.RefundId == query.RefundId);

            if (refund == null)
                return null;

            return new RefundDto
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RefundAmount = refund.RefundAmount,
                RefundReason = refund.RefundReason,
                RefundReasonCode = refund.RefundReasonCode,
                RequestedByUserName = refund.RequestedByUser != null 
                    ? $"{refund.RequestedByUser.FirstName} {refund.RequestedByUser.LastName}" 
                    : string.Empty,
                ApprovedByUserName = refund.ApprovedByUser != null 
                    ? $"{refund.ApprovedByUser.FirstName} {refund.ApprovedByUser.LastName}" 
                    : null,
                RefundStatus = refund.RefundStatus,
                ApprovalLevel = refund.ApprovalLevel,
                Comments = refund.Comments,
                FailureReason = refund.FailureReason,
                RequestDate = refund.RequestDate,
                ApprovalDate = refund.ApprovalDate,
                CompletedDate = refund.CompletedDate,
                PaymentGatewayReference = refund.PaymentGatewayReference,
                ReversedReason = refund.ReversedReason,
                ReversedDate = refund.ReversedDate
            };
        }
    }
}

