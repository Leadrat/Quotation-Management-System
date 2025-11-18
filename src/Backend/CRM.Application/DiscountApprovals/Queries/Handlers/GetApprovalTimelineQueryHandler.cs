using System;
using System.Collections.Generic;
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
    public class GetApprovalTimelineQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<GetApprovalTimelineQueryHandler> _logger;

        public GetApprovalTimelineQueryHandler(
            IAppDbContext db,
            ILogger<GetApprovalTimelineQueryHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<ApprovalTimelineDto>> Handle(GetApprovalTimelineQuery query)
        {
            _logger.LogInformation("Getting approval timeline for {ApprovalId} or {QuotationId}", 
                query.ApprovalId, query.QuotationId);

            IQueryable<Domain.Entities.DiscountApproval> approvalsQuery;

            if (query.QuotationId.HasValue)
            {
                // Get all approvals for quotation
                approvalsQuery = _db.DiscountApprovals
                    .AsNoTracking()
                    .Include(a => a.RequestedByUser)
                        .ThenInclude(u => u.Role)
                    .Include(a => a.ApproverUser)
                        .ThenInclude(u => u.Role)
                    .Where(a => a.QuotationId == query.QuotationId.Value);
            }
            else if (query.ApprovalId.HasValue)
            {
                // Get single approval and its history (if resubmitted)
                var approval = await _db.DiscountApprovals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApprovalId == query.ApprovalId.Value);

                if (approval == null)
                {
                    return new List<ApprovalTimelineDto>();
                }

                approvalsQuery = _db.DiscountApprovals
                    .AsNoTracking()
                    .Include(a => a.RequestedByUser)
                        .ThenInclude(u => u.Role)
                    .Include(a => a.ApproverUser)
                        .ThenInclude(u => u.Role)
                    .Where(a => a.QuotationId == approval.QuotationId);
            }
            else
            {
                throw new ArgumentException("Either ApprovalId or QuotationId must be provided.");
            }

            var approvals = await approvalsQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var timeline = new List<ApprovalTimelineDto>();

            foreach (var approval in approvals)
            {
                // Request event
                var requestedByUser = approval.RequestedByUser;
                timeline.Add(new ApprovalTimelineDto
                {
                    ApprovalId = approval.ApprovalId,
                    QuotationId = approval.QuotationId,
                    EventType = "Requested",
                    Status = approval.Status.ToString(),
                    PreviousStatus = null,
                    UserId = approval.RequestedByUserId,
                    UserName = requestedByUser != null ? requestedByUser.GetFullName() : "Unknown",
                    UserRole = requestedByUser?.Role?.RoleName ?? "Unknown",
                    Reason = approval.Reason,
                    Comments = approval.Comments,
                    Timestamp = approval.RequestDate
                });

                // Approval/Rejection event
                var approverUser = approval.ApproverUser;
                if (approval.IsApproved() && approval.ApprovalDate.HasValue)
                {
                    timeline.Add(new ApprovalTimelineDto
                    {
                        ApprovalId = approval.ApprovalId,
                        QuotationId = approval.QuotationId,
                        EventType = "Approved",
                        Status = approval.Status.ToString(),
                        PreviousStatus = "Pending",
                        UserId = approval.ApproverUserId ?? Guid.Empty,
                        UserName = approverUser != null ? approverUser.GetFullName() : "Unknown",
                        UserRole = approverUser?.Role?.RoleName ?? "Unknown",
                        Reason = approval.Reason,
                        Comments = approval.Comments,
                        Timestamp = approval.ApprovalDate.Value
                    });
                }
                else if (approval.IsRejected() && approval.RejectionDate.HasValue)
                {
                    timeline.Add(new ApprovalTimelineDto
                    {
                        ApprovalId = approval.ApprovalId,
                        QuotationId = approval.QuotationId,
                        EventType = "Rejected",
                        Status = approval.Status.ToString(),
                        PreviousStatus = "Pending",
                        UserId = approval.ApproverUserId ?? Guid.Empty,
                        UserName = approverUser != null ? approverUser.GetFullName() : "Unknown",
                        UserRole = approverUser?.Role?.RoleName ?? "Unknown",
                        Reason = approval.Reason,
                        Comments = approval.Comments,
                        Timestamp = approval.RejectionDate.Value
                    });
                }

                // Escalation event
                if (approval.EscalatedToAdmin)
                {
                    var escalatedApproverUser = approval.ApproverUser;
                    timeline.Add(new ApprovalTimelineDto
                    {
                        ApprovalId = approval.ApprovalId,
                        QuotationId = approval.QuotationId,
                        EventType = "Escalated",
                        Status = approval.Status.ToString(),
                        PreviousStatus = "Pending",
                        UserId = approval.ApproverUserId ?? Guid.Empty,
                        UserName = escalatedApproverUser != null ? escalatedApproverUser.GetFullName() : "Unknown",
                        UserRole = escalatedApproverUser?.Role?.RoleName ?? "Admin",
                        Reason = "Escalated to admin",
                        Comments = approval.Comments,
                        Timestamp = approval.UpdatedAt
                    });
                }
            }

            // Sort by timestamp descending
            return timeline.OrderByDescending(t => t.Timestamp).ToList();
        }
    }
}

