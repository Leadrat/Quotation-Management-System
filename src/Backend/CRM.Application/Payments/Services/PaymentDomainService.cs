using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Payments.Services
{
    public class PaymentDomainService
    {
        private readonly IAppDbContext _db;
        private readonly PaymentAggregationService _agg;
        private readonly ITenantContext _tenantContext;

        public PaymentDomainService(IAppDbContext db, PaymentAggregationService agg, ITenantContext tenantContext)
        {
            _db = db;
            _agg = agg;
            _tenantContext = tenantContext;
        }

        public async Task<decimal> GetOutstandingAsync(Guid quotationId, CancellationToken ct = default)
        {
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            var quotation = await _db.Quotations.AsNoTracking().FirstOrDefaultAsync(q => q.QuotationId == quotationId && q.TenantId == currentTenantId, ct);
            if (quotation == null) throw new InvalidOperationException("Quotation not found");
            var paidNet = await _agg.GetPaidNetTotalAsync(quotationId, ct);
            var outstanding = quotation.TotalAmount - paidNet;
            return outstanding < 0 ? 0 : outstanding;
        }

        public async Task<(bool ok, string? error)> ValidateManualPaymentAsync(Guid quotationId, decimal newAmount, CancellationToken ct = default)
        {
            if (newAmount <= 0) return (false, "Amount must be greater than zero");
            var outstanding = await GetOutstandingAsync(quotationId, ct);
            if (newAmount > outstanding)
            {
                return (false, "Amount exceeds outstanding balance");
            }
            return (true, null);
        }

        public async Task<(bool ok, string? error)> ValidateManualPaymentUpdateAsync(Guid paymentId, decimal newAmount, CancellationToken ct = default)
        {
            if (newAmount <= 0) return (false, "Amount must be greater than zero");
            // Re-enable tenant filtering with correct tenant ID
            var currentTenantId = _tenantContext.CurrentTenantId;
            var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.TenantId == currentTenantId, ct);
            if (payment == null) return (false, "Payment not found");
            var quotation = await _db.Quotations.AsNoTracking().FirstOrDefaultAsync(q => q.QuotationId == payment.QuotationId && q.TenantId == currentTenantId, ct);
            if (quotation == null) return (false, "Quotation not found");

            var paidExcluding = await _agg.GetPaidNetTotalExcludingAsync(payment.QuotationId, paymentId, ct);
            var outstanding = quotation.TotalAmount - paidExcluding;
            if (newAmount > outstanding)
            {
                return (false, "Amount exceeds outstanding balance");
            }
            return (true, null);
        }
    }
}
