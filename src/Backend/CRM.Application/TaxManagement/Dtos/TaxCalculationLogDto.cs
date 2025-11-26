using System;
using System.Collections.Generic;
using CRM.Domain.Enums;

namespace CRM.Application.TaxManagement.Dtos
{
    public class TaxCalculationLogDto
    {
        public Guid LogId { get; set; }
        public Guid? QuotationId { get; set; }
        public TaxCalculationActionType ActionType { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public Dictionary<string, object> CalculationDetails { get; set; } = new();
        public Guid ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? CountryName { get; set; }
        public string? JurisdictionName { get; set; }
    }
}

