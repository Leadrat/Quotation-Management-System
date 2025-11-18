using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Reports.Queries.Handlers
{
    public class GetAuditComplianceReportQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetAuditComplianceReportQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<AuditReportDto> Handle(GetAuditComplianceReportQuery query)
        {
            var report = new AuditReportDto
            {
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                Changes = new List<AuditEntryData>(),
                Approvals = new List<ApprovalHistoryData>(),
                Payments = new List<PaymentHistoryData>(),
                UserActivity = new List<UserActivityData>()
            };

            // Get approval history
            var approvalsQuery = _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .Where(a => a.RequestDate >= query.FromDate && a.RequestDate <= query.ToDate);

            if (query.UserId.HasValue)
            {
                approvalsQuery = approvalsQuery.Where(a => a.RequestedByUserId == query.UserId || 
                                                           a.ApproverUserId == query.UserId);
            }

            report.Approvals = await approvalsQuery
                .Select(a => new ApprovalHistoryData
                {
                    ApprovalId = a.ApprovalId,
                    QuotationId = a.QuotationId,
                    QuotationNumber = a.Quotation.QuotationNumber,
                    Status = a.Status.ToString(),
                    RequestedByUserId = a.RequestedByUserId,
                    RequestedByUserName = a.RequestedByUser.FirstName + " " + a.RequestedByUser.LastName,
                    ApprovedByUserId = a.ApproverUserId,
                    ApprovedByUserName = a.ApproverUser != null 
                        ? a.ApproverUser.FirstName + " " + a.ApproverUser.LastName 
                        : null,
                    RequestedAt = a.RequestDate,
                    ApprovedAt = a.ApprovalDate
                })
                .ToListAsync();

            // Get payment history
            var paymentsQuery = _db.Payments
                .Include(p => p.Quotation)
                .Where(p => p.CreatedAt >= query.FromDate && p.CreatedAt <= query.ToDate);

            if (query.UserId.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Quotation.CreatedByUserId == query.UserId);
            }

            report.Payments = await paymentsQuery
                .Select(p => new PaymentHistoryData
                {
                    PaymentId = p.PaymentId,
                    QuotationId = p.QuotationId,
                    QuotationNumber = p.Quotation.QuotationNumber,
                    PaymentGateway = p.PaymentGateway,
                    Amount = p.AmountPaid,
                    Status = p.PaymentStatus.ToString(),
                    CreatedAt = p.CreatedAt,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            // Get quotation changes (created/updated)
            var quotationsQuery = _db.Quotations
                .Include(q => q.CreatedByUser)
                .Where(q => q.CreatedAt >= query.FromDate && q.CreatedAt <= query.ToDate);

            if (query.UserId.HasValue)
            {
                quotationsQuery = quotationsQuery.Where(q => q.CreatedByUserId == query.UserId);
            }

            var quotations = await quotationsQuery.ToListAsync();

            foreach (var quotation in quotations)
            {
                report.Changes.Add(new AuditEntryData
                {
                    EntryId = Guid.NewGuid(),
                    EntityType = "Quotation",
                    EntityId = quotation.QuotationId,
                    Action = "Created",
                    UserId = quotation.CreatedByUserId,
                    UserName = quotation.CreatedByUser.FirstName + " " + quotation.CreatedByUser.LastName,
                    Timestamp = quotation.CreatedAt
                });
            }

            // User activity (logins, etc.) - simplified version
            // In a real system, this would come from an activity log table
            var users = await _db.Users
                .Where(u => u.LastLoginAt.HasValue && 
                           u.LastLoginAt >= query.FromDate && 
                           u.LastLoginAt <= query.ToDate)
                .ToListAsync();

            foreach (var user in users)
            {
                if (query.UserId == null || user.UserId == query.UserId)
                {
                    report.UserActivity.Add(new UserActivityData
                    {
                        UserId = user.UserId,
                        UserName = user.FirstName + " " + user.LastName,
                        ActivityType = "Login",
                        Timestamp = user.LastLoginAt!.Value,
                        Details = "User logged in"
                    });
                }
            }

            return report;
        }
    }
}

