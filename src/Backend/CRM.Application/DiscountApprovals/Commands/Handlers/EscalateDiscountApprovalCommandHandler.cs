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
    public class EscalateDiscountApprovalCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<EscalateDiscountApprovalCommandHandler> _logger;

        public EscalateDiscountApprovalCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<EscalateDiscountApprovalCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(EscalateDiscountApprovalCommand command)
        {
            _logger.LogInformation("Escalating discount approval {ApprovalId} by user {UserId}", 
                command.ApprovalId, command.EscalatedByUserId);

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

            // Verify escalation is allowed (not already escalated, or manager/admin can escalate)
            if (approval.EscalatedToAdmin)
            {
                throw new InvalidOperationException("Approval has already been escalated to admin.");
            }

            // Get escalator user and role
            var escalator = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.EscalatedByUserId);

            if (escalator == null)
            {
                throw new InvalidOperationException($"User with ID {command.EscalatedByUserId} not found.");
            }

            var escalatorRole = escalator.Role?.RoleName ?? string.Empty;

            // Only managers or admins can escalate
            if (escalatorRole != "Manager" && escalatorRole != "Admin")
            {
                throw new UnauthorizedApprovalActionException(command.ApprovalId, command.EscalatedByUserId, "escalate");
            }

            // Find an admin user
            var admin = await _db.Users
                .Where(u => u.RoleId == RoleIds.Admin && u.IsActive && u.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                throw new InvalidOperationException("No active admin user found to escalate to.");
            }

            // Escalate the approval
            approval.Escalate(command.EscalatedByUserId, admin.UserId, command.Reason);

            approval.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Publish domain event
            var domainEvent = new DiscountApprovalEscalated
            {
                ApprovalId = approval.ApprovalId,
                QuotationId = approval.QuotationId,
                EscalatedByUserId = command.EscalatedByUserId,
                AdminUserId = admin.UserId,
                Reason = command.Reason,
                EscalationDate = DateTimeOffset.UtcNow
            };
            _ = domainEvent;

            // Reload with navigation properties
            approval = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == approval.ApprovalId);

            var result = _mapper.Map<DiscountApprovalDto>(approval);
            
            _logger.LogInformation("Discount approval {ApprovalId} escalated to admin {AdminId}", 
                approval.ApprovalId, admin.UserId);

            return result;
        }
    }
}

