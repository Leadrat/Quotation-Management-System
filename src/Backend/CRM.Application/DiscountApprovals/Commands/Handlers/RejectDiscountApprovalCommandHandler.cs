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
    public class RejectDiscountApprovalCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<RejectDiscountApprovalCommandHandler> _logger;

        public RejectDiscountApprovalCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<RejectDiscountApprovalCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(RejectDiscountApprovalCommand command)
        {
            _logger.LogInformation("Rejecting discount approval {ApprovalId} by user {UserId}", 
                command.ApprovalId, command.RejectedByUserId);

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

            // Get rejector user and role
            var rejector = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.RejectedByUserId);

            if (rejector == null)
            {
                throw new InvalidOperationException($"User with ID {command.RejectedByUserId} not found.");
            }

            var rejectorRole = rejector.Role?.RoleName ?? string.Empty;

            // Verify user has permission to reject
            if (!approval.CanBeApprovedBy(command.RejectedByUserId, rejectorRole))
            {
                throw new UnauthorizedApprovalActionException(command.ApprovalId, command.RejectedByUserId, "reject");
            }

            // Reject the request
            approval.Reject(command.RejectedByUserId, command.Request.Reason, command.Request.Comments);

            // Revert quotation discount to 0 (or previous value if stored)
            approval.Quotation.DiscountPercentage = 0;
            approval.Quotation.DiscountAmount = 0;

            // Recalculate totals
            var newSubTotal = approval.Quotation.SubTotal;
            approval.Quotation.TaxAmount = newSubTotal * 0.18m; // 18% GST (simplified)
            approval.Quotation.TotalAmount = newSubTotal + approval.Quotation.TaxAmount;

            // Unlock quotation
            approval.Quotation.UnlockFromApproval();

            approval.UpdatedAt = DateTimeOffset.UtcNow;
            approval.Quotation.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Publish domain event
            var domainEvent = new DiscountApprovalRejected
            {
                ApprovalId = approval.ApprovalId,
                QuotationId = approval.QuotationId,
                RejectedByUserId = command.RejectedByUserId,
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                RejectionDate = approval.RejectionDate ?? DateTimeOffset.UtcNow
            };
            _ = domainEvent;

            var result = _mapper.Map<DiscountApprovalDto>(approval);
            
            _logger.LogInformation("Discount approval {ApprovalId} rejected successfully", approval.ApprovalId);

            return result;
        }
    }
}

