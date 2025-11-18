using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.Commands.Handlers
{
    public class ApproveAdjustmentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ApproveAdjustmentCommandHandler> _logger;

        public ApproveAdjustmentCommandHandler(
            IAppDbContext db,
            ILogger<ApproveAdjustmentCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdjustmentDto> Handle(ApproveAdjustmentCommand command)
        {
            var adjustment = await _db.Adjustments
                .FirstOrDefaultAsync(a => a.AdjustmentId == command.AdjustmentId);

            if (adjustment == null)
                throw new InvalidOperationException("Adjustment not found");

            if (!adjustment.CanBeApproved())
                throw new InvalidOperationException("Adjustment cannot be approved in current status");

            // TODO: Validate approver permissions

            var approvalDate = DateTimeOffset.UtcNow;
            adjustment.MarkAsApproved(command.ApprovedByUserId, approvalDate);

            await _db.SaveChangesAsync();

            // Publish AdjustmentApproved event
            var adjustmentEvent = new AdjustmentApproved
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                ApprovedByUserId = command.ApprovedByUserId,
                ApprovalDate = approvalDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Adjustment {AdjustmentId} approved by user {UserId}",
                adjustment.AdjustmentId, command.ApprovedByUserId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(adjustment.RequestedByUserId);
            var approvedByUser = await _db.Users.FindAsync(command.ApprovedByUserId);
            return new AdjustmentDto
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                AdjustmentType = adjustment.AdjustmentType,
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                Reason = adjustment.Reason,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                ApprovedByUserName = approvedByUser != null ? $"{approvedByUser.FirstName} {approvedByUser.LastName}" : string.Empty,
                Status = adjustment.Status,
                ApprovalLevel = adjustment.ApprovalLevel,
                RequestDate = adjustment.RequestDate,
                ApprovalDate = adjustment.ApprovalDate,
                AdjustmentDifference = adjustment.GetAdjustmentDifference()
            };
        }
    }
}

