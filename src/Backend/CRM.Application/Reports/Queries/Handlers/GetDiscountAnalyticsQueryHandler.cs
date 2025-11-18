using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GetDiscountAnalyticsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetDiscountAnalyticsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<DiscountAnalyticsDto> Handle(GetDiscountAnalyticsQuery query)
        {
            var fromDate = query.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.UtcNow;

            var baseQuery = _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .Where(a => a.RequestDate >= fromDate && a.RequestDate <= toDate);

            // Filter by user or team
            if (query.UserId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestedByUserId == query.UserId.Value);
            }
            else if (query.TeamId.HasValue)
            {
                var teamUserIds = await _db.Users
                    .Where(u => u.ReportingManagerId == query.TeamId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                baseQuery = baseQuery.Where(a => teamUserIds.Contains(a.RequestedByUserId));
            }

            var approvals = await baseQuery.ToListAsync();

            // Average discount percent
            var averageDiscountPercent = approvals.Any()
                ? approvals.Average(a => (decimal)a.CurrentDiscountPercentage)
                : 0;

            // Approval rate
            var totalRequests = approvals.Count;
            var approvalRate = totalRequests > 0
                ? (decimal)approvals.Count(a => a.Status == Domain.Enums.ApprovalStatus.Approved) / totalRequests * 100
                : 0;

            // Margin impact (sum of discount amounts)
            var marginImpact = approvals
                .Where(a => a.Status == Domain.Enums.ApprovalStatus.Approved)
                .Sum(a => a.Quotation.DiscountAmount);

            // Discount by rep
            var discountByRep = await baseQuery
                .GroupBy(a => new { a.RequestedByUserId, a.RequestedByUser.FirstName, a.RequestedByUser.LastName })
                .Select(g => new DiscountByRepData
                {
                    UserId = g.Key.RequestedByUserId,
                    UserName = g.Key.FirstName + " " + g.Key.LastName,
                    AverageDiscount = g.Average(a => (decimal)a.CurrentDiscountPercentage),
                    RequestCount = g.Count()
                })
                .ToListAsync();

            // Approval rates
            var approvalRates = approvals
                .GroupBy(a => a.Status)
                .Select(g => new ApprovalRateData
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            var totalCount = approvalRates.Sum(a => a.Count);
            foreach (var item in approvalRates)
            {
                item.Percentage = totalCount > 0 ? (decimal)item.Count / totalCount * 100 : 0;
            }

            // Margin impact by period (monthly)
            var marginImpactByPeriod = approvals
                .Where(a => a.Status == Domain.Enums.ApprovalStatus.Approved)
                .GroupBy(a => new { a.ApprovalDate!.Value.Year, a.ApprovalDate!.Value.Month })
                .Select(g => new MarginImpactData
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalDiscountAmount = g.Sum(a => a.Quotation.DiscountAmount),
                    MarginImpact = g.Sum(a => a.Quotation.DiscountAmount)
                })
                .OrderBy(x => x.Period)
                .ToList();

            return new DiscountAnalyticsDto
            {
                AverageDiscountPercent = averageDiscountPercent,
                ApprovalRate = approvalRate,
                MarginImpact = marginImpact,
                DiscountByRep = discountByRep,
                ApprovalRates = approvalRates,
                MarginImpactByPeriod = marginImpactByPeriod
            };
        }
    }
}

