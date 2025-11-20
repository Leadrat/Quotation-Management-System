using System;
using CRM.Application.CompanyDetails.Dtos;

namespace CRM.Application.Quotations.Dtos
{
    public class PublicQuotationDto
    {
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsExpired { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientMobile { get; set; }
        public string? ClientPhoneCode { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientCity { get; set; }
        public string? ClientState { get; set; }
        public string? ClientPinCode { get; set; }
        public string? ClientGstin { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<LineItemDto> LineItems { get; set; } = new();
        public string? Notes { get; set; }
        public CompanyDetailsDto? CompanyDetails { get; set; }
    }
}
