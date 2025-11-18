using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Application.Quotations.Services
{
    public class QuotationSendWorkflow : IQuotationSendWorkflow
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationPdfGenerationService _pdfService;
        private readonly IQuotationEmailService _emailService;
        private readonly QuotationManagementSettings _settings;
        private readonly ILogger<QuotationSendWorkflow> _logger;

        public QuotationSendWorkflow(
            IAppDbContext db,
            IQuotationPdfGenerationService pdfService,
            IQuotationEmailService emailService,
            IOptions<QuotationManagementSettings> settings,
            ILogger<QuotationSendWorkflow> logger)
        {
            _db = db;
            _pdfService = pdfService;
            _emailService = emailService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<QuotationAccessLinkDto> ExecuteAsync(
            Quotation quotation,
            SendQuotationRequest request,
            Guid requestedByUserId,
            bool isResend)
        {
            if (quotation == null) throw new ArgumentNullException(nameof(quotation));
            if (request == null) throw new ArgumentNullException(nameof(request));

            await DisableExistingLinks(quotation.QuotationId);

            var now = DateTimeOffset.UtcNow;
            var token = AccessTokenGenerator.Generate();
            var expirationDays = _settings.AccessLinkExpirationDays <= 0 ? 90 : _settings.AccessLinkExpirationDays;

            var accessLink = new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ClientEmail = request.RecipientEmail,
                AccessToken = token,
                IsActive = true,
                CreatedAt = now,
                ExpiresAt = now.AddDays(expirationDays),
                SentAt = now,
                ViewCount = 0
            };

            try
            {
                _db.QuotationAccessLinks.Add(accessLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding access link for quotation {QuotationId}", quotation.QuotationId);
                throw new InvalidOperationException($"Failed to create access link: {ex.Message}", ex);
            }

            byte[] pdfBytes;
            try
            {
                pdfBytes = _pdfService.GenerateQuotationPdf(quotation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for quotation {QuotationId}", quotation.QuotationId);
                throw new InvalidOperationException($"Failed to generate quotation PDF: {ex.Message}", ex);
            }

            var viewUrl = BuildViewUrl(quotation.QuotationId, token);

            try
            {
                await _emailService.SendQuotationEmailAsync(
                    quotation,
                    request.RecipientEmail,
                    pdfBytes,
                    viewUrl,
                    request.CcEmails,
                    request.BccEmails,
                    request.CustomMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email for quotation {QuotationId} to {Email}", quotation.QuotationId, request.RecipientEmail);
                throw new InvalidOperationException($"Failed to send quotation email: {ex.Message}", ex);
            }

            var previousStatus = quotation.Status;
            quotation.Status = QuotationStatus.Sent;
            quotation.UpdatedAt = now;

            _db.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                PreviousStatus = previousStatus.ToString(),
                NewStatus = QuotationStatus.Sent.ToString(),
                ChangedByUserId = requestedByUserId,
                ChangedAt = now,
                Reason = isResend ? "Quotation resent to client" : "Quotation sent to client"
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "";
                _logger.LogError(dbEx, "Database error saving quotation status. Error: {Error}", innerMessage);
                throw new InvalidOperationException($"Failed to save quotation status: {innerMessage}", dbEx);
            }

            _logger.LogInformation("Quotation {QuotationId} sent to {Email} (resend={IsResend})",
                quotation.QuotationId, request.RecipientEmail, isResend);

            return new QuotationAccessLinkDto
            {
                AccessLinkId = accessLink.AccessLinkId,
                QuotationId = accessLink.QuotationId,
                ClientEmail = accessLink.ClientEmail,
                ViewUrl = viewUrl,
                IsActive = accessLink.IsActive,
                CreatedAt = accessLink.CreatedAt,
                ExpiresAt = accessLink.ExpiresAt,
                SentAt = accessLink.SentAt,
                FirstViewedAt = accessLink.FirstViewedAt,
                LastViewedAt = accessLink.LastViewedAt,
                ViewCount = accessLink.ViewCount,
                IpAddress = accessLink.IpAddress
            };
        }

        private async Task DisableExistingLinks(Guid quotationId)
        {
            var existing = await _db.QuotationAccessLinks
                .Where(x => x.QuotationId == quotationId && x.IsActive)
                .ToListAsync();

            foreach (var link in existing)
            {
                link.IsActive = false;
            }
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


