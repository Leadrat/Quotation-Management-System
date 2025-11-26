using System;
using CRM.Application.CompanyDetails.Dtos;

namespace CRM.Application.CompanyDetails.Commands
{
    public class UpdateCompanyDetailsCommand
    {
        public UpdateCompanyDetailsRequest Request { get; set; } = new();
        public Guid UpdatedBy { get; set; }
        public string? IpAddress { get; set; }
    }
}

