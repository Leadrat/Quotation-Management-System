using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GetSalesDashboardMetricsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetSalesDashboardMetricsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<SalesDashboardMetricsDto> Handle(GetSalesDashboardMetricsQuery query)
        {
            var now = DateTimeOffset.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var fromDate = query.FromDate ?? startOfMonth;
            var toDate = query.ToDate ?? endOfMonth;

            // Base query for user's quotations
            var quotationsQuery = _db.Quotations
                .Include(q => q.Client)
                .Where(q => q.CreatedByUserId == query.UserId);

            // Quotations created this month
            var quotationsCreatedThisMonth = await quotationsQuery
                .Where(q => q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                .CountAsync();

            // Quotations sent this month
            var quotationsSentThisMonth = await quotationsQuery
                .Where(q => q.Status == QuotationStatus.Sent && 
                           q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                .CountAsync();

            // Quotations accepted this month
            var quotationsAcceptedThisMonth = await quotationsQuery
                .Where(q => q.Status == QuotationStatus.Accepted && 
                           q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                .CountAsync();

            // Conversion rate (accepted / sent)
            var conversionRate = quotationsSentThisMonth > 0
                ? (decimal)quotationsAcceptedThisMonth / quotationsSentThisMonth * 100
                : 0;

            // Total pipeline value (open quotations: Draft, Sent, Viewed)
            var totalPipelineValue = await quotationsQuery
                .Where(q => q.Status == QuotationStatus.Draft || 
                           q.Status == QuotationStatus.Sent || 
                           q.Status == QuotationStatus.Viewed)
                .SumAsync(q => q.TotalAmount);

            // Pending approvals
            var pendingApprovals = await _db.DiscountApprovals
                .Where(a => a.RequestedByUserId == query.UserId && 
                           a.Status == Domain.Enums.ApprovalStatus.Pending)
                .CountAsync();

            // Quotation trend (last 30 days)
            var trendStartDate = now.AddDays(-30);
            var trendData = await quotationsQuery
                .Where(q => q.CreatedAt >= trendStartDate)
                .GroupBy(q => q.CreatedAt.Date)
                .Select(g => new QuotationTrendData
                {
                    Date = g.Key,
                    Created = g.Count(),
                    Sent = g.Count(q => q.Status == QuotationStatus.Sent)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Status breakdown
            var statusBreakdown = await quotationsQuery
                .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate)
                .GroupBy(q => q.Status)
                .Select(g => new StatusBreakdownData
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            var totalCount = statusBreakdown.Sum(s => s.Count);
            foreach (var item in statusBreakdown)
            {
                item.Percentage = totalCount > 0 ? (decimal)item.Count / totalCount * 100 : 0;
            }

            // Top clients by quotation value
            var topClients = await quotationsQuery
                .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate)
                .GroupBy(q => new { q.ClientId, q.Client.CompanyName })
                .Select(g => new TopClientData
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.CompanyName,
                    TotalValue = g.Sum(q => q.TotalAmount),
                    QuotationCount = g.Count()
                })
                .OrderByDescending(c => c.TotalValue)
                .Take(10)
                .ToListAsync();

            // Recent quotations
            var recentQuotations = await quotationsQuery
                .OrderByDescending(q => q.CreatedAt)
                .Take(10)
                .Select(q => new RecentQuotationData
                {
                    QuotationId = q.QuotationId,
                    QuotationNumber = q.QuotationNumber,
                    ClientName = q.Client.CompanyName,
                    Status = q.Status.ToString(),
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return new SalesDashboardMetricsDto
            {
                QuotationsCreatedThisMonth = quotationsCreatedThisMonth,
                QuotationsSentThisMonth = quotationsSentThisMonth,
                QuotationsAcceptedThisMonth = quotationsAcceptedThisMonth,
                TotalPipelineValue = totalPipelineValue,
                ConversionRate = conversionRate,
                PendingApprovals = pendingApprovals,
                QuotationTrend = trendData,
                StatusBreakdown = statusBreakdown,
                TopClients = topClients,
                RecentQuotations = recentQuotations
            };
        }
    }
}

