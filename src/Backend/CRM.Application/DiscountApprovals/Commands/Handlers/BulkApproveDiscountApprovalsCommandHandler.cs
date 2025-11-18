using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using CRM.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Commands.Handlers
{
    public class BulkApproveDiscountApprovalsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<BulkApproveDiscountApprovalsCommandHandler> _logger;

        public BulkApproveDiscountApprovalsCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<BulkApproveDiscountApprovalsCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DiscountApprovalDto>> Handle(BulkApproveDiscountApprovalsCommand command)
        {
            _logger.LogInformation("Bulk approving {Count} discount approvals by user {UserId}", 
                command.Request.ApprovalIds.Count, command.ApprovedByUserId);

            // Load all approvals with navigation properties
            var approvals = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .Where(a => command.Request.ApprovalIds.Contains(a.ApprovalId))
                .ToListAsync();

            if (approvals.Count != command.Request.ApprovalIds.Count)
            {
                var foundIds = approvals.Select(a => a.ApprovalId).ToList();
                var missingIds = command.Request.ApprovalIds.Except(foundIds).ToList();
                throw new DiscountApprovalNotFoundException(missingIds.First());
            }

            // Get approver user and role
            var approver = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.ApprovedByUserId);

            if (approver == null)
            {
                throw new InvalidOperationException($"User with ID {command.ApprovedByUserId} not found.");
            }

            var approverRole = approver.Role?.RoleName ?? string.Empty;

            // Verify all approvals are pending and user has permission
            foreach (var approval in approvals)
            {
                if (!approval.IsPending())
                {
                    throw new InvalidApprovalStatusException(approval.Status.ToString(), "Pending");
                }

                if (!approval.CanBeApprovedBy(command.ApprovedByUserId, approverRole))
                {
                    throw new UnauthorizedApprovalActionException(approval.ApprovalId, command.ApprovedByUserId, "approve");
                }
            }

            var results = new List<DiscountApprovalDto>();
            var now = DateTimeOffset.UtcNow;

            // Process each approval
            foreach (var approval in approvals)
            {
                // Approve the request
                approval.Approve(command.ApprovedByUserId, command.Request.Reason, command.Request.Comments);

                // Update quotation discount
                approval.Quotation.DiscountPercentage = approval.CurrentDiscountPercentage;
                approval.Quotation.DiscountAmount = approval.Quotation.SubTotal * (approval.CurrentDiscountPercentage / 100m);
                
                // Recalculate totals (simplified)
                var newSubTotal = approval.Quotation.SubTotal - approval.Quotation.DiscountAmount;
                approval.Quotation.TaxAmount = newSubTotal * 0.18m; // 18% GST
                approval.Quotation.TotalAmount = newSubTotal + approval.Quotation.TaxAmount;

                // Unlock quotation
                approval.Quotation.UnlockFromApproval();

                approval.UpdatedAt = now;
                approval.Quotation.UpdatedAt = now;

                // Publish domain event
                var domainEvent = new DiscountApprovalApproved
                {
                    ApprovalId = approval.ApprovalId,
                    QuotationId = approval.QuotationId,
                    ApprovedByUserId = command.ApprovedByUserId,
                    DiscountPercentage = approval.CurrentDiscountPercentage,
                    Reason = command.Request.Reason,
                    Comments = command.Request.Comments,
                    ApprovalDate = approval.ApprovalDate ?? now
                };
                _ = domainEvent;

                results.Add(_mapper.Map<DiscountApprovalDto>(approval));
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Bulk approved {Count} discount approvals successfully", approvals.Count);

            return results;
        }
    }
}

