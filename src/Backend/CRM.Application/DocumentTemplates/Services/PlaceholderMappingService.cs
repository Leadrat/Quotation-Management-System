using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Services;
using CRM.Application.CompanyBankDetails.Queries;
using CRM.Application.CompanyIdentifiers.Queries;
using CRM.Application.Quotations.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DocumentTemplates.Services
{
    public class PlaceholderMappingService : IPlaceholderMappingService
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyDetailsService _companyDetailsService;
        private readonly ILogger<PlaceholderMappingService> _logger;

        public PlaceholderMappingService(
            IAppDbContext db,
            ICompanyDetailsService companyDetailsService,
            ILogger<PlaceholderMappingService> logger)
        {
            _db = db;
            _companyDetailsService = companyDetailsService;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> MapPlaceholdersToDataAsync(
            Guid templateId,
            Guid quotationId,
            CancellationToken cancellationToken = default)
        {
            var mapping = new Dictionary<string, string>();

            // Get quotation with client
            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId, cancellationToken);

            if (quotation == null)
            {
                throw new InvalidOperationException($"Quotation {quotationId} not found");
            }

            // Get company details
            var companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();

            // Map company placeholders
            if (companyDetails != null)
            {
                mapping["CompanyName"] = companyDetails.CompanyName ?? string.Empty;
                mapping["CompanyAddress"] = companyDetails.CompanyAddress ?? string.Empty;
                mapping["CompanyCity"] = companyDetails.City ?? string.Empty;
                mapping["CompanyState"] = companyDetails.State ?? string.Empty;
                mapping["CompanyPostalCode"] = companyDetails.PostalCode ?? string.Empty;
                mapping["CompanyPhone"] = companyDetails.ContactPhone ?? string.Empty;
                mapping["CompanyEmail"] = companyDetails.ContactEmail ?? string.Empty;
                mapping["CompanyGST"] = companyDetails.GstNumber ?? string.Empty;
            }

            // Map customer placeholders
            if (quotation.Client != null)
            {
                mapping["CustomerCompanyName"] = quotation.Client.CompanyName ?? string.Empty;
                mapping["CustomerAddress"] = quotation.Client.Address ?? string.Empty;
                mapping["CustomerCity"] = quotation.Client.City ?? string.Empty;
                mapping["CustomerState"] = quotation.Client.State ?? string.Empty;
                mapping["CustomerPostalCode"] = quotation.Client.PinCode ?? string.Empty;
                mapping["CustomerGSTIN"] = quotation.Client.Gstin ?? string.Empty;
            }

            // Map quotation-specific placeholders
            mapping["QuotationNumber"] = quotation.QuotationNumber ?? string.Empty;
            mapping["QuotationDate"] = quotation.CreatedAt.ToString("dd/MM/yyyy");
            mapping["QuotationValidUntil"] = quotation.ValidUntil.ToString("dd/MM/yyyy");
            mapping["QuotationSubTotal"] = quotation.SubTotal.ToString("N2");
            mapping["QuotationTaxAmount"] = quotation.TaxAmount.ToString("N2");
            mapping["QuotationTotalAmount"] = quotation.TotalAmount.ToString("N2");

            return mapping;
        }
    }
}

