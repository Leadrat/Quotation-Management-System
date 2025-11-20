using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Services;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Queries.Handlers
{
    public class GetQuotationByAccessTokenQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICompanyDetailsService _companyDetailsService;
        private readonly ILogger<GetQuotationByAccessTokenQueryHandler> _logger;

        public GetQuotationByAccessTokenQueryHandler(
            IAppDbContext db, 
            IMapper mapper,
            ICompanyDetailsService companyDetailsService,
            ILogger<GetQuotationByAccessTokenQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _companyDetailsService = companyDetailsService;
            _logger = logger;
        }

        public async Task<PublicQuotationDto> Handle(GetQuotationByAccessTokenQuery query)
        {
            var link = await _db.QuotationAccessLinks
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(l => l.Quotation)
                    .ThenInclude(q => q.LineItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.QuotationId == query.QuotationId &&
                    l.AccessToken == query.AccessToken);

            if (link == null)
            {
                throw new QuotationAccessLinkNotFoundException();
            }

            if (!link.IsActive || link.IsExpired())
            {
                throw new InvalidOperationException("Access link is inactive or expired.");
            }

            if (link.Quotation == null)
            {
                throw new QuotationNotFoundException(query.QuotationId);
            }

            var dto = _mapper.Map<PublicQuotationDto>(link.Quotation);

            // Load company details from snapshot or service
            CompanyDetailsDto? companyDetails = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(link.Quotation.CompanyDetailsSnapshot))
                {
                    companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(link.Quotation.CompanyDetailsSnapshot);
                    _logger.LogInformation("Loaded company details from snapshot for quotation {QuotationId}", query.QuotationId);
                }
                else
                {
                    // Fallback to current company details if snapshot is not available
                    companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                    _logger.LogInformation("Loaded company details from service for quotation {QuotationId}", query.QuotationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load company details for quotation {QuotationId}", query.QuotationId);
                // Try to load from service as fallback
                try
                {
                    companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Failed to load company details from service as fallback for quotation {QuotationId}", query.QuotationId);
                }
            }

            dto.CompanyDetails = companyDetails;

            return dto;
        }
    }
}


