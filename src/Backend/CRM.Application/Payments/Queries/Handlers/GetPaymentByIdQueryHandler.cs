using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentByIdQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPaymentByIdQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentDto?> Handle(GetPaymentByIdQuery query)
        {
            var payment = await _db.Payments
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

