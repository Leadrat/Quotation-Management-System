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
    public class ResubmitDiscountApprovalCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<ResubmitDiscountApprovalCommandHandler> _logger;
        private const decimal ManagerThreshold = 10.0m;
        private const decimal AdminThreshold = 20.0m;

        public ResubmitDiscountApprovalCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<ResubmitDiscountApprovalCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DiscountApprovalDto> Handle(ResubmitDiscountApprovalCommand command)
        {
            _logger.LogInformation("Resubmitting discount approval {ApprovalId} by user {UserId}", 
                command.ApprovalId, command.ResubmittedByUserId);

            // Load the original (rejected) approval
            var originalApproval = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                .FirstOrDefaultAsync(a => a.ApprovalId == command.ApprovalId);

            if (originalApproval == null)
            {
                throw new DiscountApprovalNotFoundException(command.ApprovalId);
            }

            // Verify original approval is rejected
            if (!originalApproval.IsRejected())
            {
                throw new InvalidApprovalStatusException(originalApproval.Status.ToString(), "Rejected");
            }

            // Verify user is the original requester
            if (originalApproval.RequestedByUserId != command.ResubmittedByUserId)
            {
                throw new UnauthorizedApprovalActionException(command.ApprovalId, command.ResubmittedByUserId, "resubmit");
            }

            // Check if quotation is already pending approval
            if (originalApproval.Quotation.IsPendingApproval)
            {
                throw new QuotationLockedException(originalApproval.Quotation.QuotationId, originalApproval.Quotation.PendingApprovalId);
            }

            // Get current discount percentage from quotation
            var currentDiscountPercentage = originalApproval.Quotation.DiscountPercentage;

            // Determine approval level based on threshold
            ApprovalLevel approvalLevel;
            decimal threshold;
            if (currentDiscountPercentage >= AdminThreshold)
            {
                approvalLevel = ApprovalLevel.Admin;
                threshold = AdminThreshold;
            }
            else if (currentDiscountPercentage >= ManagerThreshold)
            {
                approvalLevel = ApprovalLevel.Manager;
                threshold = ManagerThreshold;
            }
            else
            {
                throw new InvalidOperationException($"Discount percentage {currentDiscountPercentage}% is below the minimum threshold of {ManagerThreshold}%.");
            }

            // Find appropriate approver
            Guid? approverUserId = null;
            if (approvalLevel == ApprovalLevel.Manager)
            {
                // Find the requesting user's manager
                var requester = await _db.Users
                    .Include(u => u.ReportingManager)
                    .FirstOrDefaultAsync(u => u.UserId == command.ResubmittedByUserId);

                if (requester?.ReportingManagerId != null)
                {
                    approverUserId = requester.ReportingManagerId;
                }
                else
                {
                    // If no manager assigned, find any active manager
                    var manager = await _db.Users
                        .Where(u => u.RoleId == RoleIds.Manager && u.IsActive && u.DeletedAt == null)
                        .FirstOrDefaultAsync();
                    approverUserId = manager?.UserId;
                }
            }
            else // Admin
            {
                // Find any active admin
                var admin = await _db.Users
                    .Where(u => u.RoleId == RoleIds.Admin && u.IsActive && u.DeletedAt == null)
                    .FirstOrDefaultAsync();
                approverUserId = admin?.UserId;
            }

            // Create new approval record
            var newApproval = new DiscountApproval
            {
                ApprovalId = Guid.NewGuid(),
                QuotationId = originalApproval.QuotationId,
                RequestedByUserId = command.ResubmittedByUserId,
                ApproverUserId = approverUserId,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTimeOffset.UtcNow,
                CurrentDiscountPercentage = currentDiscountPercentage,
                Threshold = threshold,
                ApprovalLevel = approvalLevel,
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                EscalatedToAdmin = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.DiscountApprovals.Add(newApproval);

            // Lock quotation
            originalApproval.Quotation.LockForApproval(newApproval.ApprovalId);

            await _db.SaveChangesAsync();

            // Publish domain event
            var domainEvent = new DiscountApprovalResubmitted
            {
                NewApprovalId = newApproval.ApprovalId,
                PreviousApprovalId = originalApproval.ApprovalId,
                QuotationId = originalApproval.QuotationId,
                ResubmittedByUserId = command.ResubmittedByUserId,
                Reason = command.Request.Reason,
                Comments = command.Request.Comments,
                ResubmissionDate = newApproval.RequestDate
            };
            _ = domainEvent;

            // Reload with navigation properties for mapping
            newApproval = await _db.DiscountApprovals
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(a => a.RequestedByUser)
                .Include(a => a.ApproverUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == newApproval.ApprovalId);

            var result = _mapper.Map<DiscountApprovalDto>(newApproval);
            
            _logger.LogInformation("Discount approval {NewApprovalId} resubmitted successfully (previous: {PreviousApprovalId})", 
                newApproval.ApprovalId, originalApproval.ApprovalId);

            return result;
        }
    }
}

