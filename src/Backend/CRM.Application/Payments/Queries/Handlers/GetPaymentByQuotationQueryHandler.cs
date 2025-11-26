using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Queries.Handlers
{
    public class GetPaymentByQuotationQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;

        public GetPaymentByQuotationQueryHandler(IAppDbContext db, IMapper mapper, ITenantContext tenantContext)
        {
            _db = db;
            _mapper = mapper;
            _tenantContext = tenantContext;
        }

        public async Task<List<PaymentDto>> Handle(GetPaymentByQuotationQuery query)
        {
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            var payments = await _db.Payments
                // .Where(p => p.QuotationId == query.QuotationId && p.TenantId == currentTenantId)
                .Where(p => p.QuotationId == query.QuotationId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                QuotationId = p.QuotationId,
                PaymentGateway = p.PaymentGateway,
                PaymentReference = p.PaymentReference,
                AmountPaid = p.AmountPaid,
                Currency = p.Currency,
                PaymentStatus = p.PaymentStatus,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                FailureReason = p.FailureReason,
                IsRefundable = p.IsRefundable,
                RefundAmount = p.RefundAmount,
                RefundReason = p.RefundReason,
                RefundDate = p.RefundDate,
                CanBeRefunded = p.CanBeRefunded(),
                CanBeCancelled = p.CanBeCancelled()
            }).ToList();
        }
    }
}

