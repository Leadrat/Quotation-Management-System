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
    public class GetApprovalWorkflowMetricsQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetApprovalWorkflowMetricsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ApprovalMetricsDto> Handle(GetApprovalWorkflowMetricsQuery query)
        {
            var fromDate = query.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.UtcNow;

            var baseQuery = _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Where(a => a.RequestDate >= fromDate && a.RequestDate <= toDate);

            // Filter by manager's team if specified
            if (query.ManagerId.HasValue)
            {
                var teamUserIds = await _db.Users
                    .Where(u => u.ReportingManagerId == query.ManagerId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                baseQuery = baseQuery.Where(a => teamUserIds.Contains(a.RequestedByUserId));
            }

            var approvals = await baseQuery.ToListAsync();

            // Calculate average TAT (Time to Approval) in hours
            var approvedApprovals = approvals
                .Where(a => a.Status == ApprovalStatus.Approved && a.ApprovalDate.HasValue)
                .ToList();

            var averageTAT = approvedApprovals.Any()
                ? (decimal)approvedApprovals
                    .Select(a => (a.ApprovalDate!.Value - a.RequestDate).TotalHours)
                    .Average()
                : 0;

            // Calculate rejection rate
            var totalProcessed = approvals.Count(a => a.Status == ApprovalStatus.Approved || 
                                                      a.Status == ApprovalStatus.Rejected);
            var rejectionRate = totalProcessed > 0
                ? (decimal)approvals.Count(a => a.Status == ApprovalStatus.Rejected) / totalProcessed * 100
                : 0;

            // Calculate escalation percentage
            var escalationPercent = approvals.Any()
                ? (decimal)approvals.Count(a => a.EscalatedToAdmin) / approvals.Count * 100
                : 0;

            // Pending approvals
            var pendingApprovals = approvals.Count(a => a.Status == ApprovalStatus.Pending);

            // Approval TAT by period (weekly)
            var tatByPeriod = approvedApprovals
                .GroupBy(a => GetWeekKey(a.ApprovalDate!.Value))
                .Select(g => new ApprovalTATData
                {
                    Period = g.Key,
                    AverageHours = (decimal)g.Average(a => (a.ApprovalDate!.Value - a.RequestDate).TotalHours)
                })
                .OrderBy(x => x.Period)
                .ToList();

            // Approval status breakdown
            var statusBreakdown = approvals
                .GroupBy(a => a.Status)
                .Select(g => new ApprovalStatusData
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            var totalCount = statusBreakdown.Sum(s => s.Count);
            foreach (var item in statusBreakdown)
            {
                item.Percentage = totalCount > 0 ? (decimal)item.Count / totalCount * 100 : 0;
            }

            // Escalations
            var escalations = await baseQuery
                .Where(a => a.EscalatedToAdmin)
                .Select(a => new EscalationData
                {
                    ApprovalId = a.ApprovalId,
                    QuotationId = a.QuotationId,
                    QuotationNumber = a.Quotation.QuotationNumber,
                    DiscountAmount = a.Quotation.DiscountAmount,
                    EscalatedAt = a.RequestDate
                })
                .ToListAsync();

            return new ApprovalMetricsDto
            {
                AverageApprovalTAT = averageTAT,
                RejectionRate = rejectionRate,
                EscalationPercent = escalationPercent,
                PendingApprovals = pendingApprovals,
                ApprovalTATByPeriod = tatByPeriod,
                ApprovalStatusBreakdown = statusBreakdown,
                Escalations = escalations
            };
        }

        private string GetWeekKey(DateTimeOffset date)
        {
            var startOfYear = new DateTime(date.Year, 1, 1);
            var daysSinceStart = (date.Date - startOfYear.Date).Days;
            var weekNumber = (daysSinceStart / 7) + 1;
            return $"{date.Year}-W{weekNumber:D2}";
        }
    }
}

