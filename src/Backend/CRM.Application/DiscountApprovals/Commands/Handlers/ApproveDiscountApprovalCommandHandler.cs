using System;
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
    public class ApproveDiscountApprovalCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<ApproveDiscountApprovalCommandHandler> _logger;

        public ApproveDiscountApprovalCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<ApproveDiscountApprovalCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(ApproveDiscountApprovalCommand command)
        {
            _logger.LogInformation("Approving discount approval {ApprovalId} by user {UserId}", 
                command.ApprovalId, command.ApprovedByUserId);

            // Load approval with navigation properties
            var approval = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == command.ApprovalId);

            if (approval == null)
            {
                throw new DiscountApprovalNotFoundException(command.ApprovalId);
            }

            // Verify approval is pending
            if (!approval.IsPending())
            {
                throw new InvalidApprovalStatusException(approval.Status.ToString(), "Pending");
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

            // Verify user has permission to approve
            if (!approval.CanBeApprovedBy(command.ApprovedByUserId, approverRole))
            {
                throw new UnauthorizedApprovalActionException(command.ApprovalId, command.ApprovedByUserId, "approve");
            }

            // Approve the request
            approval.Approve(command.ApprovedByUserId, command.Request.Reason, command.Request.Comments);

            // Update quotation discount
            approval.Quotation.DiscountPercentage = approval.CurrentDiscountPercentage;
            approval.Quotation.DiscountAmount = approval.Quotation.SubTotal * (approval.CurrentDiscountPercentage / 100m);
            
            // Recalculate totals (simplified - in production, use the totals calculator service)
            var newSubTotal = approval.Quotation.SubTotal - approval.Quotation.DiscountAmount;
            approval.Quotation.TaxAmount = newSubTotal * 0.18m; // 18% GST (simplified)
            approval.Quotation.TotalAmount = newSubTotal + approval.Quotation.TaxAmount;

            // Unlock quotation
            approval.Quotation.UnlockFromApproval();

            approval.UpdatedAt = DateTimeOffset.UtcNow;
            approval.Quotation.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Publish domain event
            var domainEvent = new DiscountApprovalApproved
            {
                ApprovalId = approval.ApprovalId,
                QuotationId = approval.QuotationId,
                ApprovedByUserId = command.ApprovedByUserId,
                DiscountPercentage = approval.CurrentDiscountPercentage,
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                ApprovalDate = approval.ApprovalDate ?? DateTimeOffset.UtcNow
            };
            _ = domainEvent;

            var result = _mapper.Map<DiscountApprovalDto>(approval);
            
            _logger.LogInformation("Discount approval {ApprovalId} approved successfully", approval.ApprovalId);

            return result;
        }
    }
}

