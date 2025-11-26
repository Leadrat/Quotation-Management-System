using System;
using System.Collections.Generic;
using CRM.Domain.Enums;
using CRM.Domain.Entities;

namespace CRM.Application.TaxManagement.Commands
{
    public class UpdateTaxFrameworkCommand
    {
        public Guid TaxFrameworkId { get; set; }
        public string? FrameworkName { get; set; }
        public TaxFrameworkType? FrameworkType { get; set; }
        public string? Description { get; set; }
        public List<TaxComponent>? TaxComponents { get; set; }
        public bool? IsActive { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}

