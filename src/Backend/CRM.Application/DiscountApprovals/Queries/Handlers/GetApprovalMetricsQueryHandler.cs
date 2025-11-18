using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Queries;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Queries.Handlers
{
    public class GetApprovalMetricsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<GetApprovalMetricsQueryHandler> _logger;

        public GetApprovalMetricsQueryHandler(
            IAppDbContext db,
            ILogger<GetApprovalMetricsQueryHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ApprovalMetricsDto> Handle(GetApprovalMetricsQuery query)
        {
            _logger.LogInformation("Getting approval metrics");

            // Base query
            var baseQuery = _db.DiscountApprovals.AsNoTracking();

            // Apply date filters
            if (query.DateFrom.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestDate >= query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.RequestDate <= query.DateTo.Value);
            }

            // Apply approver filter
            if (query.ApproverUserId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.ApproverUserId == query.ApproverUserId.Value);
            }

            var approvals = await baseQuery.ToListAsync();

            var pendingCount = approvals.Count(a => a.Status == ApprovalStatus.Pending);
            var approvedCount = approvals.Count(a => a.Status == ApprovalStatus.Approved);
            var rejectedCount = approvals.Count(a => a.Status == ApprovalStatus.Rejected);
            var totalCount = approvals.Count;

            // Calculate average approval time (TAT)
            var approvedApprovals = approvals
                .Where(a => a.Status == ApprovalStatus.Approved && a.ApprovalDate.HasValue)
                .ToList();

            TimeSpan? averageApprovalTime = null;
            if (approvedApprovals.Any())
            {
                var totalTime = approvedApprovals
                    .Sum(a => (a.ApprovalDate!.Value - a.RequestDate).TotalMilliseconds);
                averageApprovalTime = TimeSpan.FromMilliseconds(totalTime / approvedApprovals.Count);
            }

            // Calculate rejection rate
            var rejectionRate = totalCount > 0 
                ? (decimal)rejectedCount / totalCount * 100 
                : 0;

            // Calculate average discount percentage
            var averageDiscountPercentage = approvals.Any()
                ? approvals.Average(a => (double)a.CurrentDiscountPercentage)
                : 0;

            // Calculate escalation count
            var escalationCount = approvals.Count(a => a.EscalatedToAdmin);

            return new ApprovalMetricsDto
            {
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,
                AverageApprovalTime = averageApprovalTime,
                RejectionRate = (decimal)rejectionRate,
                AverageDiscountPercentage = (decimal)averageDiscountPercentage,
                EscalationCount = escalationCount,
                DateFrom = query.DateFrom,
                DateTo = query.DateTo
            };
        }
    }
}

