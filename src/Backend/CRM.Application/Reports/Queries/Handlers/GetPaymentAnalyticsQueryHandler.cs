using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GetPaymentAnalyticsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetPaymentAnalyticsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentAnalyticsDto> Handle(GetPaymentAnalyticsQuery query)
        {
            var fromDate = query.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.UtcNow;

            var baseQuery = _db.Payments
                .Include(p => p.Quotation)
                .ThenInclude(q => q.Client)
                .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate);

            // Apply filters
            if (query.Gateway.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.PaymentGateway == query.Gateway.Value.ToString());
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.PaymentStatus == query.Status.Value);
            }

            var payments = await baseQuery.ToListAsync();

            // Collection rate (successful payments / total payments)
            var totalPayments = payments.Count;
            var successfulPayments = payments.Count(p => p.PaymentStatus == PaymentStatus.Success);
            var collectionRate = totalPayments > 0
                ? (decimal)successfulPayments / totalPayments * 100
                : 0;

            // Failed payments count
            var failedPaymentsCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Failed);

            // Total refunds
            var totalRefunds = payments
                .Where(p => p.PaymentStatus == PaymentStatus.Refunded || 
                           p.PaymentStatus == PaymentStatus.PartiallyRefunded)
                .Sum(p => p.RefundAmount ?? 0);

            // Payment method distribution
            var paymentMethodDistribution = payments
                .GroupBy(p => p.PaymentGateway)
                .Select(g => new PaymentMethodDistributionData
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.AmountPaid),
                })
                .ToList();

            var totalAmount = paymentMethodDistribution.Sum(p => p.Amount);
            foreach (var item in paymentMethodDistribution)
            {
                item.Percentage = totalAmount > 0 ? item.Amount / totalAmount * 100 : 0;
            }

            // Payment status breakdown
            var paymentStatusBreakdown = payments
                .GroupBy(p => p.PaymentStatus)
                .Select(g => new PaymentStatusData
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Amount = g.Sum(p => p.AmountPaid)
                })
                .ToList();

            var totalStatusAmount = paymentStatusBreakdown.Sum(p => p.Amount);
            foreach (var item in paymentStatusBreakdown)
            {
                item.Percentage = totalStatusAmount > 0 ? item.Amount / totalStatusAmount * 100 : 0;
            }

            // Refunds
            var refunds = await baseQuery
                .Where(p => p.PaymentStatus == PaymentStatus.Refunded || 
                           p.PaymentStatus == PaymentStatus.PartiallyRefunded)
                .Select(p => new RefundData
                {
                    PaymentId = p.PaymentId,
                    QuotationId = p.QuotationId,
                    QuotationNumber = p.Quotation.QuotationNumber,
                    RefundAmount = p.RefundAmount ?? 0,
                    Reason = p.RefundReason ?? "N/A",
                    RefundDate = p.RefundDate ?? p.UpdatedAt
                })
                .ToListAsync();

            return new PaymentAnalyticsDto
            {
                CollectionRate = collectionRate,
                FailedPaymentsCount = failedPaymentsCount,
                TotalRefunds = totalRefunds,
                PaymentMethodDistribution = paymentMethodDistribution,
                PaymentStatusBreakdown = paymentStatusBreakdown,
                Refunds = refunds
            };
        }
    }
}

