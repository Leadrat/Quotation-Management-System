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
    public class GetTeamPerformanceMetricsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetTeamPerformanceMetricsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TeamPerformanceDto>> Handle(GetTeamPerformanceMetricsQuery query)
        {
            var fromDate = query.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.UtcNow;

            var baseQuery = _db.Quotations
                .Include(q => q.CreatedByUser)
                .Where(q => q.CreatedAt >= fromDate && q.CreatedAt <= toDate);

            // Filter by team (users reporting to same manager) or specific user
            if (query.UserId.HasValue)
            {
                baseQuery = baseQuery.Where(q => q.CreatedByUserId == query.UserId.Value);
            }
            else if (query.TeamId.HasValue)
            {
                // Get all users reporting to this manager
                var teamUserIds = await _db.Users
                    .Where(u => u.ReportingManagerId == query.TeamId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                baseQuery = baseQuery.Where(q => teamUserIds.Contains(q.CreatedByUserId));
            }

            // Group by user and calculate metrics
            var userMetrics = await baseQuery
                .GroupBy(q => new { q.CreatedByUserId, q.CreatedByUser.FirstName, q.CreatedByUser.LastName })
                .Select(g => new
                {
                    UserId = g.Key.CreatedByUserId,
                    UserName = g.Key.FirstName + " " + g.Key.LastName,
                    QuotationsCreated = g.Count(),
                    QuotationsSent = g.Count(q => q.Status == QuotationStatus.Sent),
                    QuotationsAccepted = g.Count(q => q.Status == QuotationStatus.Accepted),
                    PipelineValue = g.Where(q => q.Status == QuotationStatus.Draft || 
                                                 q.Status == QuotationStatus.Sent || 
                                                 q.Status == QuotationStatus.Viewed)
                                    .Sum(q => (decimal?)q.TotalAmount) ?? 0,
                    AverageDiscount = g.Average(q => (decimal?)q.DiscountPercentage) ?? 0
                })
                .ToListAsync();

            // Calculate conversion rates and rankings
            var results = userMetrics
                .Select(m => new TeamPerformanceDto
                {
                    UserId = m.UserId,
                    UserName = m.UserName,
                    QuotationsCreated = m.QuotationsCreated,
                    QuotationsSent = m.QuotationsSent,
                    QuotationsAccepted = m.QuotationsAccepted,
                    ConversionRate = m.QuotationsSent > 0 
                        ? (decimal)m.QuotationsAccepted / m.QuotationsSent * 100 
                        : 0,
                    PipelineValue = m.PipelineValue,
                    AverageDiscount = m.AverageDiscount,
                    PendingApprovals = 0, // Will be calculated separately
                    Rank = 0, // Will be set after sorting
                    Trend = new List<PerformanceTrendData>() // Can be populated if needed
                })
                .OrderByDescending(t => t.QuotationsAccepted)
                .ToList();

            // Set rankings
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
            }

            // Get pending approvals for each user
            var userIds = results.Select(r => r.UserId).ToList();
            var pendingApprovals = await _db.DiscountApprovals
                .Where(a => userIds.Contains(a.RequestedByUserId) && 
                           a.Status == Domain.Enums.ApprovalStatus.Pending)
                .GroupBy(a => a.RequestedByUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var approvalsDict = pendingApprovals.ToDictionary(a => a.UserId, a => a.Count);
            foreach (var result in results)
            {
                result.PendingApprovals = approvalsDict.GetValueOrDefault(result.UserId, 0);
            }

            return results;
        }
    }
}

