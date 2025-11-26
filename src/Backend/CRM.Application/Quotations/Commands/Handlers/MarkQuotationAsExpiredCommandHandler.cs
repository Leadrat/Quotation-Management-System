using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class MarkQuotationAsExpiredCommandHandler
    {
        private readonly IAppDbContext _db;

        public MarkQuotationAsExpiredCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(MarkQuotationAsExpiredCommand command)
        {
            var quotation = await _db.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == command.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(command.QuotationId);
            }

            if (quotation.Status is QuotationStatus.Accepted or QuotationStatus.Rejected or QuotationStatus.Cancelled)
            {
                return;
            }

            if (quotation.Status == QuotationStatus.Expired)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var previousStatus = quotation.Status;
            quotation.Status = QuotationStatus.Expired;
            quotation.UpdatedAt = now;

            _db.QuotationStatusHistory.Add(new Domain.Entities.QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                PreviousStatus = previousStatus.ToString(),
                NewStatus = QuotationStatus.Expired.ToString(),
                ChangedAt = now,
                Reason = command.Reason ?? "Quotation automatically marked as expired."
            });

            await _db.SaveChangesAsync();
        }
    }
}

 
