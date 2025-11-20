using System;
using System.Collections.Generic;
using CRM.Domain.Enums;
using CRM.Domain.Entities;

namespace CRM.Application.TaxManagement.Dtos
{
    public class TaxFrameworkDto
    {
        public Guid TaxFrameworkId { get; set; }
        public Guid CountryId { get; set; }
        public string FrameworkName { get; set; } = string.Empty;
        public TaxFrameworkType FrameworkType { get; set; }
        public string? Description { get; set; }
        public List<TaxComponent> TaxComponents { get; set; } = new List<TaxComponent>();
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

