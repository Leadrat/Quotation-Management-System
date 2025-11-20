using System.Collections.Generic;
using CRM.Domain.Enums;
using CRM.Domain.Entities;

namespace CRM.Application.TaxManagement.Requests
{
    public class UpdateTaxFrameworkRequest
    {
        public string? FrameworkName { get; set; }
        public TaxFrameworkType? FrameworkType { get; set; }
        public string? Description { get; set; }
        public List<TaxComponent>? TaxComponents { get; set; }
        public bool? IsActive { get; set; }
    }
}

