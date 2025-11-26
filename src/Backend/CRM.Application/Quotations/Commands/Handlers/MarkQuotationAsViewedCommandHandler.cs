using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class MarkQuotationAsViewedCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<MarkQuotationAsViewedCommandHandler> _logger;
        private readonly QuotationManagementSettings _settings;

        public MarkQuotationAsViewedCommandHandler(
            IAppDbContext db,
            IOptions<QuotationManagementSettings> settings,
            ILogger<MarkQuotationAsViewedCommandHandler> logger)
        {
            _db = db;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<QuotationAccessLinkDto> Handle(MarkQuotationAsViewedCommand command)
        {
            var link = await _db.QuotationAccessLinks
                .Include(x => x.Quotation)
                .ThenInclude(q => q.LineItems)
                .FirstOrDefaultAsync(x => x.AccessToken == command.AccessToken);

            if (link == null)
            {
                throw new QuotationAccessLinkNotFoundException();
            }

            if (!link.IsActive || link.IsExpired())
            {
                throw new InvalidOperationException("Access link is inactive or expired.");
            }

            var quotation = link.Quotation ?? throw new QuotationNotFoundException(link.QuotationId);

            _logger.LogInformation("Marking quotation {QuotationId} as viewed via token {Token}", quotation.QuotationId, command.AccessToken);

            link.LastViewedAt = DateTimeOffset.UtcNow;
            link.ViewCount += 1;
            link.IpAddress = command.IpAddress ?? link.IpAddress;

            if (!link.FirstViewedAt.HasValue)
            {
                link.FirstViewedAt = link.LastViewedAt;
            }

            if (quotation.Status == QuotationStatus.Sent)
            {
                quotation.Status = QuotationStatus.Viewed;
                quotation.UpdatedAt = DateTimeOffset.UtcNow;

                _db.QuotationStatusHistory.Add(new QuotationStatusHistory
                {
                    HistoryId = Guid.NewGuid(),
                    QuotationId = quotation.QuotationId,
                    PreviousStatus = QuotationStatus.Sent.ToString(),
                    NewStatus = QuotationStatus.Viewed.ToString(),
                    ChangedAt = DateTimeOffset.UtcNow,
                    Reason = "Quotation viewed by client",
                    IpAddress = command.IpAddress
                });
            }

            var viewedEvent = new QuotationViewed
            {
                QuotationId = quotation.QuotationId,
                AccessLinkId = link.AccessLinkId,
                ViewCount = link.ViewCount,
                ViewedAt = link.LastViewedAt.Value,
                IpAddress = command.IpAddress
            };

            _ = viewedEvent;

            await _db.SaveChangesAsync();

            return new QuotationAccessLinkDto
            {
                AccessLinkId = link.AccessLinkId,
                QuotationId = link.QuotationId,
                ClientEmail = link.ClientEmail,
                ViewUrl = BuildViewUrl(link.QuotationId, link.AccessToken),
                IsActive = link.IsActive,
                CreatedAt = link.CreatedAt,
                ExpiresAt = link.ExpiresAt,
                SentAt = link.SentAt,
                FirstViewedAt = link.FirstViewedAt,
                LastViewedAt = link.LastViewedAt,
                ViewCount = link.ViewCount,
                IpAddress = link.IpAddress
            };
        }

        private string BuildViewUrl(Guid quotationId, string token)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
                ? "https://crm.example.com"
                : _settings.BaseUrl;

            return $"{baseUrl.TrimEnd('/')}/client-portal/quotations/{quotationId}/{token}";
        }
    }
}

 
