using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.Commands.Handlers
{
    public class RejectAdjustmentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<RejectAdjustmentCommandHandler> _logger;

        public RejectAdjustmentCommandHandler(
            IAppDbContext db,
            ILogger<RejectAdjustmentCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdjustmentDto> Handle(RejectAdjustmentCommand command)
        {
            var adjustment = await _db.Adjustments
                .FirstOrDefaultAsync(a => a.AdjustmentId == command.AdjustmentId);

            if (adjustment == null)
                throw new InvalidOperationException("Adjustment not found");

            if (!adjustment.CanBeRejected())
                throw new InvalidOperationException("Adjustment cannot be rejected in current status");

            adjustment.MarkAsRejected();
            await _db.SaveChangesAsync();

            _logger.LogInformation("Adjustment {AdjustmentId} rejected by user {UserId}",
                adjustment.AdjustmentId, command.RejectedByUserId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(adjustment.RequestedByUserId);
            return new AdjustmentDto
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                AdjustmentType = adjustment.AdjustmentType,
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                Reason = adjustment.Reason,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                Status = adjustment.Status,
                RequestDate = adjustment.RequestDate,
                AdjustmentDifference = adjustment.GetAdjustmentDifference()
            };
        }
    }
}

