using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Services
{
    public class QuotationReminderService
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationEmailService _emailService;
        private readonly ILogger<QuotationReminderService> _logger;

        public QuotationReminderService(
            IAppDbContext db,
            IQuotationEmailService emailService,
            ILogger<QuotationReminderService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<int> SendUnviewedRemindersAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var threshold = now.AddDays(-3);

            var query = await _db.QuotationAccessLinks
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.CreatedByUser)
                .Where(l =>
                    l.Quotation.Status == QuotationStatus.Sent &&
                    l.SentAt != null &&
                    l.SentAt <= threshold &&
                    l.FirstViewedAt == null)
                .OrderByDescending(l => l.SentAt ?? l.CreatedAt)
                .ToListAsync(cancellationToken);

            var latestPerQuotation = query
                .GroupBy(l => l.QuotationId)
                .Select(g => g.First())
                .ToList();

            var count = 0;

            foreach (var link in latestPerQuotation)
            {
                var salesRepEmail = link.Quotation?.CreatedByUser?.Email;
                if (string.IsNullOrWhiteSpace(salesRepEmail) || link.Quotation == null || link.SentAt == null)
                {
                    continue;
                }

                await _emailService.SendUnviewedQuotationReminderAsync(
                    link.Quotation,
                    salesRepEmail,
                    link.SentAt.Value);
                count++;
            }

            if (count > 0)
            {
                _logger.LogInformation("Sent {Count} unviewed quotation reminders", count);
            }

            return count;
        }

        public async Task<int> SendPendingResponseFollowUpsAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var threshold = now.AddDays(-7);

            var candidates = await _db.QuotationAccessLinks
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.CreatedByUser)
                .Where(l =>
                    l.FirstViewedAt != null &&
                    l.FirstViewedAt <= threshold &&
                    (l.Quotation.Status == QuotationStatus.Viewed || l.Quotation.Status == QuotationStatus.Sent) &&
                    !_db.QuotationResponses.Any(r => r.QuotationId == l.QuotationId))
                .OrderByDescending(l => l.FirstViewedAt)
                .ToListAsync(cancellationToken);

            var latestPerQuotation = candidates
                .GroupBy(l => l.QuotationId)
                .Select(g => g.First())
                .ToList();

            var count = 0;

            foreach (var link in latestPerQuotation)
            {
                var salesRepEmail = link.Quotation?.CreatedByUser?.Email;
                if (string.IsNullOrWhiteSpace(salesRepEmail) || link.Quotation == null || !link.FirstViewedAt.HasValue)
                {
                    continue;
                }

                await _emailService.SendPendingResponseFollowUpAsync(
                    link.Quotation,
                    salesRepEmail,
                    link.FirstViewedAt.Value);
                count++;
            }

            if (count > 0)
            {
                _logger.LogInformation("Sent {Count} pending response follow-ups", count);
            }

            return count;
        }
    }
}


