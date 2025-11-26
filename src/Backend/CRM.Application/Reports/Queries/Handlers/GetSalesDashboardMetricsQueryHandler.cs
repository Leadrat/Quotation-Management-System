using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GetSalesDashboardMetricsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<GetSalesDashboardMetricsQueryHandler> _logger;

        public GetSalesDashboardMetricsQueryHandler(IAppDbContext db, ILogger<GetSalesDashboardMetricsQueryHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<SalesDashboardMetricsDto> Handle(GetSalesDashboardMetricsQuery query)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1); // End of last day of month

                var fromDate = query.FromDate.HasValue ? new DateTimeOffset(query.FromDate.Value, TimeSpan.Zero) : startOfMonth;
                var toDate = query.ToDate.HasValue ? new DateTimeOffset(query.ToDate.Value.Date.AddDays(1).AddTicks(-1), TimeSpan.Zero) : endOfMonth;

                // Check if user is Admin
                var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);

                // Base query for quotations - use AsNoTracking for read-only
                IQueryable<Quotation> quotationsQuery = _db.Quotations
                    .AsNoTracking()
                    .Include(q => q.Client);

                // Authorization: SalesRep sees only own quotations, Admin sees all
                if (!isAdmin)
                {
                    quotationsQuery = quotationsQuery.Where(q => q.CreatedByUserId == query.UserId);
                }

                // Quotations created this month
                int quotationsCreatedThisMonth = 0;
                try
                {
                    quotationsCreatedThisMonth = await quotationsQuery
                        .Where(q => q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error counting quotations created this month");
                }

                // Quotations sent this month
                int quotationsSentThisMonth = 0;
                try
                {
                    quotationsSentThisMonth = await quotationsQuery
                        .Where(q => q.Status == QuotationStatus.Sent && 
                                   q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error counting quotations sent this month");
                }

                // Quotations accepted this month
                int quotationsAcceptedThisMonth = 0;
                try
                {
                    quotationsAcceptedThisMonth = await quotationsQuery
                        .Where(q => q.Status == QuotationStatus.Accepted && 
                                   q.CreatedAt >= startOfMonth && q.CreatedAt <= endOfMonth)
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error counting quotations accepted this month");
                }

                // Conversion rate (accepted / sent)
                decimal conversionRate = 0;
                if (quotationsSentThisMonth > 0)
                {
                    conversionRate = (decimal)quotationsAcceptedThisMonth / quotationsSentThisMonth * 100;
                }

                // Total pipeline value (open quotations: Draft, Sent, Viewed)
                decimal totalPipelineValue = 0;
                try
                {
                    var pipelineSum = await quotationsQuery
                        .Where(q => q.Status == QuotationStatus.Draft || 
                                   q.Status == QuotationStatus.Sent || 
                                   q.Status == QuotationStatus.Viewed)
                        .SumAsync(q => (decimal?)q.TotalAmount);
                    totalPipelineValue = pipelineSum ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating total pipeline value");
                }

                // Pending approvals
                int pendingApprovals = 0;
                try
                {
                    var approvalsQuery = _db.DiscountApprovals
                        .AsNoTracking()
                        .Where(a => a.Status == ApprovalStatus.Pending);
                    
                    // Authorization: SalesRep sees only own approvals, Admin sees all
                    if (!isAdmin)
                    {
                        approvalsQuery = approvalsQuery.Where(a => a.RequestedByUserId == query.UserId);
                    }
                    
                    pendingApprovals = await approvalsQuery.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error counting pending approvals");
                }

                // Quotation trend (last 30 days) - simplified to avoid Date grouping issues
                var trendData = new List<QuotationTrendData>();
                try
                {
                    var trendStartDate = now.AddDays(-30);
                    var quotations = await quotationsQuery
                        .Where(q => q.CreatedAt >= trendStartDate)
                        .Select(q => new { q.CreatedAt, q.Status })
                        .ToListAsync();

                    var grouped = quotations
                        .GroupBy(q => q.CreatedAt.Date)
                        .Select(g => new QuotationTrendData
                        {
                            Date = g.Key,
                            Created = g.Count(),
                            Sent = g.Count(q => q.Status == QuotationStatus.Sent)
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    trendData = grouped;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating quotation trend");
                }

                // Status breakdown
                var statusBreakdown = new List<StatusBreakdownData>();
                try
                {
                    var statusData = await quotationsQuery
                        .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate)
                        .GroupBy(q => q.Status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var totalCount = statusData.Sum(s => s.Count);
                    statusBreakdown = statusData
                        .Select(s => new StatusBreakdownData
                        {
                            Status = s.Status.ToString(),
                            Count = s.Count,
                            Percentage = totalCount > 0 ? (decimal)s.Count / totalCount * 100 : 0
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating status breakdown");
                }

                // Top clients by quotation value
                var topClients = new List<TopClientData>();
                try
                {
                    topClients = await quotationsQuery
                        .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate && q.Client != null)
                        .GroupBy(q => new { q.ClientId, CompanyName = q.Client!.CompanyName ?? "Unknown" })
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
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating top clients");
                }

                // Recent quotations
                var recentQuotations = new List<RecentQuotationData>();
                try
                {
                    recentQuotations = await quotationsQuery
                        .Where(q => q.Client != null)
                        .OrderByDescending(q => q.CreatedAt)
                        .Take(10)
                        .Select(q => new RecentQuotationData
                        {
                            QuotationId = q.QuotationId,
                            QuotationNumber = q.QuotationNumber ?? string.Empty,
                            ClientName = q.Client != null ? (q.Client.CompanyName ?? "Unknown") : "Unknown",
                            Status = q.Status.ToString(),
                            CreatedAt = q.CreatedAt
                        })
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching recent quotations");
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in GetSalesDashboardMetricsQueryHandler for UserId: {UserId}", query.UserId);
                throw;
            }
        }
    }
}
