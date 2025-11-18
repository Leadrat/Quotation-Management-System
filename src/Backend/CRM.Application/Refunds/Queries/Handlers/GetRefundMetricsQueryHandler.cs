using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetRefundMetricsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetRefundMetricsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<RefundMetricsDto> Handle(GetRefundMetricsQuery query)
        {
            var startDate = query.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = query.EndDate ?? DateTime.UtcNow;

            var refunds = await _db.Refunds
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToListAsync();

            var totalRefundAmount = refunds
                .Where(r => r.RefundStatus == RefundStatus.Completed)
                .Sum(r => r.RefundAmount);

            var totalRefundCount = refunds.Count;
            var pendingRefundCount = refunds.Count(r => r.RefundStatus == RefundStatus.Pending);
            var completedRefunds = refunds.Where(r => r.RefundStatus == RefundStatus.Completed).ToList();

            // Calculate refund percentage (refunded amount / total payment amount)
            var totalPaymentAmount = await _db.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.PaymentStatus == PaymentStatus.Success)
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            var refundPercentage = totalPaymentAmount > 0 
                ? (totalRefundAmount / totalPaymentAmount) * 100 
                : 0;

            var averageRefundAmount = completedRefunds.Any() 
                ? completedRefunds.Average(r => r.RefundAmount) 
                : 0;

            // Calculate average TAT (Time to Approval)
            var tatHours = refunds
                .Where(r => r.ApprovalDate.HasValue)
                .Select(r => (r.ApprovalDate!.Value - r.RequestDate).TotalHours)
                .ToList();

            var averageTAT = tatHours.Any() ? (decimal)tatHours.Average() : 0;

            // Reason breakdown
            var reasonBreakdown = refunds
                .GroupBy(r => r.RefundReasonCode)
                .Select(g => new RefundReasonBreakdown
                {
                    ReasonCode = g.Key.ToString(),
                    Count = g.Count(),
                    Amount = g.Where(r => r.RefundStatus == RefundStatus.Completed).Sum(r => r.RefundAmount),
                    Percentage = totalRefundCount > 0 ? (g.Count() * 100.0m / totalRefundCount) : 0
                })
                .ToList();

            // Status breakdown
            var statusBreakdown = refunds
                .GroupBy(r => r.RefundStatus)
                .Select(g => new RefundStatusBreakdown
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalRefundCount > 0 ? (g.Count() * 100.0m / totalRefundCount) : 0
                })
                .ToList();

            // TAT by period (weekly)
            var tatByPeriod = new List<RefundTATByPeriod>();
            var weeks = Enumerable.Range(0, (int)(endDate - startDate).TotalDays / 7 + 1)
                .Select(i => startDate.AddDays(i * 7))
                .ToList();

            foreach (var weekStart in weeks)
            {
                var weekEnd = weekStart.AddDays(7);
                var weekRefunds = refunds
                    .Where(r => r.RequestDate >= weekStart && r.RequestDate < weekEnd && r.ApprovalDate.HasValue)
                    .ToList();

                if (weekRefunds.Any())
                {
                    var weekTAT = weekRefunds
                        .Select(r => (r.ApprovalDate!.Value - r.RequestDate).TotalHours)
                        .Average();

                    tatByPeriod.Add(new RefundTATByPeriod
                    {
                        Period = weekStart.ToString("yyyy-MM-dd"),
                        AverageHours = (decimal)weekTAT
                    });
                }
            }

            return new RefundMetricsDto
            {
                TotalRefundAmount = totalRefundAmount,
                TotalRefundCount = totalRefundCount,
                PendingRefundCount = pendingRefundCount,
                RefundPercentage = refundPercentage,
                AverageRefundAmount = averageRefundAmount,
                AverageTAT = averageTAT,
                ReasonBreakdown = reasonBreakdown,
                StatusBreakdown = statusBreakdown,
                TATByPeriod = tatByPeriod
            };
        }
    }
}

