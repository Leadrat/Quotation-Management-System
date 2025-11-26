using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using CRM.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.Commands.Handlers
{
    public class RequestDiscountApprovalCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<RequestDiscountApprovalCommandHandler> _logger;
        private const decimal ManagerThreshold = 10.0m;
        private const decimal AdminThreshold = 20.0m;

        public RequestDiscountApprovalCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<RequestDiscountApprovalCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(RequestDiscountApprovalCommand command)
        {
            _logger.LogInformation("Requesting discount approval for quotation {QuotationId} by user {UserId}", 
                command.Request.QuotationId, command.RequestedByUserId);

            // Validate quotation exists
            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .FirstOrDefaultAsync(q => q.QuotationId == command.Request.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(command.Request.QuotationId);
            }

            // Check if quotation is already pending approval
            if (quotation.IsPendingApproval)
            {
                throw new QuotationLockedException(quotation.QuotationId, quotation.PendingApprovalId);
            }

            // Validate discount percentage matches request
            if (Math.Abs(quotation.DiscountPercentage - command.Request.DiscountPercentage) > 0.01m)
            {
                throw new InvalidOperationException("Discount percentage in request does not match quotation discount percentage.");
            }

            // Determine approval level based on threshold
            ApprovalLevel approvalLevel;
            decimal threshold;
            if (command.Request.DiscountPercentage >= AdminThreshold)
            {
                approvalLevel = ApprovalLevel.Admin;
                threshold = AdminThreshold;
            }
            else if (command.Request.DiscountPercentage >= ManagerThreshold)
            {
                approvalLevel = ApprovalLevel.Manager;
                threshold = ManagerThreshold;
            }
            else
            {
                throw new InvalidOperationException($"Discount percentage {command.Request.DiscountPercentage}% is below the minimum threshold of {ManagerThreshold}%.");
            }

            // Find appropriate approver
            Guid? approverUserId = null;
            if (approvalLevel == ApprovalLevel.Manager)
            {
                // For Manager-level approvals, leave ApproverUserId as null
                // This allows ALL managers to see and approve the request
                // (Any manager can approve Manager-level requests)
                approverUserId = null;
            }
            else // Admin
            {
                // Find any active admin
                var admin = await _db.Users
                    .Where(u => u.RoleId == RoleIds.Admin && u.IsActive && u.DeletedAt == null)
                    .FirstOrDefaultAsync();
                approverUserId = admin?.UserId;
            }

            // Create approval record
            var approval = new DiscountApproval
            {
                ApprovalId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                RequestedByUserId = command.RequestedByUserId,
                ApproverUserId = approverUserId,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTimeOffset.UtcNow,
                CurrentDiscountPercentage = command.Request.DiscountPercentage,
                Threshold = threshold,
                ApprovalLevel = approvalLevel,
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                EscalatedToAdmin = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.DiscountApprovals.Add(approval);

            // Lock quotation
            quotation.LockForApproval(approval.ApprovalId);

            await _db.SaveChangesAsync();

            // Publish domain event
            var domainEvent = new DiscountApprovalRequested
            {
                ApprovalId = approval.ApprovalId,
                QuotationId = quotation.QuotationId,
                RequestedByUserId = command.RequestedByUserId,
                ApproverUserId = approverUserId,
                DiscountPercentage = command.Request.DiscountPercentage,
                Threshold = threshold,
                ApprovalLevel = approvalLevel.ToString(),
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                RequestDate = approval.RequestDate
            };
            _ = domainEvent; // Event will be handled by event handlers in Phase 5

            // Reload with navigation properties for mapping
            approval = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == approval.ApprovalId);

            var result = _mapper.Map<DiscountApprovalDto>(approval);
            
            _logger.LogInformation("Discount approval {ApprovalId} created successfully for quotation {QuotationId}", 
                approval.ApprovalId, quotation.QuotationId);

            return result;
        }
    }
}

