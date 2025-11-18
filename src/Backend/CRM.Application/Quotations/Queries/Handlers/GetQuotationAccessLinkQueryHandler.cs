using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationAccessLinkQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly QuotationManagementSettings _settings;

        public GetQuotationAccessLinkQueryHandler(IAppDbContext db, IOptions<QuotationManagementSettings> settings)
        {
            _db = db;
            _settings = settings.Value;
        }

        public async Task<QuotationAccessLinkDto?> Handle(GetQuotationAccessLinkQuery query)
        {
            var quotation = await _db.Quotations
                .AsNoTracking()
                .Select(q => new { q.QuotationId, q.CreatedByUserId })
                .FirstOrDefaultAsync(q => q.QuotationId == query.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(query.QuotationId);
            }

            EnsureAuthorized(query.RequestorUserId, query.RequestorRole, quotation.CreatedByUserId);

            var link = await _db.QuotationAccessLinks
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync(l => l.QuotationId == query.QuotationId);

            if (link == null)
            {
                return null;
            }

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

        private static void EnsureAuthorized(Guid userId, string role, Guid ownerId)
        {
            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && ownerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this quotation.");
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


