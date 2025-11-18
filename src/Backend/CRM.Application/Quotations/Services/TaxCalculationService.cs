using System;
using CRM.Shared.Config;
using Microsoft.Extensions.Options;

namespace CRM.Application.Quotations.Services
{
    public class TaxCalculationService
    {
        private readonly CompanySettings _companySettings;
        private const decimal TaxRate = 0.18m; // 18% GST
        private const decimal IntraStateCgstRate = 0.09m; // 9% CGST
        private const decimal IntraStateSgstRate = 0.09m; // 9% SGST

        public TaxCalculationService(IOptions<CompanySettings> companySettings)
        {
            _companySettings = companySettings.Value;
        }

        public TaxCalculationResult CalculateTax(decimal subtotal, decimal discountAmount, string? clientStateCode)
        {
            var taxableAmount = subtotal - discountAmount;

            if (string.IsNullOrWhiteSpace(clientStateCode))
            {
                // Default to inter-state if state code not provided
                return CalculateInterStateTax(taxableAmount);
            }

            // Handle null or empty company state code
            var companyStateCode = _companySettings?.StateCode;
            if (string.IsNullOrWhiteSpace(companyStateCode))
            {
                // Default to inter-state if company state code not configured
                return CalculateInterStateTax(taxableAmount);
            }

            var isIntraState = string.Equals(
                clientStateCode.Trim(),
                companyStateCode.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (isIntraState)
            {
                return CalculateIntraStateTax(taxableAmount);
            }
            else
            {
                return CalculateInterStateTax(taxableAmount);
            }
        }

        private TaxCalculationResult CalculateIntraStateTax(decimal taxableAmount)
        {
            var cgst = taxableAmount * IntraStateCgstRate;
            var sgst = taxableAmount * IntraStateSgstRate;
            var totalTax = cgst + sgst;

            return new TaxCalculationResult
            {
                CgstAmount = cgst,
                SgstAmount = sgst,
                IgstAmount = 0,
                TotalTax = totalTax
            };
        }

        private TaxCalculationResult CalculateInterStateTax(decimal taxableAmount)
        {
            var igst = taxableAmount * TaxRate;

            return new TaxCalculationResult
            {
                CgstAmount = 0,
                SgstAmount = 0,
                IgstAmount = igst,
                TotalTax = igst
            };
        }
    }

    public class TaxCalculationResult
    {
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalTax { get; set; }
    }
}

