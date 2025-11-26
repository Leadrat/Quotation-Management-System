using System;
using System.Threading.Tasks;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public GetPaymentByIdQueryHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<PaymentDto?> Handle(GetPaymentByIdQuery query)
        {
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            var payment = await _db.Payments
                // .FirstOrDefaultAsync(p => p.PaymentId == query.PaymentId && p.TenantId == currentTenantId)
                .FirstOrDefaultAsync(p => p.PaymentId == query.PaymentId);

            if (payment == null)
                return null;

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                PaymentGateway = payment.PaymentGateway,
                PaymentReference = payment.PaymentReference,
                AmountPaid = payment.AmountPaid,
                Currency = payment.Currency,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                FailureReason = payment.FailureReason,
                IsRefundable = payment.IsRefundable,
                RefundAmount = payment.RefundAmount,
                RefundReason = payment.RefundReason,
                RefundDate = payment.RefundDate,
                CanBeRefunded = payment.CanBeRefunded(),
                CanBeCancelled = payment.CanBeCancelled()
            };
        }
    }
}

