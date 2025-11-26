using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Services
{
    public class PaymentAggregationService
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public PaymentAggregationService(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<decimal> GetPaidTotalAsync(Guid quotationId, CancellationToken ct = default)
        {
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            return await _db.Payments
                .Where(p => p.QuotationId == quotationId 
                            // && p.TenantId == currentTenantId
                            && (p.PaymentStatus == PaymentStatus.Success 
                                || p.PaymentStatus == PaymentStatus.PartiallyRefunded 
                                || p.PaymentStatus == PaymentStatus.Refunded))
                .SumAsync(p => p.AmountPaid, ct);
        }

        public async Task<decimal> GetPaidTotalExcludingAsync(Guid quotationId, Guid excludePaymentId, CancellationToken ct = default)
        {
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            return await _db.Payments
                .Where(p => p.QuotationId == quotationId 
                            // && p.TenantId == currentTenantId
                            && p.PaymentId != excludePaymentId 
                            && (p.PaymentStatus == PaymentStatus.Success 
                                || p.PaymentStatus == PaymentStatus.PartiallyRefunded 
                                || p.PaymentStatus == PaymentStatus.Refunded))
                .SumAsync(p => p.AmountPaid, ct);
        }

        // Net paid = AmountPaid - RefundAmount (if any)
        public async Task<decimal> GetPaidNetTotalAsync(Guid quotationId, CancellationToken ct = default)
        {
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            return await _db.Payments
                .Where(p => p.QuotationId == quotationId 
                            && p.TenantId == currentTenantId
                            && (p.PaymentStatus == PaymentStatus.Success 
                                || p.PaymentStatus == PaymentStatus.PartiallyRefunded 
                                || p.PaymentStatus == PaymentStatus.Refunded))
                .SumAsync(p => p.AmountPaid - (p.RefundAmount ?? 0m), ct);
        }

        public async Task<decimal> GetPaidNetTotalExcludingAsync(Guid quotationId, Guid excludePaymentId, CancellationToken ct = default)
        {
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            return await _db.Payments
                .Where(p => p.QuotationId == quotationId 
                            && p.TenantId == currentTenantId
                            && p.PaymentId != excludePaymentId 
                            && (p.PaymentStatus == PaymentStatus.Success 
                                || p.PaymentStatus == PaymentStatus.PartiallyRefunded 
                                || p.PaymentStatus == PaymentStatus.Refunded))
                .SumAsync(p => p.AmountPaid - (p.RefundAmount ?? 0m), ct);
        }
    }
}
